uniform sampler2D color_texture;

void main() 
{
	float colorizeMode = gl_TexCoord[0].p;
	vec4 tex = texture2D(color_texture, gl_TexCoord[0].st);
	
	gl_FragColor.rgb = mix(tex.rgb * gl_Color.rgb, gl_Color.a * tex.rgb + tex.a * gl_Color.rgb, colorizeMode);
	gl_FragColor.a = tex.a * gl_Color.a;
} 
