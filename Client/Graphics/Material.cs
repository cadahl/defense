
using System;

namespace Client.Graphics
{
	using System.Collections.Generic;
	using System.IO;
	using OpenTK;
	using OpenTK.Graphics.OpenGL;
	//using Client.Graphics;

	public class Material : IDisposable
	{
		private int _program;

		public Material(Renderer r, string vsPath, string fsPath)
		{
			_program = GL.CreateProgram();
			int vertexShader = CreateShader(r, ShaderType.VertexShader, vsPath);
			int fragmentShader = CreateShader(r, ShaderType.FragmentShader, fsPath);
			
			GL.AttachShader(_program, vertexShader);
			GL.AttachShader(_program, fragmentShader);
			GL.LinkProgram(_program);

            string info = GL.GetProgramInfoLog(_program);
			if(info.Length>0)
	            Console.WriteLine(info);
		}
		
		public void Dispose()
		{
			if(_program != 0)
				GL.DeleteProgram(_program);
		}
		
		public void Bind()
		{
			GL.UseProgram(_program);
		}
		
		public void Uniform1(string name, float f)
		{
			int loc = GL.GetUniformLocation(_program, name);
			GL.Uniform1(loc, f);
		}

		public void Uniform2(string name, ref Vector2 v)
		{
			int loc = GL.GetUniformLocation(_program, name);
			GL.Uniform2(loc, v);
		}

		public void Uniform3(string name, ref Vector3 v)
		{
			int loc = GL.GetUniformLocation(_program, name);
			GL.Uniform3(loc, v);
		}

		public void Uniform4(string name, ref Vector4 v)
		{
			int loc = GL.GetUniformLocation(_program, name);
			GL.Uniform4(loc, v);
		}

		public void Color4(string name, ref OpenTK.Graphics.Color4 v)
		{
			int loc = GL.GetUniformLocation(_program, name);
			GL.Uniform4(loc, v);
		}

		public void SetTilemap(string name, Tilemap tilemap)
		{
		    int texture_location = GL.GetUniformLocation(_program, name);
		    GL.Uniform1(texture_location, 0);
		    GL.BindTexture(TextureTarget.Texture2D, tilemap.Texture);
		}
		
		private int CreateShader(Renderer r, ShaderType type, string srcPath)
		{
			int sh;
			
			if(!r.ShaderCache.TryGetValue(srcPath.GetHashCode(), out sh))
			{
				using(StreamReader sr = File.OpenText(srcPath))
				{
					sh = GL.CreateShader(type);
					
					GL.ShaderSource(sh, sr.ReadToEnd());
					GL.CompileShader(sh);
					
					string log = GL.GetShaderInfoLog(sh);
					if(log != null)
					{
						string s = log.Trim('\n','\r',' ','\t');
						if(s.Length > 0)
							Console.WriteLine(s);
					}

					int compileResult;
		            GL.GetShader(sh, ShaderParameter.CompileStatus, out compileResult);
		            if (compileResult != 1)
		            {
		                Console.WriteLine("Compile Error in " + srcPath);
		            }			
				}
			}
			
			return sh;
		}
	}
}
