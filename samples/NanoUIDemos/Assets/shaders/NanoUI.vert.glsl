#version 450

layout(location = 0) in vec2 vertex;
layout(location = 1) in vec2 tcoord;

layout(set = 0, binding = 0) uniform TransformBuffer
{
    mat4 transformMat;
};

layout(location = 0) out vec2 fpos;
layout(location = 1) out vec2 ftcoord;

void main()
{
	ftcoord = tcoord;
	fpos = vertex;

	gl_Position = transformMat * vec4(vertex, 0, 1);
}