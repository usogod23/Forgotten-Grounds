//  ////////////////////////////////////////////////////
//  Global inputs

float   _LuxInstantWindEnabled;
float4  _LuxInstantWind;             // wind direction (xyz), wind strength (w)
float   _LuxInstantTurbulence;       // turbulence
float2  _LuxInstantWindFadeParams;   // distanceSqr (x), 1.0 / distanceSqr (y)

float4 SmoothCurve(float4 x) {
    return x * x * (3.0 - 2.0 * x);
}
float4 TriangleWave(float4 x) {
    return abs(frac(x + 0.5) * 2.0 - 1.0);
}
float4 SmoothTriangleWave(float4 x) {
    return SmoothCurve(TriangleWave(x));
}

float2 SmoothCurve(float2 x) {
    return x * x * (3.0 - 2.0 * x);
}
float2 TriangleWave(float2 x) {
    return abs(frac(x + 0.5) * 2.0 - 1.0);
}
float2 SmoothTriangleWave(float2 x) {
    return SmoothCurve(TriangleWave(x));
}

float SmoothCurve(float x) {
    return x * x * (3.0 - 2.0 * x);
}
float TriangleWave(float x) {
    return abs(frac(x + 0.5) * 2.0 - 1.0);
}
float SmoothTriangleWave(float x) {
    return SmoothCurve(TriangleWave(x));
}

float3x3 GetRotationAroundAxis(float3 axis, float angle)
{
    //axis = normalize(axis); // moved to calling function
    float s; // = sin(angle);
    float c; // = cos(angle);
    sincos(angle, s, c);
    float oc = 1.0f - c;

    return float3x3 (   oc * axis.x * axis.x + c,           oc * axis.x * axis.y - axis.z * s,  oc * axis.z * axis.x + axis.y * s,
                        oc * axis.x * axis.y + axis.z * s,  oc * axis.y * axis.y + c,           oc * axis.y * axis.z - axis.x * s,
                        oc * axis.z * axis.x - axis.y * s,  oc * axis.y * axis.z + axis.x * s,  oc * axis.z * axis.z + c);   
}

float3 RotateAboutAxisOptimized(float4 NormalizedRotationAxisAndAngle, float3 Position)
{
    float3 ClosestPointOnAxis = NormalizedRotationAxisAndAngle.xyz * dot(NormalizedRotationAxisAndAngle.xyz, Position);
    float3 UAxis = Position - ClosestPointOnAxis;
    float3 VAxis = cross(NormalizedRotationAxisAndAngle.xyz, UAxis);
    float CosAngle;
    float SinAngle;
    sincos(NormalizedRotationAxisAndAngle.w, SinAngle, CosAngle);
    float3 R = UAxis * CosAngle + VAxis * SinAngle;
    return ClosestPointOnAxis + R;
}

float2 DecodeFloat (float enc)
{
    float2 result ;
    result.y = enc % 2048.0;
    result.x = floor(enc * rcp(2048.0));
    return result * rcp(2048.0 - 1.0);
}


void InstantWind_float(

    float Time,
    float3 PositionOS,
    float3 NormalOS,
    float3 BitangentOS,
    float4 VertexColor,
    float4 UV0,

    out float3 o_positionOS,
    out real2 o_AO_ColorVar,
    out float4 o_DebugData
)
{

    o_positionOS = PositionOS;
    o_DebugData = 1.0;

    float3 treePositionAWS = GetAbsolutePositionWS(UNITY_MATRIX_M._m03_m13_m23);
    //float colorVar = saturate((frac(treePositionAWS.x + treePositionAWS.y + treePositionAWS.z) + frac((treePositionAWS.x + treePositionAWS.y + treePositionAWS.z) * 3.3)) * 0.5);
//  12/2024 - to better match Speedtree. Here we might also take scale/rotation into account but do not do right now due to combined billboards. 
    float colorVar = frac(treePositionAWS.x + treePositionAWS.y + treePositionAWS.z); // + UNITY_MATRIX_M._m00 * 3.33);    

    o_AO_ColorVar = real2(lerp(1.0, VertexColor[_VertexAOChannel], _VertexAO), colorVar);

//  Fade wind over distance
    float3 dist = (treePositionAWS - _WorldSpaceCameraPos);
    float distSqr = dot(dist, dist);
    float windFade = saturate( (_LuxInstantWindFadeParams.x - distSqr) * _LuxInstantWindFadeParams.y);

//  Wind zone and script might be disabled or missing
    if (!_LuxInstantWindEnabled || windFade == 0.0)
    {
        return;
    }

    float3 windDir = TransformWorldToObjectDir(_LuxInstantWind.xyz);
    float mainWindStrength = _LuxInstantWind.w * windFade;
    float windTurbulence = _LuxInstantTurbulence * windFade;

//  Animate incoming wind
    float fObjPhase = dot(treePositionAWS, 4.0) * 0.193;
    float2 windParams = float2(fObjPhase, Time * 0.075 * _MainBendingSpeed);

    float vWavesIn = windParams.x + windParams.y;
    // 1.975, 0.793, 0.375, 0.193 are good frequencies
    float4 vWaves = (frac(vWavesIn.xxxx * float4(1.975, 0.793, 0.375, 0.193)) * 2.0 - 1.0);
    vWaves = SmoothTriangleWave(vWaves);
    float2 vWavesSum = vWaves.xz + vWaves.yw;

    float dotPosXZ;
    float3 posOSscaled = PositionOS.xyz;
    if (!_EnableBakedWindMask)
    {
        posOSscaled *= _MaskScale;
        posOSscaled.y *= _UseHemisphere;
    }
    dotPosXZ = dot(posOSscaled.xyz, posOSscaled.xyz);

    float perVertexRandom = 0.0;

//  Branch Wind
    if (_EnableBranchWind)
    {

    //  Directional mask
        float facingWind = 1.0;
        if (_RollingDirectionalMask > 0.0)
        {
            float2 normalizedPositionOS_XZ = normalize(PositionOS.xz);
            facingWind = dot(normalizedPositionOS_XZ, -normalize(windDir.xz) ) * 0.5 + 0.5;
            facingWind = lerp(1.0, facingWind, _RollingDirectionalMask); 
        }

        float branchWindSpeed = _BranchWindSpeed;

    //  Get Phase
        float phase = 0.0;
        if (_EnableBakedPhase)
        {
            perVertexRandom += VertexColor[_PhaseChannel] * _PhaseScale;
            phase += perVertexRandom + facingWind * perVertexRandom;
            // branchWindSpeed += perVertexRandom * 0.25;
        }
        if (_EnableProceduralPhase) 
        {
    //      Just a simple triangle wave in 3D
            float3 trigWave = abs(frac(PositionOS.xyz * _ProceduralTiling + 0.5));
            trigWave.x = (dot(trigWave, trigWave) / 3.0 );
            perVertexRandom += (SmoothTriangleWave(trigWave.x) * 2.0 - 1.0) * _RollingProceduralStrength;
            phase += (perVertexRandom + facingWind * perVertexRandom) * _RollingProceduralStrength;
        }
    //  We skip mainWindStrength here to prevent branchBending from going nuts. Instead we factor in _LuxInstantTurbulence
        float branchBending = (SmoothTriangleWave(Time * branchWindSpeed + phase) * 2.0 - 1.0); // * mainWindStrength; 
    //  Get mask
        float windMask = _EnableBakedWindMask ? VertexColor[_MaskVCChannel] : pow( max(0.0, dotPosXZ - _WindMask), _WindMaskFalloff) * 0.1;
        branchBending *= windMask;
    //  Factor in "wind shadow"
        branchBending *= facingWind;
    //  Apply strength from incoming wind/turbulence
        // branchBending *= _LuxInstantTurbulence;
    //  12/2024 - Fixed: Fade added!
        branchBending *= windTurbulence;

        if (_Rolling != 0.0)
        {
            float rollingStrength = _Rolling * 0.1;
            float sinRollingAngle, cosRollingAngle;
            sincos(branchBending * ( rollingStrength * 0.5 + vWavesSum.x * rollingStrength), sinRollingAngle, cosRollingAngle);
            o_positionOS = (o_positionOS * cosRollingAngle) + (cross( normalize(float3(0.2, 0.7, 0.1) ), o_positionOS) * sinRollingAngle);
        }
        o_positionOS.y += branchBending * (_RollingDisplacement * 0.5 + vWavesSum.x * _RollingDisplacement);
    //  Set debug
        o_DebugData.ga = float2(phase, windMask);
    }

//  Edge Flutter
    if (_EnableFlutter * _FlutterStrength > 0.0)
    {
        float flutterMask = VertexColor[_FlutterMask];
        if (_TVEFlutterMask)
        {
            flutterMask = DecodeFloat(UV0.z).y;
        }
        float perVertexRandomFlutter = sin(_FlutterTiling * (dot(PositionOS.xyz, float3(1, 1, 1)) + UV0.x + UV0.y));
        o_positionOS.xyz +=
            (NormalOS.xyz * 0.7 + BitangentOS * 0.3)
            * SmoothTriangleWave(Time * (_FlutterSpeed - perVertexRandom * 0.25) + perVertexRandomFlutter + vWavesSum.x * 0.25)
            * flutterMask * mainWindStrength
            * (_FlutterStrength * 0.5 + vWavesSum.y * perVertexRandom * _FlutterStrength * 0.5)
        ;
    
        o_DebugData.b = perVertexRandomFlutter;
        o_DebugData.r = flutterMask;
    }

//  Main bending
    float rotationStrength = pow(max(0, PositionOS.y + dotPosXZ * _MainScaleXZ), _MainBendingPower) * vWavesSum.x * mainWindStrength * _MainBendingStrength * 0.01;
    
    float3x3 windRot;
    float3 rotationAxisM = -windDir; // left handed
    rotationAxisM = cross(rotationAxisM, float3(0, 1, 0) );
    rotationAxisM = normalize(rotationAxisM);
    // windRot = GetRotationAroundAxis(rotationAxisM, rotationStrength);
    // o_positionOS = mul(windRot, o_positionOS);
    o_positionOS = RotateAboutAxisOptimized(float4(rotationAxisM, rotationStrength), o_positionOS);
}