Texture2D    g_texture : register(t0, space2);
SamplerState g_sampler : register(s0, space2);

struct PS_INPUT
{
    float4 position   : SV_Position;    // vertex position
    float2 ftcoord    : TEXCOORD0;      // float 2 tex coord
    float2 fpos       : TEXCOORD1;      // float 2 position 
};

cbuffer PS_CONSTANTS : register(b0, space3)
{
    float4x4 scissorMat : packoffset(c0);
    float4x4 paintMat : packoffset(c4);
    float4 innerCol : packoffset(c8);
    float4 outerCol : packoffset(c9);
    float2 scissorExt : packoffset(c10);
    float2 scissorScale : packoffset(c10.z);
    
    float2 extent : packoffset(c11);
    float radius : packoffset(c11.z);
    float feather : packoffset(c11.w);
    
    float actionType : packoffset(c12);
    float fontSize : packoffset(c12.y);
	float unused1 : packoffset(c12.z);
	float unused2 : packoffset(c12.w);
};
 
float sdroundrect(float2 pt, float2 ext, float rad) 
{
    float2 ext2 = ext - float2(rad,rad);
    float2 d = abs(pt) - ext2;
    return min(max(d.x,d.y),0.0) + length(max(d,0.0)) - rad;
}

// Scissoring
float scissorMask(float2 p) 
{
    float2 sc = (abs((mul(scissorMat, float4(p.x, p.y,0, 1.0))).xy) - scissorExt);
    sc = float2(0.5,0.5) - sc * scissorScale;
    return clamp(sc.x,0.0,1.0) * clamp(sc.y,0.0,1.0);
}

float4 main(PS_INPUT input) : SV_TARGET
{
    float4 result;
    float scissor = scissorMask(input.fpos);
    float strokeAlpha = 1.0f;

    if (actionType == 0) 
    {
        // Calculate gradient color using box gradient
        float2 pt = (mul(paintMat, float4(input.fpos,0,1.0))).xy;
        float d = clamp((sdroundrect(pt, extent, radius) + feather*0.5) / feather, 0.0, 1.0);
        float4 color = lerp(innerCol, outerCol, d);
        
        // Combine alpha
        color *= strokeAlpha * scissor;
        result = color;
    }
    else if (actionType == 1)
    {
        // Calculate color fron texture
        float2 pt = (mul(paintMat, float4(input.fpos,0,1.0))).xy / extent;
        float4 color = g_texture.Sample(g_sampler, pt);
        
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
    }
    else if (actionType == 2)
    {
        // Stencil fill
        result = float4(1,1,1,1);
    } 
    else if (actionType == 3)
    {
        // Normal text rendering

        // full color (RGBA)
        //float4 color = g_texture.Sample(g_sampler, input.ftcoord);
        // color = float4(color.xyz*color.w,color.w);
        
        // only r channel active (R)
        float col = g_texture.Sample(g_sampler, input.ftcoord).x;
        float4 color = float4(col, col, col, col);

        color *= scissor;
        result = (color * innerCol);
    }
    else //if (actionType == 4)
    {
        // SDF text
		// this works also with normal baking - could be still simpler!
        float distance = g_texture.Sample(g_sampler, input.ftcoord).x;

        // The right smoothing value for crisp fonts is 0.25f / (spread * scale)
		// - spread is the value you used when generating the font
		// - scale is the scale you’re drawing it at

        // note: this algo could be better/different!
        float weight = 0.5;
        float smoothing = clamp(4.0 / fontSize, 0.0, 0.5);
        float alpha = smoothstep(weight - smoothing, weight + smoothing, distance);

        result = float4(innerCol.xyz, innerCol.w * alpha) * scissor;
    }

    return result;
}