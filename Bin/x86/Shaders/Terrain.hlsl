struct VertexInputType
{
	float4 position : POSITION;
	float4 normal : NORMAL;
	float2 tex : TEXCOORD0;
};

struct PixelInputType
{
	float4 position : SV_POSITION;
	float3 normal : NORMAL;
	float2 tex : TEXCOORD0;
};

struct PixelOutputType
{
	float4 diffuse : COLOR0;
	float4 normal : COLOR1;
};

matrix viewMatrix;
matrix projectionMatrix;
Texture2D terrainTexture[%TEXTURECOUNT%];
Texture2D alphaTexture[%OVERLAYCOUNT%];//Always one less than terraintextures
float texRepeat[%TEXTURECOUNT%];
SamplerState textureSampler;
SamplerState alphaSampler;

PixelInputType VShader(VertexInputType input)
{
	PixelInputType output;
	
	
	// Change the position vector to be 4 units for proper matrix calculations.
	input.position.w = 1.0f;

	// Calculate the position of the vertex against the view, and projection matrices.
	output.position = mul(input.position, viewMatrix);
	output.position = mul(output.position, projectionMatrix);
	

	// Store the texturecoordinate
	output.tex = input.tex;

	output.normal = input.normal;
	
	return output;
}

PixelOutputType PShader(PixelInputType input) : SV_TARGET
{
	PixelOutputType output;
	output.diffuse = terrainTexture[0].Sample(textureSampler, input.tex * texRepeat[0]);
	output.normal = float4(input.normal, 1.0f);
	/*uint i = 0;
	[unroll] while (i < %OVERLAYCOUNT%)
	{
		float opacity = alphaTexture[i].Sample(alphaSampler, input.tex).r;
		const float blend = 0.1f;
		i++;
		if(opacity > 0.05f)
		{
			float4 texturesample = terrainTexture[i].Sample(textureSampler, input.tex * texRepeat[i]);
			if(texturesample.a > opacity - blend)
			{
				if(texturesample.a > opacity + blend)
				{
					output.diffuse = float4(texturesample.r, texturesample.g, texturesample.b, 1.0f);
				}
				else
				{
					output.diffuse = lerp(output.diffuse, float4(texturesample.r, texturesample.g, texturesample.b, 1.0f), (texturesample.a + blend - opacity) / (2 * blend));
				}
			}
		}
	}*/
	return output;
}

technique11 Terrain
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_4_0, VShader()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_4_0, PShader()));
	}
}