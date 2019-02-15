#ifndef MYST_UNLIT_INCLUDED
#define MYST_UNLIT_INCLUDED

float4x4 unity_MatrixVP;
float4x4 unity_ObjectToWorld;

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