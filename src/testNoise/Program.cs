using System;
using System.Collections.Generic;
using System.Windows.Forms;

using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using Util;
using Graphics;
using GUI;
using GpuNoise;


namespace testNoise
{
	public class TestHarness : GameWindow
	{
		public static int theWidth = 1600;
		public static int theHeight = 900;

		Viewport myViewport;
		Camera myCamera;
		GameWindowCameraEventHandler myCameraEventHandler;
		GUI.GuiEventHandler myUiEventHandler;

		ModuleTree myNoiseTree;

		public TestHarness()
			: base(theWidth, theHeight, new GraphicsMode(32, 24, 0, 0), "Test Noise", GameWindowFlags.Default, DisplayDevice.Default, 4, 5,
#if DEBUG
			GraphicsContextFlags.Debug)
#else
         GraphicsContextFlags.Default)
#endif
		{
			myViewport = new Viewport(0, 0, theWidth, theHeight);
			myCamera = new Camera(myViewport, 60.0f, 0.1f, 2000f);

			this.KeyUp += new EventHandler<KeyboardKeyEventArgs>(handleKeyboardUp);

			this.VSync = VSyncMode.Off;
		}

		#region boilerplate
		public void handleKeyboardUp(object sender, KeyboardKeyEventArgs e)
		{
			if (e.Key == Key.Escape)
			{
				DialogResult res = MessageBox.Show("Are you sure", "Quit", MessageBoxButtons.OKCancel);
				if (res == DialogResult.OK)
				{
					Exit();
				}
			}
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			string version = GL.GetString(StringName.Version);
			int major = System.Convert.ToInt32(version[0].ToString());
			int minor = System.Convert.ToInt32(version[2].ToString());
			if (major < 4 && minor < 4)
			{
				MessageBox.Show("You need at least OpenGL 4.4 to run this example. Aborting.", "Ooops", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				this.Exit();
			}
			System.Console.WriteLine("Found OpenGL Version: {0}.{1}", major, minor);

			initRenderer();
		}

		protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
		{
			base.OnClosing(e);
		}

		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);
			myViewport.width = Width;
			myViewport.height = Height;
			myViewport.apply();
		}

		#endregion

		public void initRenderer()
		{
			int maxImageUnits = 0;
			GL.GetInteger((GetPName)All.MaxImageUnits, out maxImageUnits);
			Console.WriteLine("Max image units: {0}", maxImageUnits);

			double fps = TimeSource.fps();
         FontManager.init();
			myUiEventHandler = new GuiEventHandler(this);
			UI.displaySize = new Vector2(theWidth, theHeight);

			//create tree
			int size = 1024;
			myNoiseTree = new ModuleTree(size, size);

			//build tree
			Fractal2d f = myNoiseTree.addModule(Module.Type.Fractal2d, "fractal") as Fractal2d;
			AutoCorrect ac = myNoiseTree.addModule(Module.Type.AutoCorrect, "autocorrect") as AutoCorrect;
			AutoCorrect ac2 = myNoiseTree.addModule(Module.Type.AutoCorrect, "autocorrect2") as AutoCorrect;
			Combiner comb = myNoiseTree.addModule(Module.Type.Combiner, "combiner") as Combiner;
			Constant c = myNoiseTree.addModule(Module.Type.Constant, "constant") as Constant;
			Gradient g = myNoiseTree.addModule(Module.Type.Gradient, "gradient") as Gradient;
			Scale s = myNoiseTree.addModule(Module.Type.Scale, "scale") as Scale;
			Constant sc = myNoiseTree.addModule(Module.Type.Constant, "scaleVal") as Constant;
			Select sel = myNoiseTree.addModule(Module.Type.Select, "select") as Select;
			Fractal2d f2 = myNoiseTree.addModule(Module.Type.Fractal2d, "f2") as Fractal2d;
			Translate t = myNoiseTree.addModule(Module.Type.Translate, "translate") as Translate;
			Constant tx = myNoiseTree.addModule(Module.Type.Constant, "tx") as Constant;
			Constant ty = myNoiseTree.addModule(Module.Type.Constant, "ty") as Constant;

			//link modules
			ac.source = f;
			ac2.source = f2;
			
			sel.low = ac;
			sel.high = ac2;
			sel.control = g;

			//set the output
			myNoiseTree.final.source = sel;
		}

		protected override void OnUpdateFrame(FrameEventArgs e)
		{
			base.OnUpdateFrame(e);
			myCamera.updateCameraUniformBuffer();
		}

		protected override void OnRenderFrame(FrameEventArgs e)
		{
			base.OnRenderFrame(e);

			//update the timers
			TimeSource.frameStep();

			myNoiseTree.update();

			//clear renderstate (especially scissor)
			RenderState rs = new RenderState();
			rs.apply();

			GL.ClearColor(0.2f, 0.2f, 0.2f, 1.0f);
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

			myCamera.bind();
			// 
			// 			int size = 275;
			// 			int posX = 100;
			// 			int posY = 300;
			int size = 800;
			int posX = 10;
			int posY = 10;
			RenderTexture2dCommand cmd;

			cmd = new RenderTexture2dCommand(new Vector2(posX, posY), new Vector2(posX + size, posY + size), myNoiseTree.final.output);
			cmd.renderState.setUniformBuffer(myCamera.uniformBufferId(), 0);
			cmd.execute();
			/*
						//-x left
						cmd = new RenderTexture2dCommand(new Vector2(posX, posY), new Vector2(posX + size, posY + size), myNoise.myTextures[1]);
						cmd.renderState.setUniformBuffer(myCamera.uniformBufferId(), 0);
						cmd.execute();

						//-z front
						posX += size;
						cmd = new RenderTexture2dCommand(new Vector2(posX, posY), new Vector2(posX + size, posY + size), myNoise.myTextures[4]);
						cmd.renderState.setUniformBuffer(myCamera.uniformBufferId(), 0);
						cmd.execute();

						//+Y  top
						posY += size;
						cmd = new RenderTexture2dCommand(new Vector2(posX, posY), new Vector2(posX + size, posY + size), myNoise.myTextures[3]);
						cmd.renderState.setUniformBuffer(myCamera.uniformBufferId(), 0);
						cmd.execute();

						//-Y  bottom
						posY -= size * 2;
						cmd = new RenderTexture2dCommand(new Vector2(posX, posY), new Vector2(posX + size, posY + size), myNoise.myTextures[2]);
						cmd.renderState.setUniformBuffer(myCamera.uniformBufferId(), 0);
						cmd.execute();

						//+x  right
						posX += size;
						posY += size;
						cmd = new RenderTexture2dCommand(new Vector2(posX, posY), new Vector2(posX + size, posY + size), myNoise.myTextures[0]);
						cmd.renderState.setUniformBuffer(myCamera.uniformBufferId(), 0);
						cmd.execute();

						//+z  back
						posX += size;
						cmd = new RenderTexture2dCommand(new Vector2(posX, posY), new Vector2(posX + size, posY + size), myNoise.myTextures[5]);
						cmd.renderState.setUniformBuffer(myCamera.uniformBufferId(), 0);
						cmd.execute();

						RenderCubemapSphere cmd2 = new RenderCubemapSphere(new Vector3(0, 0, -5), 2.0f,  myNoise.myCubemap, true);
						cmd2.renderState.setUniformBuffer(myCamera.uniformBufferId(), 0);
						cmd2.execute();
			*/


			renderUi();


			SwapBuffers();
		}

		float avgFps = 0;
		private void renderUi()
		{
			avgFps = (0.99f * avgFps) + (0.01f * (float)TimeSource.fps());
			UI.beginFrame();
			UI.label("FPS: {0:0.00}", avgFps);

			UI.beginWindow("Noise Editor");
			UI.setWindowSize(new Vector2(300, 800), SetCondition.Always);
			UI.setWindowPosition(new Vector2(1250, 50), SetCondition.FirstUseEver);

			Fractal f = myNoiseTree.findModule("fractal") as Fractal;
			UI.label("FractalOne---");
			UI.slider("Function", ref f.function);
			UI.slider("Octaves", ref f.octaves, 1, 10);
			UI.slider("Frequency", ref f.frequency, 0.1f, 10.0f);
			UI.slider("lacunarity", ref f.lacunarity, 1.0f, 3.0f);
			UI.slider("Gain", ref f.gain, 0.01f, 2.0f);
			UI.slider("Offset", ref f.offset, 0.0f, 10.0f);
			UI.slider("H", ref f.H, 0.1f, 2.0f);
			UI.separator();


			Fractal f2 = myNoiseTree.findModule("f2") as Fractal;
			UI.label("Fractal Two---");
			UI.slider("Function ", ref f2.function);
			UI.slider("Octaves ", ref f2.octaves, 1, 10);
			UI.slider("Frequency ", ref f2.frequency, 0.1f, 10.0f);
			UI.slider("lacunarity ", ref f2.lacunarity, 1.0f, 3.0f);
			UI.slider("Gain ", ref f2.gain, 0.01f, 2.0f);
			UI.slider("Offset ", ref f2.offset, 0.0f, 10.0f);
			UI.slider("H ", ref f2.H, 0.1f, 2.0f);
			UI.separator();

			Gradient g = myNoiseTree.findModule("gradient") as Gradient;
			UI.label("Gradient---");
			UI.slider("X0", ref g.x0, 0.0f, 1.0f);
			UI.slider("X1", ref g.x1, 0.0f, 1.0f);
			UI.slider("Y0", ref g.y0, 0.0f, 1.0f);
			UI.slider("Y1", ref g.y1, 0.0f, 1.0f);
			UI.separator();

			Select s = myNoiseTree.findModule("select") as Select;
			UI.label("Select---");
			UI.slider("Threshold", ref s.threshold, 0.0f, 1.0f);
			UI.slider("Falloff", ref s.falloff, 0.0f, 1.0f);
			UI.separator();

			UI.endWindow();

			UI.endFrame();

			List<RenderCommand> cmds = UI.getRenderCommands();
			foreach (RenderCommand rc in cmds)
			{
				StatelessRenderCommand src = rc as StatelessRenderCommand;
				if (src != null)
					src.renderState.setUniformBuffer(myCamera.uniformBufferId(), 0);

				rc.execute();
			}
		}
	}

	class Program
	{
		static void Main(string[] args)
		{
			using (TestHarness example = new TestHarness())
			{
				example.Title = "Test Noise";
				example.Run(30.0, 0.0);
			}

		}
	}
}
