void Lux_SimpleTranslucentLighting_half(
   float3   PositionWS,
   half3    NormalWS,
   half3    TangentWS,
   half3    BitangentWS,

   half3    Albedo,
   half3    NormalTS,
   half3    Transmission,
   half     Thickness,

   half     VertexOcclusion,

   bool     IsFrontFace,
   bool     FlipBackfaceNormals,

   bool EnableTransmission,

   out half3 o_NormalWS,
   out half3 o_Transmission
)
{
      o_Transmission = 0;

   #ifdef SHADERGRAPH_PREVIEW
      o_NormalWS = half3(0,1,0);
   #else

   // Fix backface normals if selected 
      NormalTS.z = (!IsFrontFace && FlipBackfaceNormals) ? -NormalTS.z : NormalTS.z;

      half3x3 tangentToWorld = half3x3(TangentWS, BitangentWS, NormalWS);
      o_NormalWS = NormalizeNormalPerPixel(TransformTangentToWorld(NormalTS, tangentToWorld));

      if (_EnableTransmission)
      {
         half4 shadowCoord = TransformWorldToShadowCoord(PositionWS);
         Light mainLight = GetMainLight(shadowCoord);

         half w = 0.3; // 0.4
         half NdotL = saturate((dot(o_NormalWS, mainLight.direction) + w) / ((1 + w) * (1 + w)));

         half3 viewDirWS = GetWorldSpaceNormalizeViewDir(PositionWS);
         half3 transLightDir = mainLight.direction + o_NormalWS * Transmission.z;
         half transDot = dot( transLightDir, -viewDirWS );
         transDot = exp2(saturate(transDot) * Transmission.y - Transmission.y);
         o_Transmission = transDot * (1.0 - NdotL) * mainLight.color * mainLight.shadowAttenuation * Transmission.x;

         if(_AmbientTranslucency > 0)
         {
            o_Transmission += SampleSH(-o_NormalWS) * _AmbientTranslucency * VertexOcclusion;
         }

         o_Transmission *= Albedo * saturate(1.0 - Thickness);
      }

   #endif
}

void SampleSH(half3 normalWS, out half3 Ambient)
{
    // LPPV is not supported in Ligthweight Pipeline
    real4 SHCoefficients[7];
    SHCoefficients[0] = unity_SHAr;
    SHCoefficients[1] = unity_SHAg;
    SHCoefficients[2] = unity_SHAb;
    SHCoefficients[3] = unity_SHBr;
    SHCoefficients[4] = unity_SHBg;
    SHCoefficients[5] = unity_SHBb;
    SHCoefficients[6] = unity_SHC;

    Ambient = max(half3(0, 0, 0), SampleSH9(SHCoefficients, normalWS));
}