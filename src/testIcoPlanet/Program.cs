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
         mySkyboxCmd.pipelineState.vaoState.vao.bindVertexFormat<V3>(shader);
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
			ImGui.beginFrame();

         ImGui.beginWindow("Planet");
         ImGui.setWindowSize(new Vector2(300, 300), SetCondition.Always);
         ImGui.setWindowPosition(new Vector2(1250, 50), SetCondition.FirstUseEver);
         ImGui.label("FPS: {0:0.00}", avgFps);
         ImGui.slider("Minimum Edge Size", ref myPlanet.myMinEdgesize, 0.01f, 1.0f);
         ImGui.slider("Maximum Height", ref myPlanet.myMaxHeight, 0.0f, 5000.0f);
         ImGui.label("Height Above Surface: {0:0.00}", myCamera.position.Length - myPlanet.myScale);
         ImGui.label("Triangle count: {0}", myPlanet.myNextTri);
         ImGui.label("Index count: {0}", myPlanet.myIndexCount);
         ImGui.label("Vertex count: {0}", myPlanet.myVertCount);
         ImGui.checkbox("Freeze Rebuild", ref myPlanet.freezeRebuild);
         ImGui.checkbox("Draw Wireframe", ref myPlanet.myRenderWireframe);
         ImGui.endWindow();

         ImGui.beginWindow("Noise Editor");
         ImGui.setWindowSize(new Vector2(300, 400), SetCondition.Always);
         ImGui.setWindowPosition(new Vector2(1250, 400), SetCondition.FirstUseEver);

         ImGui.slider("Function", ref myNoise.function);
         ImGui.slider("Octaves", ref myNoise.octaves, 1, 10);
         ImGui.slider("Frequency", ref myNoise.frequency, 0.1f, 10.0f);
         ImGui.slider("lacunarity", ref myNoise.lacunarity, 1.0f, 3.0f);
         ImGui.slider("Gain", ref myNoise.gain, 0.01f, 2.0f);
         ImGui.slider("Offset", ref myNoise.offset, 0.0f, 10.0f);
         ImGui.slider("H", ref myNoise.H, 0.1f, 2.0f);
         if (ImGui.button("Capture Cubemap", new Vector2(75, 25)))
         {
            myNoise.myCubemap.saveData("heightMap");
         }
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
            example.Title = "Test Octahedron Planet";
            example.Run(30.0, 60.0);
         }
      }
   }
}
