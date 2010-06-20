namespace Client 
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Threading;
	using Client.Graphics;
	using OpenTK;
	using OpenTK.Graphics.OpenGL;
	
	public class G52client 
	{
	    public Audio.Player AudioPlayer { get; private set; }
	    public Input Input { get; private set; }
	    public Renderer Renderer { get; private set; }
	    public Game Game { get; private set; }
	   
		[STAThread]
	    public static void Main(string[] args) 
		{
			try 
			{
			    G52client client = new G52client();
				client.GetType();
			}
			catch(Exception ex) 
			{
			    Console.Error.WriteLine("Top-level catch:\n" + ex.Message + "\n" + ex.StackTrace);
			}
	    }
	
	    public G52client() 
		{
	        int sw = 1024;
	        int sh = 768;
	
			this.AudioPlayer = new Client.Audio.Player();
			
			this.Renderer = new Renderer(sw, sh);
			
			this.Renderer.Update += (double time) => 
			{
				Game.Update(time);

				// Update audio at 10 Hz
				if((Game.UpdateCount % 6) == 0)
					AudioPlayer.Update();
			};

			this.Renderer.Render += () => 
			{
				Game.Render();
			};
					
	        this.Input = new Input(Renderer.Window);
	        this.Game = new Game(this);
	 
	        this.Renderer.Run();
	    }
		
	}
}