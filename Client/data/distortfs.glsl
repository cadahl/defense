uniform sampler2D color_texture;

void main() 
{
	vec2 st = gl_TexCoord[0].st;

	vec4 tex = texture2D(color_texture, st);
	
	gl_FragColor = tex;
} 

