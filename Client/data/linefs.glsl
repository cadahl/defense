
void main() 
{
	gl_FragColor.rgb = gl_Color.rgb * gl_Color.a;
	gl_FragColor.a = gl_Color.a;
} 
