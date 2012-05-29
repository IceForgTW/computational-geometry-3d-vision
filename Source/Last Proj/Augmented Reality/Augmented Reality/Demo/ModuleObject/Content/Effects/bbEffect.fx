// -------- Parameters ----------
float4x4 xView;
float4x4 xProjection;
float4x4 xWorld;
float3 xCameraPos;
float3 xAllowedRotDir;

// -------- Texture Sampler --------

Texture xBillboardTexture;

sampler textureSampler = sampler_state
{
	texture = <xBillboardTexture>;
	magfilter = LINEAR;
	minfilter = LINEAR;
	mipfilter = LINEAR;
	AddressU = CLAMP;
	AddressV = CLAMP;
};

// --------- Technique : CylBillboard --------------
struct BBVertexToPixel
{
	float4 Position : POSITION;
	float2 TexCoords : TEXCOORD0;
};

struct BBPixelToFrame
{
	float4 Color : COLOR0;
};

BBVertexToPixel CylBillboardVS(float4 inPos : POSITION, float2 inTexCoords : TEXCOORD0)
{
	BBVertexToPixel output = (BBVertexToPixel)0;
	float4x4 preViewProjection = mul(xView, xProjection);
	
	float3 center = mul(inPos, xWorld);
	float3 eyeVector = center - xCameraPos;
	
	float3 upVector = normalize(xAllowedRotDir);
	
	float3 sideVector = normalize(cross(eyeVector, upVector));
	
	float3 finalPosition = center;
	finalPosition += (inTexCoords.x - 0.5f) * sideVector;
	finalPosition += (1.5f - inTexCoords.y * 1.5f) * upVector;
	
	float4 finalPosition4 = float4(finalPosition, 1);
	
	output.Position = mul(finalPosition4, preViewProjection);
	output.TexCoords = inTexCoords;
	
	return output;
}

BBPixelToFrame CylBillboardPS(BBVertexToPixel PSIn)
{
	BBPixelToFrame output = (BBPixelToFrame)0;
	
	output.Color = tex2D(textureSampler, PSIn.TexCoords);
	
	return output;
}

technique CylBillboard
{
	pass Pass0
	{
		VertexShader = compile vs_1_1 CylBillboardVS();
		PixelShader = compile ps_1_1 CylBillboardPS();
	}
}