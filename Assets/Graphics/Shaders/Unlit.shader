Shader "Myst/Unlit" {
    Properties 
    {
        _Color ("Color", Color) = (1, 1, 1, 1)
    }

    SubShader 
    {
        Pass 
        {
            HLSLPROGRAM
            
            #pragma target 5.1

            #pragma vertex VS_Unlit
            #pragma fragment PS_Unlit

            #include "../ShaderLibrary/Unlit.hlsl"

            ENDHLSL
        }
    }
}