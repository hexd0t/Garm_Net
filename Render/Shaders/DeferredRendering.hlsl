/*******************************************************\
|                    Compose Effect                     |
\*******************************************************/

Texture2D composeDiffuse;
Texture2D composeNormal;
Texture2D composeDepth;
SamplerState composeSampler;
int composeFlags;

struct ComposeVShaderIn
{
	float4 position : POSITION;
	float2 tex : TEXCOORD0;
};
struct ComposePShaderIn
{
	float4 position : SV_POSITION;
	float2 tex : TEXCOORD0;
};


ComposePShaderIn ComposeVShader(ComposeVShaderIn inp)
{
	ComposePShaderIn outp;
	outp.position = inp.position;
	outp.tex = inp.tex;
	return outp;
}

float4 ComposePShader(ComposePShaderIn inp) : SV_TARGET
{
	bool showGBuffers = fmod(composeFlags,2) == 1;

	float4 texcontent;
	if(showGBuffers)
	{
		if(inp.tex.x<0.5f)
		{//left part
			if(inp.tex.y<0.5f)
			{//upper part
				texcontent = composeDiffuse.Sample(composeSampler, inp.tex * 2);
			}
			else
			{//lower part
				texcontent = composeNormal.Sample(composeSampler, float2(inp.tex.x, inp.tex.y-0.5f) * 2);
			}
		}
		else
		{//right part
			if(inp.tex.y<0.5f)
			{//upper part
				float depthvar = composeDepth.Sample(composeSampler, float2(inp.tex.x-0.5f, inp.tex.y) * 2);
				texcontent = float4(depthvar, depthvar, depthvar, 1.0f);
			}
			else
			{//lower part
				texcontent = float4(((inp.tex.x+inp.tex.y)%0.05f)*20.0f, ((inp.tex.x*inp.tex.y)%0.1f)*10.0f,(inp.tex.x*inp.tex.y)%1.0f,1.0f);
			}
		}
	}
	else
	{
		texcontent = composeDiffuse.Sample(composeSampler, inp.tex);
	}
	return texcontent;
}


technique11 Compose
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_4_0, ComposeVShader()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_4_0, ComposePShader()));
	}
}