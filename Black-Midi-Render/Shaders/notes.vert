#version 330 core

layout(location = 0) in vec3 position;
layout(location = 1) in vec4 glColor;
layout(location = 2) in vec2 attrib;

out vec4 color;

void main()
{
    gl_Position = vec4(position.x * 2 - 1, position.y * 2 - 1, 1.0f, 1.0f);
    if(attrib.x == 0) color = glColor;
    else
    { 
        //vec3 hsv = rgb2hsv(glColor.xyz);
        //hsv.z += attrib.x;
        color = vec4(glColor.xyz + attrib.x, glColor.w);//vec4(hsv2rgb(hsv), glColor.w);
    }
}
