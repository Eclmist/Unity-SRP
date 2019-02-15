#ifndef MYST_UNLIT_INCLUDED
#define MYST_UNLIT_INCLUDED

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"

CBUFFER_START(UnityPerFrame)
    float4x4 unity_MatrixVP;
CBUFFER_END

CBUFFER_START(UnityPerDraw)
    float4x4 unity_ObjectToWorld;
CBUFFER_END

struct VS_INPUT
{
    float4 pos : POSITION;
};

struct VS_OUTPUT
{
    float4 clipPos : SV_POSITION;
};

VS_OUTPUT VS_Unlit(const VS_INPUT v)
{
    VS_OUTPUT output;
    float4 worldPos = mul(unity_ObjectToWorld, float4(v.pos.xyz, 1.0));
    output.clipPos = mul(unity_MatrixVP, worldPos);

    return output;
}

float4 PS_Unlit(const VS_OUTPUT input) : SV_TARGET
{
    return 1;
}

#endif // MYST_UNLIT_INCLUDED