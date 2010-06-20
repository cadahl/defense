void main() 
{   
	gl_FrontColor = gl_Color;
    gl_TexCoord[0].stpq = gl_MultiTexCoord0.stpq;
    gl_Position = gl_ModelViewProjectionMatrix * gl_Vertex;
}

