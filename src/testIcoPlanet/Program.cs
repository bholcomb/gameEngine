using System;
using System.Collections.Generic;
using System.Windows.Forms;

using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using Util;
using Graphics;
using UI;
using GpuNoise;

using Planet;


namespace testNoise
{
   public class TestHarness : GameWindow
   {
      public static int theWidth = 1600;
      public static int theHeight = 900;

      Viewport myViewport;
      Camera myCamera;
      GameWindowCameraEventHandler myCameraEventHandler;
      UI.GuiEventHandler myUiEventHandler;

      Planet.Planet myPlanet;
      Graphics.Font myFont;

      public TestHarness()
         : base(theWidth, theHeight, new GraphicsMode(32, 24, 0, 0), "Test Icosahedron Planet", GameWindowFlags.Default, DisplayDevice.Default, 4, 5,
#if DEBUG
         GraphicsContextFlags.Debug)
#else
         GraphicsContextFlags.Default)
#endif
      {
         myViewport = new Viewport(0, 0, theWidth, theHeight);
         myCamera = new Camera(myViewport, 60.0f, 0.01f, 10000.0f);

         myCameraEventHandler = new GameWindowCameraEventHandler(myCamera, this);

         Keyboard.KeyUp += new EventHandler<KeyboardKeyEventArgs>(handleKeyboardUp);

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

         if(e.Key == Key.BackSpace)
         {
            myPlanet.reset();
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
         Renderer.init();

         myUiEventHandler = new GuiEventHandler(this);
         ImGui.displaySize = new Vector2(theWidth, theHeight);

         myFont = FontManager.findFont("CONSOLA");

         myPlanet = new Planet.Planet(myCamera);
         myCamera.position = new Vector3(0, 0, myPlanet.myScale + 1000.0f);
         myCamera.lookAt(new Vector3(0, 0, 0));
      }

      protected override void OnUpdateFrame(FrameEventArgs e)
      {
         base.OnUpdateFrame(e);

         //update the camera
         myCameraEventHandler.tick((float)e.Time);

         myPlanet.update();
		}

      float avgFps = 0.0f;

      protected override void OnRenderFrame(FrameEventArgs e)
      {
         base.OnRenderFrame(e);

         //update the timers
         TimeSource.frameStep();

         RenderState rs = new RenderState();
         rs.apply();

         GL.ClearColor(0.2f, 0.2f, 0.2f, 1.0f);
         GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

         myCamera.updateCameraUniformBuffer();
         myCamera.bind();

         myPlanet.render();

         renderUi();

         avgFps = (0.99f * avgFps) + (0.01f * (float)TimeSource.fps());
         //myFont.print(20, 20, "FPS: {0:0.00}", avgFps);

         SwapBuffers();
      }

		private void renderUi()
      {
			ImGui.beginFrame();

         ImGui.beginWindow("Planet");
         ImGui.setWindowSize(new Vector2(300, 300), SetCondition.Always);
         ImGui.setWindowPosition(new Vector2(1250, 50), SetCondition.FirstUseEver);
         ImGui.label("FPS: {0:0.00}", avgFps);
         ImGui.slider("Minimum Edge Size", ref myPlanet.myMinEdgesize, 0.01f, 1.0f);
         ImGui.label("Height Above Surface: {0:0.00}", myCamera.position.Length - myPlanet.myScale);
         ImGui.label("Triangle count: {0}", myPlanet.myNextTri);
         ImGui.checkbox("Freeze Rebuild", ref myPlanet.freezeRebuild);
         ImGui.endWindow();

         ImGui.endFrame();

         List<RenderCommand> cmds = ImGui.getRenderCommands();
         foreach (RenderCommand rc in cmds)
         {
				StatelessRenderCommand src = rc as StatelessRenderCommand;
				if(src != null)
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
            example.Title = "Test Icosahedron Planet";
            example.Run(30.0, 0.0);
         }
      }
   }
}
