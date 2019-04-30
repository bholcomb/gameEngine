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
using GpuNoise;
using Terrain;
using Editor;

namespace WorldEditor
{
   public class WorldEditor : GameWindow
   {
      public static int theWidth = 1900;
      public static int theHeigth = 1000;

      Initializer myInitializer;
      Viewport myViewport;
      Camera myCamera;
      RenderTarget myRenderTarget;

      GuiEventHandler myEventHandler;

      MainWindow myMainWindow;
      World myWorld;

      public WorldEditor()
         : base(theWidth, theHeigth, new GraphicsMode(32, 24, 0, 0), "World Editor", GameWindowFlags.Default, DisplayDevice.Default, 4, 5,
#if DEBUG
         GraphicsContextFlags.Debug)
#else
         GraphicsContextFlags.Default)
#endif
      {
         myInitializer = new Initializer(new String[] { "worldEditor.lua" });

         this.KeyUp += new EventHandler<KeyboardKeyEventArgs>(handleKeyboardUp);

         this.VSync = VSyncMode.Off;
         this.Width = theWidth;
         this.Height = theHeigth;
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
         if (major < 4 && minor < 5)
         {
            MessageBox.Show("You need at least OpenGL 4.5 to run this example. Aborting.", "Ooops", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            Exit();
         }
         System.Console.WriteLine("Found OpenGL Version: {0}.{1}", major, minor);

         GL.ClearColor(0.8f, 0.2f, 0.2f, 1.0f);

         myViewport = new Viewport(0, 0, theWidth, theHeigth);
         myCamera = new Camera(myViewport, 60.0f, 0.1f, 10000f);
         myEventHandler = new GuiEventHandler(this);
         myWorld = new World();
         myMainWindow = new MainWindow(this, myWorld);

         initRenderTarget(Width, Height);
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
         initRenderTarget(Width, Height);
      }

      protected override void OnUpdateFrame(FrameEventArgs e)
      {
         base.OnUpdateFrame(e);
         myCamera.updateCameraUniformBuffer();

         myWorld.myGenerator.update();
      }

      protected override void OnRenderFrame(FrameEventArgs e)
      {
         base.OnRenderFrame(e);

         //update the timers
         TimeSource.frameStep();

         renderUi();

         Renderer.render();
      }

      private void renderUi()
      {
         UI.beginFrame();

         myMainWindow.onGui();

         UI.endFrame();
      }

      public void initRenderTarget(int width, int height)
      {
         if (myRenderTarget != null)
         {
            myRenderTarget.Dispose();
            myRenderTarget = null;
         }

         List<RenderTargetDescriptor> rtdesc = new List<RenderTargetDescriptor>();
         rtdesc.Add(new RenderTargetDescriptor() { attach = FramebufferAttachment.ColorAttachment0, format = SizedInternalFormat.Rgba32f }); //creates a texture internally
         rtdesc.Add(new RenderTargetDescriptor() { attach = FramebufferAttachment.DepthAttachment, tex = new Texture(width, height, PixelInternalFormat.DepthComponent32f) }); //uses an existing texture

         myRenderTarget = new RenderTarget(width, height, rtdesc);
      }

      public void initRenderer()
      {
         Renderer.init(myInitializer.findData<InitTable>("renderer"));
         FontManager.init();

         //setup the rendering scene
         Graphics.View v = new Graphics.View("Main View", myCamera, myViewport);

         Pass p = new Pass("environment", "sky");
         p.renderTarget = myRenderTarget;
         p.filter = new TypeFilter(new List<String>() { "skybox" });
         p.clearColor = new Color4(0.8f, 0.2f, 0.2f, 1.0f);
         p.clearTarget = true; //false is default setting
         v.addPass(p);

         p = new Pass("terrain", "forward-lighting");
         p.filter = new TypeFilter(new List<String>() { "terrain" });
         v.addPass(p);

         p = new Pass("model", "forward-lighting");
         p.filter = new TypeFilter(new List<String>() { "light", "staticModel", "skinnedModel", "particle" });
         p.renderTarget = myRenderTarget; //go back to normal render target
         v.addPass(p);

         p = new DebugPass();
         v.addPass(p);

         //add some sub-views for debug graphics and UI
         Graphics.View uiView = new Graphics.View("UI", myCamera, myViewport);
         uiView.processRenderables = false;
         UIPass uiPass = new UIPass(myRenderTarget);
         uiView.addPass(uiPass);
         v.addSibling(uiView);

         //add the view
         Renderer.views[v.name] = v;

         //set the present function
         Renderer.present = present;
      }

      void present()
      {
         RenderCommand cmd = new BlitFrameBufferCommand(myRenderTarget, new Rect(0, 0, myRenderTarget.buffers[FramebufferAttachment.ColorAttachment0].width,
            myRenderTarget.buffers[FramebufferAttachment.ColorAttachment0].height), new Rect(0, 0, myCamera.viewport().width, myCamera.viewport().height));
         cmd.execute();

         SwapBuffers();
      }
   }

   class Program
   {
      static void Main(string[] args)
      {
         using (WorldEditor example = new WorldEditor())
         {
            example.Title = "World Editor";
            example.Run(30, 30);
         }

      }
   }
}
