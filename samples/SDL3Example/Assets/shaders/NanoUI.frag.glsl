#version 450

layout(set = 0, binding = 1) uniform FragmentUniformBuffer
{
    mat4 scissorMat;
	mat4 paintMat;
	vec4 innerCol;
	vec4 outerCol;
	vec2 scissorExt;
	vec2 scissorScale;

	vec2 extent;
	float radius;
	float feather;
	
	float actionType;
	float fontSize;
	float unused1;
	float unused2;
};

layout(set = 1, binding = 0) uniform texture2D tex;
layout(set = 1, binding = 1) uniform sampler samp;

layout(location = 0) in vec2 fpos;
layout(location = 1) in vec2 ftcoord;

float sdroundrect(vec2 pt, vec2 ext, float rad)
{
	vec2 ext2 = ext - vec2(rad,rad);
	vec2 d = abs(pt) - ext2;
	return min(max(d.x,d.y),0.0) + length(max(d,0.0)) - rad;
}

// Scissoring
float scissorMask(vec2 p)
{
	vec2 sc = (abs((scissorMat * vec4(p,0,1)).xy) - scissorExt);
	sc = vec2(0.5,0.5) - sc * scissorScale;
	return clamp(sc.x,0.0,1.0) * clamp(sc.y,0.0,1.0);
}

layout(location = 0) out vec4 outColor;

void main(void)
{
	vec4 result;
	float scissor = scissorMask(fpos);
	float strokeAlpha = 1.0;

	if (actionType == 0)
	{
		// Gradient
		// Calculate gradient color using box gradient
		vec2 pt = (paintMat * vec4(fpos,0,1)).xy;
		float d = clamp((sdroundrect(pt, extent, radius) + feather*0.5) / feather, 0.0, 1.0);
		vec4 color = mix(innerCol,outerCol,d);

		// Combine alpha
		color *= strokeAlpha * scissor;
		result = color;
	}
	else if (actionType == 1)
	{
		// Texture
		// Calculate color fron texture
		vec2 pt = (paintMat * vec4(fpos,0,1)).xy / extent;
		
		//vec4 color = texture2D(tex, pt);
		vec4 color = texture(sampler2D(tex, samp), pt);

		color = vec4(color.xyz*color.w,color.w);
		
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
		result = vec4(1,1,1,1);
	}
	else if (actionType == 3)
	{
		// Normal text rendering
		// only r channel active (R)
		vec4 color = vec4(texture(sampler2D(tex, samp), ftcoord).x);

		// full color (RGBA)
		//vec4 color = texture(sampler2D(tex, samp), ftcoord);
		//color = vec4(color.xyz*color.w,color.w);

		color *= scissor;
		result = color * innerCol;
	}
	else if (actionType == 4)
	{
		// SDF text
		// this works also with normal baking - could be still simpler!
		float distance = texture(sampler2D(tex, samp), ftcoord).x;
		
		// The right smoothing value for crisp fonts is 0.25f / (spread * scale)
		// - spread is the value you used when generating the font
		// - scale is the scale you’re drawing it at

		// note: this algo could be better/different!
		float weight = 0.5;
		float smoothing = clamp(4.0 / fontSize, 0.0, 0.5);
		float alpha = smoothstep(weight - smoothing, weight + smoothing, distance);

		result = vec4(innerCol.rgb, innerCol.a * alpha) * scissor;
	}

	outColor = result;
}