#version 450

layout(set = 1, binding = 0) uniform texture2D Texture0;
layout(set = 1, binding = 1) uniform sampler Sampler0;

layout(location = 0) in vec4 Color;
layout(location = 1) in vec2 TexCoord;

layout(location = 0) out vec4 s_OutputColor;

void main()
{
    vec4 color = texture(sampler2D(Texture0, Sampler0), TexCoord);
    color *= Color;

    s_OutputColor = color;
}
