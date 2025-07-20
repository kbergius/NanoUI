#version 450

layout(constant_id = 100) const bool ClipSpaceInvertedY = true;
layout(constant_id = 101) const bool TextureCoordinatesInvertedY = false;

layout(location = 0) in vec2 vertex;
layout(location = 1) in vec2 tcoord;

layout(location = 0) out vec2 outUV;

void main()
{
    outUV = tcoord;
    gl_Position = vec4(vertex.x, vertex.y, 0, 1);

    // OpenGL, OpenGLES
    if (ClipSpaceInvertedY || TextureCoordinatesInvertedY)
    {
        outUV.y = -outUV.y;
    }
}