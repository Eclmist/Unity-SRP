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
            
            #pragma target 5.0

            // Creates shader permutations for instancing
            #pragma multi_compile_instancing

            // Don't support non-uniform scale instancing for now
            #pragma instancing_options assumeuniformscaling

            #pragma vertex VS_Unlit
            #pragma fragment PS_Unlit

            #include "../ShaderLibrary/Unlit.hlsl"

            ENDHLSL
        }
    }
}