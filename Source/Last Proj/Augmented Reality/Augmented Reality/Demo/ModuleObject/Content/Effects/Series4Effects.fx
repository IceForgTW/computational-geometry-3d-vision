// -------- Parameters ----------
float4x4 xView;
float4x4 xProjection;
float4x4 xWorld;
float4x4 xWorldViewProjection;
int		 xWidth;
int		 xHeight;

bool xEnableLighting;
float3 xLightDirection;
float xAmbient;

// -------- Texture Sampler --------

Texture xTexture;

sampler TextureSampler = sampler_state
{
	texture = <xTexture>;
	magfilter = LINEAR;
	minfilter = LINEAR;
	mipfilter = LINEAR;
	AddressU = wrap;
	AddressV = wrap;
};

// --------- Technique : Textured --------------
struct TexVertexToPixel
{
	float4 Position : POSITION;
	float4 Color : COLOR0;
	float LightingFactor : TEXCOORD0;
	float2 TextureCoords : TEXCOORD1;
};

struct TexPixelToFrame
{
	float4 Color : COLOR0;
};

TexVertexToPixel TexturedVS ( float4 inPos : POSITION, float3 inNormal : NORMAL, float2 inTexCoords : TEXCOORD0)
{
	TexVertexToPixel output = (TexVertexToPixel)0;
	
//	float4x4 preViewProjection = mul(xView, xProjection);
//	float4x4 preWorldViewProjection = mul(xWorld, preViewProjection);
	
//	output.Position = mul(inPos, preWorldViewProjection);
	output.Position = mul(inPos, xWorldViewProjection);
	output.Position[0] = output.Position[0]/output.Position[2];
	output.Position[0] = 2*output.Position[0]/xWidth - 1;
	output.Position[1] = output.Position[1]/output.Position[2];
	output.Position[1] = 1-2*output.Position[1]/xHeight;
	output.Position[2] = 0.5+output.Position[2]/1000000.0f;
	output.Position[3] = 1;	
	
	output.TextureCoords = inTexCoords;
	
	float3 Normal = normalize(mul(normalize(inNormal), xWorld));
	output.LightingFactor = 1;
	if(xEnableLighting)
	{
		output.LightingFactor = saturate(dot(Normal, -xLightDirection));
	}
	
	return output;
}

TexPixelToFrame TexturedPS ( TexVertexToPixel PSIn )
{
	TexPixelToFrame output = (TexPixelToFrame)0;
	
	output.Color = tex2D(TextureSampler, PSIn.TextureCoords);
	output.Color.rgb *= saturate(PSIn.LightingFactor + xAmbient);
	
	return output;
}

technique Textured
{
	pass Pass0
	{
		VertexShader = compile vs_1_1 TexturedVS();
		PixelShader = compile ps_1_1 TexturedPS();
	}
}

// --------- Technique : MultiTextured --------------

	// -------- Texture Sampler --------
Texture xTexture0;
sampler TextureSampler0 = sampler_state
{
	texture = <xTexture0>;
	magfilter = LINEAR;
	minfilter = LINEAR;
	mipfilter = LINEAR;
	AddressU = wrap;
	AddressV = wrap;
};

Texture xTexture1;
sampler TextureSampler1 = sampler_state
{
	texture = <xTexture1>;
	magfilter = LINEAR;
	minfilter = LINEAR;
	mipfilter = LINEAR;
	AddressU = wrap;
	AddressV = wrap;
};

Texture xTexture2;
sampler TextureSampler2 = sampler_state
{
	texture = <xTexture2>;
	magfilter = LINEAR;
	minfilter = LINEAR;
	mipfilter = LINEAR;
	AddressU = mirror;
	AddressV = mirror;
};

Texture xTexture3;
sampler TextureSampler3 = sampler_state
{
	texture = <xTexture3>;
	magfilter = LINEAR;
	minfilter = LINEAR;
	mipfilter = LINEAR;
	AddressU = mirror;
	AddressV = mirror;
};

	// ----- Technique Processing -----
struct MultiTexVertexToPixel
{
	float4 Position : POSITION;
	float4 Color : COLOR0;
	float3 Normal : TEXCOORD0;
	float2 TextureCoords : TEXCOORD1;
	float4 LightDirection : TEXCOORD2;
	float4 TextureWeights : TEXCOORD3;
	float Depth : TEXCOORD4;
};

struct MultiTexPixelToFrame
{
	float4 Color : COLOR0;
};

MultiTexVertexToPixel MultiTexturedVS (float4 inPos : POSITION, float3 inNormal : NORMAL, float2 inTexCoords : TEXCOORD0, float4 inTexWeights : TEXCOORD1)
{
	MultiTexVertexToPixel output = (MultiTexVertexToPixel)0;
	
	float4x4 preViewProjection = mul(xView, xProjection);
	float4x4 preWorldViewProjection = mul(xWorld, preViewProjection);
	
	output.Position = mul(inPos, preWorldViewProjection);
	output.Normal = mul(normalize(inNormal), xWorld);
	output.TextureCoords = inTexCoords;
	output.LightDirection.xyz = -xLightDirection;
	output.LightDirection.w = 1;
	output.TextureWeights = inTexWeights;
	output.Depth = output.Position.z / output.Position.w;
	
	return output;
}

MultiTexPixelToFrame MultiTexturedPS (MultiTexVertexToPixel PSIn)
{
	MultiTexPixelToFrame output = (MultiTexPixelToFrame)0;
	
	float lightingFactor = 1;
	if(xEnableLighting)
	{
		lightingFactor = saturate(saturate(dot(PSIn.Normal, PSIn.LightDirection)) + xAmbient);
	}
	
	float blendDistance = 0.99f;
	float blendWidth = 0.005f;
	float blendFactor = clamp((PSIn.Depth - blendDistance) / blendWidth, 0, 1);
	
	float4 farColor;
	farColor = tex2D(TextureSampler0, PSIn.TextureCoords) * PSIn.TextureWeights.x;
	farColor += tex2D(TextureSampler1, PSIn.TextureCoords) * PSIn.TextureWeights.y;
	farColor += tex2D(TextureSampler2, PSIn.TextureCoords) * PSIn.TextureWeights.z;
	farColor += tex2D(TextureSampler3, PSIn.TextureCoords) * PSIn.TextureWeights.w;
	
	float4 nearColor;
	float2 nearTextureCoords = PSIn.TextureCoords * 3.0f;
	nearColor = tex2D(TextureSampler0, nearTextureCoords) * PSIn.TextureWeights.x;
	nearColor += tex2D(TextureSampler1, nearTextureCoords) * PSIn.TextureWeights.y;
	nearColor += tex2D(TextureSampler2, nearTextureCoords) * PSIn.TextureWeights.z;
	nearColor += tex2D(TextureSampler3, nearTextureCoords) * PSIn.TextureWeights.w;
	
	output.Color = lerp(nearColor, farColor, blendFactor);
	output.Color *= lightingFactor;
	
	return output;
}

technique MultiTextured
{
	pass Pass0
	{
		VertexShader = compile vs_1_1 MultiTexturedVS();
		PixelShader = compile ps_2_0 MultiTexturedPS();
	}
}

// ----------------- Technique : Water -------------------
// ----- Parameters -----
float4x4 xReflectionView;
float xWaveHeight;
float xWaveLength;
float3 xCameraPos;
float xTime;
float3 xWindDirection;
float xWindForce;

// ----- Textures -----
Texture xReflectionMap;

sampler ReflectionSampler = sampler_state
{
	texture = <xReflectionMap>;
	magfilter = LINEAR;
	minfilter = LINEAR;
	mipfilter = LINEAR;
	AddressU = mirror;
	AddressV = mirror;
};

Texture xRefractionMap;

sampler RefractionSampler = sampler_state
{
	texture = <xRefractionMap>;
	magfilter = LINEAR;
	minfilter = LINEAR;
	mipfilter = LINEAR;
	AddressU = mirror;
	AddressV = mirror;
};

Texture xWaterBumpMap;

sampler WaterBumpMapSampler = sampler_state
{
	texture = <xWaterBumpMap>;
	magfilter = LINEAR;
	minfilter = LINEAR;
	mipfilter = LINEAR;
	AddressU = mirror;
	AddressV = mirror;
};

// ----- Technique Proccessing -----
struct WVertexToPixel
{
	float4 Position : POSITION;
	float4 ReflectionMapSamplingPos : TEXCOORD0;
	float2 BumpMapSamplingPos : TEXCOORD1;
	float4 RefractionMapSamplingPos : TEXCOORD2;
	float4 Position3D : TEXCOORD3;
};

struct WPixelToFrame
{
	float4 Color : COLOR0;
};

WVertexToPixel WaterVS(float4 inPos : POSITION, float4 inTex : TEXCOORD)
{
	WVertexToPixel output = (WVertexToPixel)0;
	
	float4x4 preViewProjection = mul(xView, xProjection);
	float4x4 preWorldViewProjection = mul(xWorld, preViewProjection);
	float4x4 preReflectionViewProjection = mul(xReflectionView, xProjection);
	float4x4 preWorldReflectionViewProjection = mul(xWorld, preReflectionViewProjection);
	
	output.Position = mul(inPos, preWorldViewProjection);
	output.ReflectionMapSamplingPos = mul(inPos, preWorldReflectionViewProjection);
	output.RefractionMapSamplingPos = mul(inPos, preWorldViewProjection);
	output.Position3D = mul(inPos, xWorld);
	
	float3 windDir = normalize(xWindDirection);
	float3 perpDir = normalize(cross(xWindDirection, float3(0,1,0)));
	
	float xdot = dot(inTex, windDir.xz);
	float ydot = dot(inTex, perpDir.xz);
	float2 moveVector = float2(xdot, ydot + (xTime * xWindForce));
	output.BumpMapSamplingPos = moveVector / xWaveLength;
	
	return output;
}

WPixelToFrame WaterPS(WVertexToPixel PSIn)
{
	WPixelToFrame output = (WPixelToFrame)0;
	
	float4 bumpColor = tex2D(WaterBumpMapSampler, PSIn.BumpMapSamplingPos);
	float2 perturbation = xWaveHeight * (bumpColor - 0.5f) * 2.0f;
	
	float2 ProjectedReflTexCoords;
	ProjectedReflTexCoords.x = PSIn.ReflectionMapSamplingPos.x / PSIn.ReflectionMapSamplingPos.w / 2.0f + 0.5f;
	ProjectedReflTexCoords.y = -PSIn.ReflectionMapSamplingPos.y / PSIn.ReflectionMapSamplingPos.w / 2.0f + 0.5f;
	float2 perturbatedReflTexCoords = ProjectedReflTexCoords + perturbation;
	float4 reflectiveColor = tex2D(ReflectionSampler, perturbatedReflTexCoords);
	
	float2 ProjectedRefrTexCoords;
	ProjectedRefrTexCoords.x = PSIn.RefractionMapSamplingPos.x / PSIn.RefractionMapSamplingPos.w / 2.0f + 0.5f;
	ProjectedRefrTexCoords.y = -PSIn.RefractionMapSamplingPos.y / PSIn.RefractionMapSamplingPos.w / 2.0f + 0.5f;
	float2 perturbatedRefrTexCoords = ProjectedRefrTexCoords + perturbation;
	float4 refractiveColor = tex2D(RefractionSampler, perturbatedRefrTexCoords);
	
	float3 eyeVector = normalize(xCameraPos - PSIn.Position3D);
	float3 normalVector = (bumpColor.rbg - 0.5f) * 2.0f;
	float fresnelTerm = dot(eyeVector, normalVector);
	
	float4 combinedColor = lerp(reflectiveColor, refractiveColor, fresnelTerm);
	float4 dullColor = float4(0.3f, 0.3f, 0.5f, 1.0f);
	
	output.Color = lerp(combinedColor, dullColor, 0.2f);
	
	float3 reflectionVector = -reflect(xLightDirection, normalVector);
	float specular = dot(normalize(reflectionVector), normalize(eyeVector));
	specular = pow(specular, 256);
	output.Color.rgb += specular;
	
	return output;
}

technique Water
{
	pass Pass0
	{
		VertexShader = compile vs_1_1 WaterVS();
		PixelShader = compile ps_2_0 WaterPS();
	}
}

// ------ Technique : PerlinNoise ------
float xOvercast;

struct PNVertexToPixel
{
	float4 Position : POSITION;
	float2 TexCoords : TEXCOORD0;
};

struct PNPixelToFrame
{
	float4 Color : COLOR0;
};

PNVertexToPixel PerlinVS(float4 inPos : POSITION, float2 inTexCoords : TEXCOORD0)
{
	PNVertexToPixel output = (PNVertexToPixel)0;
	
	output.Position = inPos;
	output.TexCoords = inTexCoords;
	
	return output;
}

PNPixelToFrame PerlinPS(PNVertexToPixel PSIn)
{
	PNPixelToFrame output = (PNPixelToFrame)0;
	
	float2 move = float2(0,1);
	float4 perlin = tex2D(TextureSampler, (PSIn.TexCoords) + xTime * move) / 2;
	perlin += tex2D(TextureSampler, (PSIn.TexCoords) * 2 + xTime * move) / 4;
	perlin += tex2D(TextureSampler, (PSIn.TexCoords) * 4 + xTime * move) / 8;
	perlin += tex2D(TextureSampler, (PSIn.TexCoords) * 8 + xTime * move) / 16;
	perlin += tex2D(TextureSampler, (PSIn.TexCoords) * 16 + xTime * move) / 32;
	perlin += tex2D(TextureSampler, (PSIn.TexCoords) * 32 + xTime * move) / 32;
	
	output.Color = 1.0f - pow(perlin.r, xOvercast) * 2.0f;
	return output;
}

technique PerlinNoise
{
	pass Pass0
	{
		VertexShader = compile vs_1_1 PerlinVS();
		PixelShader = compile ps_2_0 PerlinPS();
	}
}

// ------ Technique : SkyDome ------
struct SDVertexToPixel
{
	float4 Position : POSITION;
	float2 TexCoords : TEXCOORD0;
	float4 ObjectPosition : TEXCOORD1;
};

struct SDPixelToFrame
{
	float4 Color : COLOR0;
};

SDVertexToPixel SkyDomeVS(float4 inPos : POSITION, float2 inTexCoords : TEXCOORD0)
{
	SDVertexToPixel output = (SDVertexToPixel)0;
	
	float4x4 preViewProjection = mul(xView, xProjection);
	float4x4 preWorldViewProjection = mul(xWorld, preViewProjection);
	
	output.Position = mul(inPos, preWorldViewProjection);
	output.TexCoords = inTexCoords;
	output.ObjectPosition = inPos;
	
	return output;
}

SDPixelToFrame SkyDomePS(SDVertexToPixel PSIn)
{
	SDPixelToFrame output = (SDPixelToFrame)0;
	
	float4 topColor = float4(0.3f, 0.3f, 0.8f, 1.0f);
	float4 bottomColor = 1;
	
	float4 baseColor = lerp(bottomColor, topColor, saturate((PSIn.ObjectPosition.y)/0.4f));
	float4 cloudValue = tex2D(TextureSampler, PSIn.TexCoords);
	
	output.Color = lerp(baseColor, 1, cloudValue);
	
	return output;
}

technique SkyDome
{
	pass Pass0
	{
		VertexShader = compile vs_1_1 SkyDomeVS();
		PixelShader = compile ps_2_0 SkyDomePS();
	}
}

// ------ Technique : PowerAreaMap ------
struct PAMapVertexToPixel
{
	float4 Position : POSITION;
	float4 Position2D : TEXCOORD0;
};

struct PAMapPixelToFrame
{
	float4 Color : COLOR0;
};

float4x4 xPowerAreaWorldViewProjection;

PAMapVertexToPixel PowerAreaMapVertexShader( float4 inPos : POSITION )
{
	PAMapVertexToPixel output = (PAMapVertexToPixel)0;
	
	output.Position = mul(inPos, xPowerAreaWorldViewProjection);
	output.Position2D = output.Position;
	
	return output;
}

PAMapPixelToFrame PowerAreaMapPixelShader ( PAMapVertexToPixel PSIn )
{
	PAMapPixelToFrame output = (PAMapPixelToFrame)0;
	
	output.Color = PSIn.Position2D.z / PSIn.Position2D.w;
	
	return output;
}

technique PowerAreaMap
{
	pass Pass0
	{
		VertexShader = compile vs_2_0 PowerAreaMapVertexShader();
		PixelShader = compile ps_2_0 PowerAreaMapPixelShader();
	}
}

// --------- Technique : PowerArea --------------

float4x4 xPowerAreaViewProjection;

Texture xPowerAreaMap;
Texture xPowerAreaTexture;

sampler PowerAreaSampler = sampler_state
{
	texture = <xPowerAreaTexture>;
	magfilter = LINEAR;
	minfilter = LINEAR;
	mipfilter = LINEAR;
	AddressU = clamp;
	AddressV = clamp;
};

sampler PowerAreaMapSampler = sampler_state
{
	texture = <xPowerAreaMap>;
	magfilter = LINEAR;
	minfilter = LINEAR;
	mipfilter = LINEAR;
	AddressU = clamp;
	AddressV = clamp;
};

struct PAVertexToPixel
{
	float4 Position : POSITION;
	float4 Color : COLOR0;
	float3 Normal : TEXCOORD0;
	float2 TextureCoords : TEXCOORD1;
	float4 LightDirection : TEXCOORD2;
	float4 TextureWeights : TEXCOORD3;
	float Depth : TEXCOORD4;
	float4 PosSeenByPowerArea : TEXCOORD5;
};

struct PAPixelToFrame
{
	float4 Color : COLOR0;
};

PAVertexToPixel PowerAreaVS (float4 inPos : POSITION, float3 inNormal : NORMAL, float2 inTexCoords : TEXCOORD0, float4 inTexWeights : TEXCOORD1)
{
	PAVertexToPixel output = (PAVertexToPixel)0;
	
	float4x4 preViewProjection = mul(xView, xProjection);
	float4x4 preWorldViewProjection = mul(xWorld, preViewProjection);
	float4x4 prePowerAreaWorldViewProjection = mul(xWorld, xPowerAreaViewProjection);
	
	output.Position = mul(inPos, preWorldViewProjection);
	output.Normal = mul(normalize(inNormal), xWorld);
	output.TextureCoords = inTexCoords;
	output.LightDirection.xyz = -xLightDirection;
	output.LightDirection.w = 1;
	output.TextureWeights = inTexWeights;
	output.Depth = output.Position.z / output.Position.w;
	output.PosSeenByPowerArea = mul(inPos, prePowerAreaWorldViewProjection);
	
	return output;
}

PAPixelToFrame PowerAreaPS (PAVertexToPixel PSIn)
{
	PAPixelToFrame output = (PAPixelToFrame)0;
	
	float lightingFactor = 1;
	if(xEnableLighting)
	{
		lightingFactor = saturate(saturate(dot(PSIn.Normal, PSIn.LightDirection)) + xAmbient);
	}
	
	float blendDistance = 0.99f;
	float blendWidth = 0.005f;
	float blendFactor = clamp((PSIn.Depth - blendDistance) / blendWidth, 0, 1);
	
	float4 farColor;
	farColor = tex2D(TextureSampler0, PSIn.TextureCoords) * PSIn.TextureWeights.x;
	farColor += tex2D(TextureSampler1, PSIn.TextureCoords) * PSIn.TextureWeights.y;
	farColor += tex2D(TextureSampler2, PSIn.TextureCoords) * PSIn.TextureWeights.z;
	farColor += tex2D(TextureSampler3, PSIn.TextureCoords) * PSIn.TextureWeights.w;
	
	float4 nearColor;
	float2 nearTextureCoords = PSIn.TextureCoords * 3.0f;
	nearColor = tex2D(TextureSampler0, nearTextureCoords) * PSIn.TextureWeights.x;
	nearColor += tex2D(TextureSampler1, nearTextureCoords) * PSIn.TextureWeights.y;
	nearColor += tex2D(TextureSampler2, nearTextureCoords) * PSIn.TextureWeights.z;
	nearColor += tex2D(TextureSampler3, nearTextureCoords) * PSIn.TextureWeights.w;
	
	output.Color = lerp(nearColor, farColor, blendFactor);
	output.Color *= lightingFactor;
	
	// Draw the power area
	float2 ProjectedTexCoords;
	ProjectedTexCoords[0] = PSIn.PosSeenByPowerArea.x / PSIn.PosSeenByPowerArea.w / 2.0f + 0.5f;
	ProjectedTexCoords[1] = -PSIn.PosSeenByPowerArea.y / PSIn.PosSeenByPowerArea.w / 2.0f + 0.5f;
	
	if((saturate(ProjectedTexCoords).x == ProjectedTexCoords.x) && (saturate(ProjectedTexCoords).y == ProjectedTexCoords.y))
	{
		float depthStoredInPowerAreaMap = tex2D(PowerAreaMapSampler, ProjectedTexCoords).r;
		float realDistance = PSIn.PosSeenByPowerArea.z / PSIn.PosSeenByPowerArea.w;
		if((realDistance - 0.01f) <= depthStoredInPowerAreaMap)
		{
			float4 color = tex2D(PowerAreaSampler, ProjectedTexCoords);
			output.Color += color;
		}
	}
	
	return output;
}

technique PowerArea
{
	pass Pass0
	{
		VertexShader = compile vs_1_1 PowerAreaVS();
		PixelShader = compile ps_2_0 PowerAreaPS();
	}
}