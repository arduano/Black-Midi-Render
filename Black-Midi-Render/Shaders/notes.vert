#version 330 core

layout(location = 0) in vec3 position;

uniform mat4 gl_ModelViewProjectionMatrix;
uniform vec4 gl_Color;

void main()
{
    //gl_Position = gl_ModelViewProjectionMatrix * vec4(position.x, position.y, position.z, 1.0f);
    gl_Position = vec4(position.x, position.y, position.z, 1.0f);
    //gl_FrontColor = gl_Color; 
}