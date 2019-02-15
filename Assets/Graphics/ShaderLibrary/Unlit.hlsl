#ifndef MYST_UNLIT_INCLUDED
#define MYST_UNLIT_INCLUDED

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"

CBUFFER_START(UnityPerFrame)
    float4x4 unity_MatrixVP;
CBUFFER_END

CBUFFER_START(UnityPerDraw)
    float4x4 unity_ObjectToWorld;
CBUFFER_END

#define UNITY_MATRIX_M unity_ObjectToWorld
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl" // Overwrites model matrix if instancing enabled

UNITY_INSTANCING_BUFFER_START(PerInstance)
    UNITY_DEFINE_INSTANCED_PROP(float4, _Color)
UNITY_INSTANCING_BUFFER_END(PerInstance)


struct VS_INPUT
{
    float4 pos : POSITION;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct VS_OUTPUT
{
    float4 clipPos : SV_POSITION;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

VS_OUTPUT VS_Unlit(const VS_INPUT v)
{
    VS_OUTPUT output;

    // Sets up instance matrices if any
    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_TRANSFER_INSTANCE_ID(v, output);

    float4 worldPos = mul(UNITY_MATRIX_M, float4(v.pos.xyz, 1.0));
    output.clipPos = mul(unity_MatrixVP, worldPos);

    return output;
}

float4 PS_Unlit(const VS_OUTPUT input) : SV_TARGET
{
    UNITY_SETUP_INSTANCE_ID(input);
    return UNITY_ACCESS_INSTANCED_PROP(PerInstance, _Color);
}

#endif // MYST_UNLIT_INCLUDED