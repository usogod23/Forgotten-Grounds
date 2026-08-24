void ShadowFade_float(
    float alpha,
    out float o_alpha
)
{
    #if (SHADERPASS == SHADERPASS_SHADOWS)
        o_alpha = 1.0;
    #else 
        o_alpha = alpha; 
    #endif
}

void ShadowFade_half(
    half alpha,
    out half o_alpha
)
{
    #if (SHADERPASS == SHADERPASS_SHADOWS)
        o_alpha = 1.0;
    #else 
        o_alpha = alpha; 
    #endif
}

void ApplyColorVariation_float(
    float3 BaseColor,
    float VariationStrength,

    out float3 o_BaseColor
)
{ 
    o_BaseColor = lerp(BaseColor, (BaseColor + _HueVariation.rgb) * 0.5, VariationStrength * _HueVariation.a);
}

void ApplyColorVariation_half(
    half3 BaseColor,
    half VariationStrength,

    out half3 o_BaseColor
)
{ 
    o_BaseColor = lerp(BaseColor, (BaseColor + _HueVariation.rgb) * 0.5, VariationStrength * _HueVariation.a);
}

void ViewSpaceTransforms_half(
    float3 normalOS,
    float3 tangentOS,

    out float3 o_normalsVS,
    out float3 o_tangentVS
)
{
    o_normalsVS = TransformWorldToViewDir( TransformObjectToWorldDir(normalOS), true );
    o_tangentVS = TransformWorldToViewDir( TransformObjectToWorldDir(tangentOS), true );
}

void ConvertToNormalWS_float(
    float3 NormalVS,
    float3 TangentVS,
    float3 NormalTS,
    bool IsFrontFace,
    bool FlipBackFaceNormals,

    out float3 o_NormalWS
)
{
    if (FlipBackFaceNormals)
    {
        NormalTS.z *= IsFrontFace ? 1.0 : -1.0;
    } 

    float3 bitangentVS = cross(NormalVS, TangentVS) * GetOddNegativeScale();
    float3x3 tangentToVS = float3x3 (TangentVS, bitangentVS, NormalVS);
    float3 normalVS = mul(NormalTS, tangentToVS);
    
//  Now make it a "valid" normal and force it pointing towards the camera
    normalVS.z = abs(normalVS.z);

    o_NormalWS = TransformViewToWorldDir(normalVS);
}

// void ConvertToNormalWS_half(
//     float3 NormalVS,
//     float3 TangentVS,
//     float3 NormalTS,

//     bool IsFrontFace,

//     out float3 o_NormalWS
// )
// {
    
//     //NormalTS.z *= IsFrontFace ? 1.0 : -1.0;

//     float3 bitangentVS = cross(NormalVS, TangentVS) * GetOddNegativeScale();
//     float3x3 tangentToVS = float3x3 (TangentVS, bitangentVS, NormalVS);
//     float3 normalVS = normalize (mul(NormalTS, tangentToVS));
//     // Now "flip" the normal
//     //normalVS.z = abs(normalVS.z);

//     o_NormalWS = TransformViewToWorldDir(normalVS);

// }

// // From tangent to world space
// float3x3 tangentToWS = float3x3 (tangentVS, bitangentVS, normalVS);
// normalWS = normalize (mul ( normalTS, tangentToWS) );

// // From world to view space
// float3 normalInVS = TransformWorldToViewDir(normalWS);
// // Now "flip" the normal
// normalInVS.z = abs(normalInVS.z);

// // From view to world space again
// //float4x4 viewTranspose = transpose(UNITY_MATRIX_V);
// //normalWS = mul((float3x3)viewTranspose, normalInVS); 

// normalWS = mul((float3x3)_InvViewMatrix, normalInVS);

