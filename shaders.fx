struct PointLight
{
    float4 Position; // (x, y, z, 1.0) если позиция, (x, y, z, 0.0) если направление
    float4 Color; // RGB + интенсивность
};

struct ShaderMatricesInput
{
    float4x4 WorldViewProj;
    float4x4 ModelRotationTransformation;
    float4x4 ModelFullTransformation;
    float4x4 CameraRotationTransformation;
    float4x4 ProjectionTransformation;
    float3 CameraPosition;
    uint PointLights;
};
ShaderMatricesInput consts;

StructuredBuffer<PointLight> PointLightsBuffer : register(t0);

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

// Параметры материала
float4 ambientColor = float4(0.1, 0.1, 0.1, 1.0); // Амбиентный цвет
float specularPower = 32.0; // Степень блеска (shininess)

PS_IN VS(VS_IN input)
{
    PS_IN output = (PS_IN) 0;

    output.pos = mul(input.pos, consts.WorldViewProj);
    output.col = input.col;
    output.worldPos = mul(input.pos, consts.ModelFullTransformation); // Переводим в мировые координаты
    output.normal = normalize(mul(input.nor, (float3x3) consts.ModelRotationTransformation)); // Нормаль в мировом пространстве

    return output;
}

// Функция для вычисления затухания света
float CalculateAttenuation(float distance, float4 attenuation)
{
    return 1.0 / (attenuation.x + attenuation.y * distance + attenuation.z * distance * distance);
}

float4 PS(PS_IN input) : SV_Target
{
    float3 viewDir = normalize((float3)input.worldPos - consts.CameraPosition);
    float3 normal = normalize(input.normal);
    float4 result = float4(0.1, 0.1, 0.1, 1.0) * input.col; // Амбиентная составляющая

    for (uint i = 0; i < consts.PointLights; i++)
    {
        PointLight light = PointLightsBuffer[i];
        float3 lightDir;
        float attenuation = light.Color.w;

        if (light.Position.w == 0.0) // Направленный свет
        {
            lightDir = normalize(light.Position.xyz);
        }
        else // Точечный свет
        {
            lightDir = light.Position.xyz - input.worldPos.xyz;
            float distance = length(lightDir);
            lightDir = normalize(lightDir);
            attenuation /= distance * distance;
        }

        // Диффузная составляющая (Lambert)
        float diffuseFactor = max(dot(normal, lightDir), 0.0);
        float4 diffuse = diffuseFactor * light.Color * input.col;

        // Спекулярная составляющая (Phong)
        float3 reflectDir = reflect(lightDir, normal);
        float specularFactor = pow(max(dot(viewDir, reflectDir), 0.0), 100);
        float4 specular = specularFactor * light.Color;

        // Суммируем вклад света с учетом затухания
        result += (diffuse + specular) * attenuation;
    }

    return result;
}