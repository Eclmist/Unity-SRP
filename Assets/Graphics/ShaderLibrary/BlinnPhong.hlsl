#ifndef MYST_BLINNPHONG_INCLUDED
#define MYST_BLINNPHONG_INCLUDED

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"

CBUFFER_START(UnityPerFrame)
    float4x4 unity_MatrixVP;
CBUFFER_END

CBUFFER_START(UnityPerDraw)
    float4x4 unity_ObjectToWorld;
CBUFFER_END

#define MAX_VISIBLE_LIGHTS 8

CBUFFER_START(_LightBuffer)
    float4 _VisibleLightColors[MAX_VISIBLE_LIGHTS];
    float4 _VisibleLightDirections[MAX_VISIBLE_LIGHTS];
CBUFFER_END

float3 DiffuseLight(int index, float3 normal)
{
    float3 lightColor = _VisibleLightColors[index].rgb;
    float3 lightDirection = _VisibleLightDirections[index].rgb;
    float diffuse = saturate(dot(normal, lightDirection));
    return diffuse * lightColor;
}

#define UNITY_MATRIX_M unity_ObjectToWorld
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl" // Overwrites model matrix if instancing enabled

UNITY_INSTANCING_BUFFER_START(PerInstance)
    UNITY_DEFINE_INSTANCED_PROP(float4, _Color)
UNITY_INSTANCING_BUFFER_END(PerInstance)


struct VS_INPUT
{
    float4 pos : POSITION;
    float3 normal : NORMAL;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct VS_OUTPUT
{
    float4 clipPos : SV_POSITION;
    float3 normal : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

VS_OUTPUT VS_BlinnPhong(const VS_INPUT v)
{
    VS_OUTPUT output;

    // Sets up instance matrices if any
    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_TRANSFER_INSTANCE_ID(v, output);

    float4 worldPos = mul(UNITY_MATRIX_M, float4(v.pos.xyz, 1.0));
    output.clipPos = mul(unity_MatrixVP, worldPos);
    output.normal = mul((float3x3)UNITY_MATRIX_M, v.normal);

    return output;
}

float4 PS_BlinnPhong(const VS_OUTPUT input) : SV_TARGET
{
    UNITY_SETUP_INSTANCE_ID(input);

    float3 albedo = UNITY_ACCESS_INSTANCED_PROP(PerInstance, _Color).rgb;
    float3 normal = normalize(input.normal);

    float3 diffuseLight = 0;

    for (int i = 0; i < MAX_VISIBLE_LIGHTS; i++)
    {
        diffuseLight += DiffuseLight(i, input.normal);
    }

    float3 color = diffuseLight * albedo;
    return float4(color, 1);

}

#endif // MYST_BLINNPHONG_INCLUDED