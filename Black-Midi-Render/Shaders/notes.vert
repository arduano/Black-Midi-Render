#version 330 core

in vec2 position;
in vec4 glColor;
in vec2 attrib;


uniform mat4 gl_ModelViewProjectionMatrix;
out vec4 color;


vec3 rgb2hsv(vec3 rgb)
{
    vec3         hsv;
    float      _min, _max, delta;

    _min = rgb.x < rgb.y ? rgb.x : rgb.y;
    _min = _min  < rgb.z ? _min  : rgb.z;

    _max = rgb.x > rgb.y ? rgb.x : rgb.y;
    _max = _max  > rgb.z ? _max  : rgb.z;

    hsv.z = _max;                                // v
    delta = _max - _min;
    if (delta < 0.00001)
    {
        hsv.y = 0;
        hsv.x = 0; // undefined, maybe nan?
        return hsv;
    }
    if( _max > 0.0 ) { // NOTE: if Max is == 0, this divide would cause a crash
        hsv.y = (delta / _max);                  // s
    } else {
        // if max is 0, then r = g = b = 0              
        // s = 0, h is undefined
        hsv.y = 0.0;
        hsv.x = 0;                            // its now undefined
        return hsv;
    }
    if( rgb.x >= _max )                           // > is bogus, just keeps compilor happy
        hsv.x = ( rgb.y - rgb.z ) / delta;        // between yellow & magenta
    else
    if( rgb.y >= _max )
        hsv.x = 2.0 + ( rgb.z - rgb.x ) / delta;  // between cyan & yellow
    else
        hsv.x = 4.0 + ( rgb.x - rgb.y ) / delta;  // between magenta & cyan

    hsv.x *= 60.0;                              // degrees

    if( hsv.x < 0.0 )
        hsv.x += 360.0;

    return hsv;
}


vec3 hsv2rgb(vec3 hsv)
{
    float      hh, p, q, t, ff;
    int        i;
    vec3         rgb;

    if(hsv.y <= 0.0) {       // < is bogus, just shuts up warnings
        rgb.x = hsv.z;
        rgb.y = hsv.z;
        rgb.z = hsv.z;
        return rgb;
    }
    hh = hsv.x;
    if(hh >= 360.0) hh = 0.0;
    hh /= 60.0;
    i = int(hh);
    ff = hh - i;
    p = hsv.z * (1.0 - hsv.y);
    q = hsv.z * (1.0 - (hsv.y * ff));
    t = hsv.z * (1.0 - (hsv.y * (1.0 - ff)));

    switch(i) {
    case 0:
        rgb.x = hsv.z;
        rgb.y = t;
        rgb.z = p;
        break;
    case 1:
        rgb.x = q;
        rgb.y = hsv.z;
        rgb.z = p;
        break;
    case 2:
        rgb.x = p;
        rgb.y = hsv.z;
        rgb.z = t;
        break;

    case 3:
        rgb.x = p;
        rgb.y = q;
        rgb.z = hsv.z;
        break;
    case 4:
        rgb.x = t;
        rgb.y = p;
        rgb.z = hsv.z;
        break;
    case 5:
    default:
        rgb.x = hsv.z;
        rgb.y = p;
        rgb.z = q;
        break;
    }
    return rgb;     
}

void main()
{
    gl_Position = gl_ModelViewProjectionMatrix * vec4(position.x, position.y, 1, 1.0f);
    if(attrib.x == 0) color = glColor;
    else
    { 
        vec3 hsv = rgb2hsv(glColor.xyz);
        hsv.z += attrib.x;
        color = vec4(hsv2rgb(hsv), glColor.w);
    }
}
