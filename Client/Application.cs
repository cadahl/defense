namespace Client 
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Threading;
	using OpenTK;
	using OpenTK.Graphics.OpenGL;
	using Defense;
	
	public class Application 
	{
	    public Audio.Player AudioPlayer { get; private set; }
	    public Input Input { get; private set; }
	    public Graphics.Renderer Renderer { get; private set; }
	    public Game Game { get; private set; }
		public NetworkServer Server { get; private set; }
		public NetworkClient Client { get; private set; }
	   
		[STAThread]
	    public static void Main(string[] args) 
		{
			try 
			{
			    Application app = new Application();
				app.Run();
			}
			catch(Exception ex) 
			{
			    Console.Error.WriteLine("Top-level catch:\n" + ex.Message + "\n" + ex.StackTrace);
			}
	    }
	
	    public void Run() 
		{
			this.Server = new NetworkServer();
			this.Client = new NetworkClient();
			
			this.AudioPlayer = new Audio.Player();
			
			this.Renderer = new Graphics.Renderer();
			
			this.Renderer.Update += (double time) => 
			{
				Game.Update(time);

				// Update audio at 10 Hz
				if((Game.UpdateCount % 6) == 0)
					AudioPlayer.Update();
			};

			this.Renderer.Render += () => { Game.Render(); } ;
					
	        this.Input = new Input(Renderer.Window);
	        this.Game = new Game(this);
	 
	        this.Renderer.Run();
	    }
		
	}
}