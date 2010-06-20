
uniform vec3 circle;

void main()
{   
	gl_FrontColor = gl_Color;
    gl_TexCoord[0].stpq = gl_MultiTexCoord0.stpq - circle.xyyy;
    gl_Position = gl_ModelViewProjectionMatrix * gl_Vertex;
}
