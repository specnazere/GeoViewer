cbuffer ShaderConstsInput : register(b1)
{
    float4x4 WorldViewProj;
    float4x4 ModelRotationTransformation;
    float4x4 ModelFullTransformation;
    float3 OverlapingColor;
    float OverlapOpacity;
};

struct VS_IN
{
    float4 pos : POSITION;
    float4 col : COLOR;
    float4 nor : NORMAL;
};

struct PS_IN
{
    float4 pos : SV_POSITION;
    float4 col : COLOR;
    float4 worldPos : TEXCOORD0; // Мировые координаты вершины
    float3 nor : NORMAL;
};

PS_IN VS(VS_IN input)
{
    PS_IN output = (PS_IN) 0;

    if (OverlapOpacity == 1)
    {
        output.col = float4(OverlapingColor, 1.0);
    }
    else
    {
        output.col = input.col;
    }
    output.pos = mul(input.pos, WorldViewProj);
    output.worldPos = mul(input.pos, ModelFullTransformation); // Переводим в мировые координаты
    output.nor = mul((float3) input.nor, (float3x3) ModelRotationTransformation); // Нормаль в мировом пространстве

    return output;
}