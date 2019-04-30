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
      GUI.GuiEventHandler myUiEventHandler;

      Planet.Planet myPlanet;
      SkyBox mySkybox;
      GpuNoise.GpuNoiseCubeMap myNoise;

      StatelessDrawElementsCommand mySkyboxCmd;

      public TestHarness()
         : base(theWidth, theHeight, new GraphicsMode(32, 32, 0, 0), "Test Octahedron Planet", GameWindowFlags.Default, DisplayDevice.Default, 4, 5,
#if DEBUG
         GraphicsContextFlags.Debug)
#else
         GraphicsContextFlags.Default)
#endif
      {
         myViewport = new Viewport(0, 0, theWidth, theHeight);
         myCamera = new Camera(myViewport, 60.0f, 0.01f, 100000.0f);

         myCameraEventHandler = new GameWindowCameraEventHandler(myCamera, this);
         myCameraEventHandler.mouseConstrainedUp = false;

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

         GL.ClipControl(ClipOrigin.LowerLeft, ClipDepthMode.ZeroToOne);

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

         FontManager.init();
         myUiEventHandler = new GuiEventHandler(this);
         UI.displaySize = new Vector2(theWidth, theHeight);

         int size = 1024 * 4;
         myNoise = new GpuNoise.GpuNoiseCubeMap(size, size);

         myPlanet = new Planet.Planet(myCamera, myNoise.myCubemap);
         myCamera.position = new Vector3(0, 0, myPlanet.myScale + 5000.0f);
         myCamera.lookAt(new Vector3(0, 0, 0));

         SkyBoxDescriptor sbmd = new SkyBoxDescriptor("../data/skyboxes/space/space.json");
         mySkybox = Renderer.resourceManager.getResource(sbmd) as SkyBox;

         mySkyboxCmd = new StatelessDrawElementsCommand(mySkybox.mesh);
         List<ShaderDescriptor> desc = new List<ShaderDescriptor>();
         desc.Add(new ShaderDescriptor(ShaderType.VertexShader, "..\\src\\Graphics\\shaders\\skybox-vs.glsl", ShaderDescriptor.Source.File));
         desc.Add(new ShaderDescriptor(ShaderType.FragmentShader, "..\\src\\Graphics\\shaders\\skybox-ps.glsl", ShaderDescriptor.Source.File));
         ShaderProgramDescriptor sd = new ShaderProgramDescriptor(desc, null, "skyboxShader");
         ShaderProgram shader = Renderer.resourceManager.getResource(sd) as ShaderProgram;
         mySkyboxCmd.pipelineState.shaderState.shaderProgram = shader;
         mySkyboxCmd.pipelineState.vaoState.vao = new VertexArrayObject();
         mySkyboxCmd.pipelineState.vaoState.vao.bindVertexFormat(shader, V3.bindings());
         mySkyboxCmd.pipelineState.depthTest.enabled = false;
         mySkyboxCmd.pipelineState.depthWrite.enabled = false;
         mySkyboxCmd.pipelineState.culling.enabled = false;
         mySkyboxCmd.pipelineState.depthTest.depthFunc = DepthFunction.Lequal;
         mySkyboxCmd.pipelineState.generateId();
         CubemapTexture cubemap = mySkybox.mesh.material.myTextures[(int)Material.TextureId.Skybox].value() as CubemapTexture;
         mySkyboxCmd.renderState.setTexture((int)cubemap.id(), 0, TextureTarget.TextureCubeMap);
         mySkyboxCmd.renderState.setUniform(new UniformData(20, Uniform.UniformType.Int, 0)); // we know this location from the shader
         mySkyboxCmd.renderState.setVertexBuffer(SkyBox.theVbo.id, 0, 0, V3.stride);
         mySkyboxCmd.renderState.setIndexBuffer(SkyBox.theIbo.id);
         mySkyboxCmd.renderState.setUniformBuffer(myCamera.uniformBufferId(), 0);
      }

      protected override void OnUpdateFrame(FrameEventArgs e)
      {
         base.OnUpdateFrame(e);

         //update the camera
         myCameraEventHandler.tick((float)e.Time);

         myNoise.update();

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

         GL.ClearColor(0.2f, 0.2f, 0.2f, 0.0f);
         GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
         GL.DepthFunc(DepthFunction.Greater);

         myCamera.updateCameraUniformBuffer();
         myCamera.bind();

         
         mySkyboxCmd.execute();

         myPlanet.render();

         renderUi();

         avgFps = (0.99f * avgFps) + (0.01f * (float)TimeSource.fps());

         SwapBuffers();
      }

		private void renderUi()
      {
         UI.beginFrame();

         UI.beginWindow("Planet");
         UI.setWindowSize(new Vector2(300, 300), SetCondition.Always);
         UI.setWindowPosition(new Vector2(1250, 50), SetCondition.FirstUseEver);
         UI.label("FPS: {0:0.00}", avgFps);
         UI.slider("Minimum Edge Size", ref myPlanet.myMinEdgesize, 0.01f, 1.0f);
         UI.slider("Maximum Height", ref myPlanet.myMaxHeight, 0.0f, 5000.0f);
         UI.label("Height Above Surface: {0:0.00}", myCamera.position.Length - myPlanet.myScale);
         UI.label("Triangle count: {0}", myPlanet.myNextTri);
         UI.label("Index count: {0}", myPlanet.myIndexCount);
         UI.label("Vertex count: {0}", myPlanet.myVertCount);
         UI.checkbox("Freeze Rebuild", ref myPlanet.freezeRebuild);
         UI.checkbox("Draw Wireframe", ref myPlanet.myRenderWireframe);
         UI.endWindow();

         UI.beginWindow("Noise Editor");
         UI.setWindowSize(new Vector2(300, 400), SetCondition.Always);
         UI.setWindowPosition(new Vector2(1250, 400), SetCondition.FirstUseEver);

         UI.slider("Function", ref myNoise.function);
         UI.slider("Octaves", ref myNoise.octaves, 1, 10);
         UI.slider("Frequency", ref myNoise.frequency, 0.1f, 10.0f);
         UI.slider("lacunarity", ref myNoise.lacunarity, 1.0f, 3.0f);
         UI.slider("Gain", ref myNoise.gain, 0.01f, 2.0f);
         UI.slider("Offset", ref myNoise.offset, 0.0f, 10.0f);
         UI.slider("H", ref myNoise.H, 0.1f, 2.0f);
         if (UI.button("Capture Cubemap", new Vector2(75, 25)))
         {
            myNoise.myCubemap.saveData("heightMap");
         }
         UI.endWindow();

         UI.endFrame();

         List<RenderCommand> cmds = UI.getRenderCommands();
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
            example.Title = "Test Octahedron Planet";
            example.Run(30.0, 60.0);
         }
      }
   }
}
