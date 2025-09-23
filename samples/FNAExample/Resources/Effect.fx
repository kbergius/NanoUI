#include "Macros.fxh"

DECLARE_TEXTURE(g_texture, 0);

BEGIN_CONSTANTS

    float4 innerCol;
    float4 outerCol;
    float2 scissorExt;
    float2 scissorScale;
    float2 extent;
    float radius;
    float feather;
    float actionType;
    float fontSize;

MATRIX_CONSTANTS

    float4x4 scissorMat;
    float4x4 paintMat;
	float4x4 transformMat;

END_CONSTANTS


struct VS_OUTPUT
{
    float4 position   : SV_Position;    // vertex position
    float2 fpos       : TEXCOORD0;      // float2 position 
    float2 ftcoord    : TEXCOORD1;      // float2 tex coord
};
  
struct PS_INPUT
{
    float4 position   : SV_Position;    // vertex position
    float2 fpos       : TEXCOORD0;      // float2 position 
    float2 ftcoord    : TEXCOORD1;      // float2 tex coord
};

VS_OUTPUT VSMain(float2 pt : POSITION, float2 tex : TEXCOORD0)
{
    VS_OUTPUT output;
    output.ftcoord = tex;
    output.fpos = pt;
    output.position = mul(float4(pt.x, pt.y, 0, 1), transformMat);
    
    return output;
}

float sdroundrect(float2 pt, float2 ext, float rad) 
{
    float2 ext2 = ext - float2(rad,rad);
    float2 d = abs(pt) - ext2;
    return min(max(d.x,d.y),0.0) + length(max(d,0.0)) - rad;
}

// Scissoring
float scissorMask(float2 p) 
{
    float2 sc = (abs((mul(float4(p.x, p.y,0, 1.0), scissorMat)).xy) - scissorExt.xy);
    sc = float2(0.5,0.5) - sc * scissorScale.xy;

    return clamp(sc.x,0.0,1.0) * clamp(sc.y,0.0,1.0);
}

float4 PSMainFillGradient(PS_INPUT input) : SV_TARGET
{
    float4 result;
    float scissor = scissorMask(input.fpos);
    float strokeAlpha = 1.0;

	// Calculate gradient color using box gradient
	float2 pt = (mul(float4(input.fpos,0, 1.0), paintMat)).xy;
    float d = clamp((sdroundrect(pt, extent.xy, radius.x) + feather.x*0.5) / feather.x, 0.0, 1.0);
	float4 color = lerp(innerCol, outerCol, d);
	
	// Combine alpha
	color *= strokeAlpha * scissor;
    result = color;

    return result;
}

float4 PSMainFillImage(PS_INPUT input) : SV_TARGET
{
    float4 result;
    float scissor = scissorMask(input.fpos);
    float strokeAlpha = 1.0;

	// Calculate color fron texture
	float2 pt = (mul(float4(input.fpos,0, 1.0), paintMat)).xy / extent.xy;
    float4 color = SAMPLE_TEXTURE(g_texture, pt);
	color = float4(color.xyz*color.w,color.w);
	
	// Apply color tint and alpha.
	//color *= innerCol;
    // Apply color tint if spedified
	if(innerCol.r != 0 || innerCol.g != 0 || innerCol.b != 0 || innerCol.a != 0)
	{
		color *= innerCol;
	}

	// Combine alpha
	color *= strokeAlpha * scissor;
	result = color;

    return result;
}

// Stencil fill
float4 PSMainFillStencil(PS_INPUT input) : SV_TARGET
{
	return float4(1,1,1,1);
}

float4 PSMainText(PS_INPUT input) : SV_TARGET
{
    float4 result;
    float scissor = scissorMask(input.fpos);
	float strokeAlpha = 1.0;

    // Normal text rendering

	// full color (RGBA)
	//float4 color = SAMPLE_TEXTURE(g_texture, input.ftcoord);
    //color = float4(color.xyz*color.w,color.w);

    // only r channel active (R)
    float col = SAMPLE_TEXTURE(g_texture, input.ftcoord).w;
    float4 color = float4(col, col, col, col);

	color *= scissor;
	result = (color * innerCol);
    
    return result;
}

float4 PSMainSDF(PS_INPUT input) : SV_TARGET
{
    float4 result;
    float scissor = scissorMask(input.fpos);
	float strokeAlpha = 1.0;

    // SDF text
	// this works also with normal baking - could be still simpler!
    float distance = SAMPLE_TEXTURE(g_texture, input.ftcoord).w;
    
    // The right smoothing value for crisp fonts is 0.25f / (spread * scale)
	// - spread is the value you used when generating the font
	// - scale is the scale you’re drawing it at

    // note: this algo could be better/different!
    float weight = 0.5;
    float smoothing = clamp(4.0 / fontSize, 0.0, 0.5);
    float alpha = smoothstep(weight - smoothing, weight + smoothing, distance);

    result = float4(innerCol.xyz, innerCol.w * alpha) * scissor;

    return result;
}

TECHNIQUE(FillGradient, VSMain, PSMainFillGradient);
TECHNIQUE(FillImage, VSMain, PSMainFillImage);
TECHNIQUE(FillStencil, VSMain, PSMainFillStencil);
TECHNIQUE(Text, VSMain, PSMainText);
TECHNIQUE(SDF, VSMain, PSMainSDF);
