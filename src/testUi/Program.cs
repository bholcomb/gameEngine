using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;

using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Platform;

using Util;
using Graphics;
using GUI;

namespace testUi
{
   public class TestHarness : GameWindow
   {
      public static int theWidth = 1280;
      public static int theHeight = 800;

      Viewport myViewport;
      Camera myCamera;
      GameWindowCameraEventHandler myCameraEventHandler;
      GUI.GuiEventHandler myUiEventHandler;
      Canvas myCanvas;

      //clear color
      float r = 0.2f;
      float g = 0.2f;
      float b = 0.2f;

      public TestHarness()
         : base(theWidth, theHeight, new GraphicsMode(32, 24, 0, 8), "Test UI", GameWindowFlags.Default, DisplayDevice.Default, 4, 4,
#if DEBUG
         GraphicsContextFlags.Debug)
#else
         GraphicsContextFlags.Default)
#endif
      {
         myViewport = new Viewport(0, 0, theWidth, theHeight);
         myCamera = new Camera(myViewport, 60.0f, 0.1f, 2000f);

         Keyboard.KeyUp += new EventHandler<KeyboardKeyEventArgs>(handleKeyboardUp);

         this.VSync = VSyncMode.Off;
      }

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
         if (major < 3 && minor < 3)
         {
            MessageBox.Show("You need at least OpenGL 3.3 to run this example. Aborting.", "Ooops", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            this.Exit();
         }
         System.Console.WriteLine("Found OpenGL Version: {0}.{1}", major, minor);

         GL.ClearColor(r, g, b, 1.0f);
         GL.Enable(EnableCap.Multisample);

         initRenderer();

         myCanvas = new Canvas();
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

      protected override void OnUpdateFrame(FrameEventArgs e)
      {
         base.OnUpdateFrame(e);
      }

      protected override void OnRenderFrame(FrameEventArgs e)
      {
         base.OnRenderFrame(e);

         //update the timers
         TimeSource.frameStep();

         RenderState rs = new RenderState();
         rs.force();

         GL.ClearColor(r, g, b, 1.0f);

         GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
         
         myCamera.updateCameraUniformBuffer();
         myCamera.bind();

         //canvas draw calls use screen space (bottom left origin)
         myCanvas.reset();
         myCanvas.setScreenResolution(UI.displaySize);
         myCanvas.setScale(1.0f);
         myCanvas.addCircle(new Vector2(100, 600), 75.0f, Color4.Red);
         myCanvas.addRectFilled(new Rect(300, 500, 600, 800), Color4.Blue, 10.0f);

         List<RenderCommand> cmds = new List<RenderCommand>();
         myCanvas.generateCommands(ref cmds);
         foreach (RenderCommand rc in cmds)
         {
            rc.execute();
         }

         UI.beginFrame();
         UI.label("Test");
         if (UI.button("click", new Vector2(100, 50)))
         {
            r = 1.0f;
            g = 0.2f;
            b = 0.2f;
         }

         UI.slider("R", ref r, 0.0f, 1.0f);
         UI.slider("G", ref g, 0.0f, 1.0f);
         UI.slider("B", ref b, 0.0f, 1.0f);

         UI.debug();
         UI.endFrame();

         cmds = UI.getRenderCommands();
         foreach(RenderCommand rc in cmds)
         {
             rc.execute();
         }

         SwapBuffers();
      }

      public void initRenderer()
      {
         double fps = TimeSource.fps();
         myUiEventHandler = new GuiEventHandler(this);
         UI.displaySize = new Vector2(theWidth, theHeight);
      }
   }

   class Program
   {
      static void Main(string[] args)
      {
         using (TestHarness example = new TestHarness())
         {
            example.Title = "Test UI";
            example.Run(30.0, 30.0);
         }

      }
   }
}
