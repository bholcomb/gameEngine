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
      GuiEventHandler myEventHandler;

      MainWindow myMainWindow;
      Generator myGenerator;

      ShaderProgram myDisplayBiomeShader;

      public WorldEditor()
         : base(theWidth, theHeigth, new GraphicsMode(32, 24, 0, 0), "World Editor", GameWindowFlags.Default, DisplayDevice.Default, 4, 5,
#if DEBUG
         GraphicsContextFlags.Debug)
#else
         GraphicsContextFlags.Default)
#endif
      {
         myInitializer = new Initializer(new String[] { "worldEditor.lua" });
         myViewport = new Viewport(0, 0, theWidth, theHeigth);
         myCamera = new Camera(myViewport, 60.0f, 0.1f, 2000f);

         this.KeyUp += new EventHandler<KeyboardKeyEventArgs>(handleKeyboardUp);

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
         if (major < 4 && minor < 5)
         {
            MessageBox.Show("You need at least OpenGL 4.5 to run this example. Aborting.", "Ooops", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            Exit();
         }
         System.Console.WriteLine("Found OpenGL Version: {0}.{1}", major, minor);

         GL.ClearColor(0.8f, 0.2f, 0.2f, 1.0f);

         myEventHandler = new GuiEventHandler(this);

         List<ShaderDescriptor> shadersDesc = new List<ShaderDescriptor>();
         shadersDesc.Add(new ShaderDescriptor(ShaderType.VertexShader, "World Editor.shaders.cube-vs.glsl"));
         shadersDesc.Add(new ShaderDescriptor(ShaderType.FragmentShader, "World Editor.shaders.display-biome-ps.glsl"));
         ShaderProgramDescriptor sd = new ShaderProgramDescriptor(shadersDesc);
         myDisplayBiomeShader = Renderer.resourceManager.getResource(sd) as ShaderProgram;

         FontManager.init();

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

         myGenerator.update();

         //clear renderstate (especially scissor)
         RenderState rs = new RenderState();
         rs.apply();

         GL.ClearColor(0.2f, 0.2f, 0.2f, 1.0f);
         GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

         RenderCubemapSphere cmd = new RenderCubemapSphere(new Vector3(0, 0, -5), 2.0f, myGenerator.myBiomeMap, true);
         cmd.renderState.setUniformBuffer(myCamera.uniformBufferId(), 0);
         cmd.pipelineState.shaderState.shaderProgram = myDisplayBiomeShader;
         cmd.renderState.setUniform(new UniformData(21, Uniform.UniformType.Bool, showElevation)); //show elevation
         cmd.renderState.setUniform(new UniformData(22, Uniform.UniformType.Bool, showWater)); //show water
         cmd.renderState.setUniform(new UniformData(23, Uniform.UniformType.Bool, showHeat)); //show heat
         cmd.renderState.setUniform(new UniformData(24, Uniform.UniformType.Bool, showMoisture)); //show moisture
         cmd.renderState.setUniform(new UniformData(25, Uniform.UniformType.Bool, showBiome)); //show biome
         cmd.execute();

         renderUi();

         SwapBuffers();
      }

      bool showElevation = false;
      bool showWater = false;
      bool showHeat = false;
      bool showMoisture = false;
      bool showBiome = false;

      float avgFps = 0;
      private void renderUi()
      {
         UI.beginFrame();

         //myMainWindow.onGui();

         bool closed= false;
         UI.beginWindow("Views", ref closed, Window.Flags.RootWindow);
         UI.setWindowLayout(Layout.Direction.Horizontal);
         if (UI.button("Elevation", new Vector2(150, 20)) == true)
         {
            showElevation = !showElevation;
         }
         if (UI.button("Water", new Vector2(150, 20)) == true)
         {
            showWater = !showWater;
         }
         if (UI.button("Heat", new Vector2(150, 20)) == true)
         {
            showHeat = !showHeat;
         }
         if (UI.button("Moisture", new Vector2(150, 20)) == true)
         {
            showMoisture = !showMoisture;
         }
         if (UI.button("Biome", new Vector2(150, 20)) == true)
         {
            showBiome = !showBiome;
         }

         if (showElevation)
         {
            UI.beginWindow("Elevation Editor");
            UI.setWindowSize(new Vector2(300, 400), SetCondition.Always);
            UI.setWindowPosition(new Vector2(1250, 200), SetCondition.FirstUseEver);

            UI.slider("Function", ref myGenerator.elevation.function);
            UI.slider("Octaves", ref myGenerator.elevation.octaves, 1, 10);
            UI.slider("Frequency", ref myGenerator.elevation.frequency, 0.1f, 4.0f);
            UI.slider("lacunarity", ref myGenerator.elevation.lacunarity, 1.0f, 3.0f);
            UI.slider("Gain", ref myGenerator.elevation.gain, 0.1f, 2.0f);
            UI.slider("Offset", ref myGenerator.elevation.offset, -1.0f, 1.0f);
            UI.slider("H", ref myGenerator.elevation.H, 0.1f, 2.0f);

            UI.endWindow();
         }

         if (showHeat)
         {
            UI.beginWindow("Heat Editor");
            UI.setWindowSize(new Vector2(300, 400), SetCondition.Always);
            UI.setWindowPosition(new Vector2(1250, 200), SetCondition.FirstUseEver);

            UI.slider("Function", ref myGenerator.heat.function);
            UI.slider("Octaves", ref myGenerator.heat.octaves, 1, 10);
            UI.slider("Frequency", ref myGenerator.heat.frequency, 0.1f, 10.0f);
            UI.slider("lacunarity", ref myGenerator.heat.lacunarity, 1.0f, 3.0f);
            UI.slider("Gain", ref myGenerator.heat.gain, 0.01f, 2.0f);
            UI.slider("Offset", ref myGenerator.heat.offset, 0.0f, 10.0f);
            UI.slider("H", ref myGenerator.heat.H, 0.1f, 2.0f);

            UI.separator();
            UI.label("South Gradient");
            UI.slider("sx0", ref myGenerator.heat.sx0, 0.0f, 1.0f);
            UI.slider("sx1", ref myGenerator.heat.sx1, 0.0f, 1.0f);
            UI.slider("sy0", ref myGenerator.heat.sy0, 0.0f, 1.0f);
            UI.slider("sy1", ref myGenerator.heat.sy1, 0.0f, 1.0f);

            UI.separator();
            UI.label("North Gradient");
            UI.slider("nx0", ref myGenerator.heat.nx0, 0.0f, 1.0f);
            UI.slider("nx1", ref myGenerator.heat.nx1, 0.0f, 1.0f);
            UI.slider("ny0", ref myGenerator.heat.ny0, 0.0f, 1.0f);
            UI.slider("ny1", ref myGenerator.heat.ny1, 0.0f, 1.0f);

            UI.endWindow();
         }

         if (showMoisture)
         {
            UI.beginWindow("Moisture Editor");
            UI.setWindowSize(new Vector2(300, 400), SetCondition.Always);
            UI.setWindowPosition(new Vector2(1250, 200), SetCondition.FirstUseEver);

            UI.slider("Function", ref myGenerator.moisture.function);
            UI.slider("Octaves", ref myGenerator.moisture.octaves, 1, 10);
            UI.slider("Frequency", ref myGenerator.moisture.frequency, 0.1f, 10.0f);
            UI.slider("lacunarity", ref myGenerator.moisture.lacunarity, 1.0f, 3.0f);
            UI.slider("Gain", ref myGenerator.moisture.gain, 0.01f, 2.0f);
            UI.slider("Offset", ref myGenerator.moisture.offset, 0.0f, 10.0f);
            UI.slider("H", ref myGenerator.moisture.H, 0.1f, 2.0f);

            UI.endWindow();
         }
         //UI.debug();


         UI.endFrame();


         List<RenderCommand> cmds = UI.getRenderCommands();
         foreach (RenderCommand rc in cmds)
         {
            StatelessRenderCommand src = rc as StatelessRenderCommand;
            if (src != null)
            {
               src.renderState.setUniformBuffer(myCamera.uniformBufferId(), 0);
            }

            rc.execute();
         }
      }

      public void initRenderer()
      {
         myMainWindow = new MainWindow(this);
         myGenerator = new Generator();
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
