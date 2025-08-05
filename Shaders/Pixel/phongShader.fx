struct PointLight
{
    float4 Position; // (x, y, z, 1.0) если позиция, (x, y, z, 0.0) если направление
    float4 Color; // RGB + интенсивность
};

struct DirectionalLight
{
    float4 Direction; // (x, y, z, 1.0) если позиция, (x, y, z, 0.0) если направление
    float4 Color; // RGB + интенсивность
};

cbuffer ShaderConstsInput : register(b0)
{
    float4 AmbiendLighting;
    float3 CameraPosition;
    uint PointLights;
    uint DirectionalLights;
    float DiffusalComponent;
    float SpecularComponent;
    float SpecularDispersion;
};

StructuredBuffer<PointLight> PointLightsBuffer : register(t0);
StructuredBuffer<DirectionalLight> DirectionalLightsBuffer : register(t1);

struct PS_IN
{
    float4 pos : SV_POSITION;
    float4 col : COLOR;
    float4 worldPos : TEXCOORD0; // Мировые координаты вершины
    float3 nor : NORMAL;
};

float4 PS_Phong(PS_IN input) : SV_Target
{
    float3 viewDir = normalize((float3) input.worldPos - CameraPosition);
    float3 normal = normalize(input.nor);
    float4 result = AmbiendLighting * AmbiendLighting.w * input.col; // Амбиентная составляющая

    for (uint i = 0; i < PointLights; i++)
    {
        PointLight light = PointLightsBuffer[i];
        float3 lightDir;
        float attenuation = light.Color.w;
        
        lightDir = light.Position.xyz - input.worldPos.xyz;
        float distance = length(lightDir);
        lightDir = normalize(lightDir);
        attenuation /= distance * distance;

        //
        float diffuseFactor = max(dot(normal, lightDir), 0.0);
        float4 diffuse = diffuseFactor * light.Color * input.col;

        //
        float3 reflectDir = reflect(lightDir, normal);
        float specularFactor = pow(max(dot(viewDir, reflectDir), 0.0), pow(10, SpecularDispersion));
        float4 specular = specularFactor * light.Color;

        //
        result += (diffuse * DiffusalComponent + specular * SpecularComponent) * attenuation;
    }

    for (uint i = 0; i < DirectionalLights; i++)
    {
        DirectionalLight light = DirectionalLightsBuffer[i];
        float3 lightDir;
        float attenuation = light.Color.w;
        
        lightDir = normalize(light.Direction.xyz);

        //
        float diffuseFactor = max(dot(normal, lightDir), 0.0);
        float4 diffuse = diffuseFactor * light.Color * input.col;

        //
        float3 reflectDir = reflect(lightDir, normal);
        float specularFactor = pow(max(dot(viewDir, reflectDir), 0.0), pow(10, SpecularDispersion));
        float4 specular = specularFactor * light.Color;

        //
        result += (diffuse * DiffusalComponent + specular * SpecularComponent) * attenuation;
    }

    return result;
}