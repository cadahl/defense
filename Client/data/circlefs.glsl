#version 120

uniform vec4 color;
uniform vec3 circle;
uniform vec2 lowedge;
uniform vec2 highedge;

void main() 
{
	float dr = length(gl_TexCoord[0].st) - circle.z;
	vec2 res = smoothstep(lowedge, highedge, vec2(dr,dr));
	float c = res.y-res.x;

	if(c<0.01)
		discard;

	c *= color.a;

	gl_FragColor = vec4(c,c,c,c);
} 

