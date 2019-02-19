#version 330 core

layout(location = 0) in vec3 position;

uniform mat4 mvpm;
in vec4 inColor;
out vec4 color;

void main()
{
    gl_Position = mvpm * vec4(position.x, position.y, position.z, 1.0f);
	color = inColor;
}