#version 330 core

in vec3 position;
in vec4 gl_Color;
in vec2 texCoordV;
out vec2 UV;

uniform mat4 gl_ModelViewProjectionMatrix;
out vec4 color;

void main()
{
    gl_Position = gl_ModelViewProjectionMatrix * vec4(position.x, position.y, position.z, 1.0f);
	color = gl_Color;
    UV = new vec2(position.x, position.y);
}