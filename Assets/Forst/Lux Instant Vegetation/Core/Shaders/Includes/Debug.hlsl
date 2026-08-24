float3 getColor (float index)
{
    float3 vmask = (float3)1.0;
    if (index < 2.5) // skip alpha
    {
        if (index < .5)
        {
           vmask = float3(1, 0, 0); 
        }
        else if (index < 1.5)
        {
           vmask = float3(0, 1, 0); 
        }
        else
        {
           vmask = float3(0, 0, 1); 
        }
    }
    return vmask;
}


void Debug_float(
    float4 DebuInput,
    out float3 o_DebugColor
)
{ 
    #ifdef SHADERGRAPH_PREVIEW
        o_DebugColor = 0;
    #else
        o_DebugColor = 0;
        float3 vmask = (float3)1.0;

        if (_DebugWindMask)
        {
            o_DebugColor = (DebuInput.a).xxx; // mask
        }
        if (_DebugBranchPhase)
        {
            vmask = (float3)1.0;
            if (_EnableBakedPhase)
            {
                vmask = getColor(_PhaseChannel);
            }
            o_DebugColor = (DebuInput.g * 0.5 + 0.5).xxx * vmask.xyz; // phase  
        }
        if (_DebugFlutterPhase)
        {
            o_DebugColor = (DebuInput.b * 0.5 + 0.5).xxx; // phase edge
        }

        if (_DebugFlutterMask)
        {
            vmask = (float3)1.0;
            if (!_TVEFlutterMask)
            {
                vmask = getColor(_FlutterMask);
            }
            o_DebugColor = DebuInput.rrr * vmask.xyz; // phase    
        }
    #endif
}

void Debug_half(
    half4 DebuInput,
    out half3 o_DebugColor
)
{ 
    #ifdef SHADERGRAPH_PREVIEW
        o_DebugColor = 0;
    #else
        o_DebugColor = 0;
        half3 vmask = (half3)1.0;

        if (_DebugWindMask)
        {
            o_DebugColor = (DebuInput.a).xxx; // mask
        }
        if (_DebugBranchPhase)
        {
            vmask = (half3)1.0;
            if (_EnableBakedPhase)
            {
                vmask = getColor(_PhaseChannel);
            }
            o_DebugColor = (DebuInput.g * 0.5 + 0.5).xxx * vmask.xyz; // phase  
        }
        if (_DebugFlutterPhase)
        {
            o_DebugColor = (DebuInput.b * 0.5 + 0.5).xxx; // phase edge
        }

        if (_DebugFlutterMask)
        {
            vmask = (half3)1.0;
            if (!_TVEFlutterMask)
            {
                vmask = getColor(_FlutterMask);
            }
            o_DebugColor = DebuInput.rrr * vmask.xyz; // phase    
        }
    #endif
}