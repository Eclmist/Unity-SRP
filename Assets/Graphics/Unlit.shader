Shader "Myst/Unlit" {
    Properties {}

    SubShader 
    {
        Pass 
        {
            HLSLPROGRAM
            
            #pragma vertex VS_Unlit
            #pragma fragment PS_Unlit

            #include "Unlit.hlsl"

            ENDHLSL
        }
    }
}