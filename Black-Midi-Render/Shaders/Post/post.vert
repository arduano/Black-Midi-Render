#version 330 core

in vec3 position;
in vec4 inColor;
in vec2 texCoordV;
out vec2 UV;

uniform mat4 mvpm;
out vec4 color;

void main()
{
    gl_Position = mvpm * vec4(position.x, position.y, position.z, 1.0f);
	color = inColor;
    UV = vec2(position.x, position.y);
}