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

float2 screenSize;

PShaderIn VShader(VShaderIn inp)
{
	PShaderIn outp;
	outp.position = inp.position;
	outp.tex = inp.tex*screenSize;
	return outp;
}

float4 PShader(PShaderIn inp) : SV_TARGET
{
	float4 result;
	bool isA = (inp.tex.X%30) < 5;
	bool isB = ((inp.tex.X * inp.tex.Y * 0,866f)%30) < 5;
	if(isA && !isB)
		result = float4(1,0,0,1);
	else if(isB /*&& !isC*/)
		result = float(0,1,0,1);
	else
		result = float4(0.3f, 0.3f, 0.3f,1.0f);

	return result;
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