cbuffer ShaderConstsInput : register(b0)
{
    float4 OverlapingColor;
};

struct PS_IN
{
    float4 pos : SV_POSITION;
    float4 col : COLOR;
    float4 nor : NORMAL;
    float4 worldPos : TEXCOORD0; // Мировые координаты вершины
    float3 normal : TEXCOORD1; // Нормаль в мировом пространстве
};

float4 PS_Overlap(PS_IN input) : SV_Target
{
    return OverlapingColor;
}