#version 330 core

layout(location = 0) in vec3 position;

uniform mat4 gl_ModelViewProjectionMatrix;
in vec4 gl_Color;
out vec4 color;

void main()
{
    gl_Position = gl_ModelViewProjectionMatrix * vec4(position.x, position.y, position.z, 1.0f);
	color = gl_Color;
}