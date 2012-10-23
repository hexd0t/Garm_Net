/*******************************************************\
|                    Menu subrender                     |
\*******************************************************/

struct VShaderIn
{
	float4 position : POSITION;
	float2 tex : TEXCOORD0;
};
struct PShaderIn
{
	float4 position : SV_POSITION;
	float2 tex : TEXCOORD0;
};

Texture2D menuTexture;
SamplerState textureSampler;

PShaderIn VShader(VShaderIn inp)
{
	PShaderIn outp;
	outp.position = inp.position;
	outp.tex = inp.tex;
	return outp;
}

float4 PShader(PShaderIn inp) : SV_TARGET
{
	return menuTexture.Sample(textureSampler, inp.tex);;
}


technique11 Menu
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_4_0, VShader()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_4_0, PShader()));
	}
}