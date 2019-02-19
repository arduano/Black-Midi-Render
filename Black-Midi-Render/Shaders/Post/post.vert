#version 330 core

uniform mat4 gl_ModelViewProjectionMatrix;

in vec3 position;
in vec4 glColor;
in vec2 texCoordV;
out vec2 UV;

out vec4 color;

void main()
{
    gl_Position = gl_ModelViewProjectionMatrix * vec4(position.x, position.y, position.z, 1.0f);
	color = glColor;
    UV = vec2(position.x, position.y);
}