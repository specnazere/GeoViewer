cbuffer ShaderConstsInput : register(b1)
{
    float4x4 WorldViewProj;
    float4x4 ModelRotationTransformation;
    float4x4 ModelFullTransformation;
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
    float4 nor : NORMAL;
    float4 worldPos : TEXCOORD0; // Мировые координаты вершины
    float3 normal : TEXCOORD1; // Нормаль в мировом пространстве
};

PS_IN VS(VS_IN input)
{
    PS_IN output = (PS_IN) 0;

    output.pos = mul(input.pos, WorldViewProj);
    output.col = input.col;
    output.worldPos = mul(input.pos, ModelFullTransformation); // Переводим в мировые координаты
    output.normal = normalize(mul(input.nor, (float3x3) ModelRotationTransformation)); // Нормаль в мировом пространстве

    return output;
}