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
using Terrain;
using UI;

namespace testRenderer
{
	public class TestRenderer : GameWindow
	{
		public static int theWidth = 1280;
		public static int theHeigth = 800;

		Initializer myInitializer;
		Viewport myViewport;
		Camera myCamera;
		GameWindowCameraEventHandler myCameraEventHandler;
		UI.GuiEventHandler myUiEventHandler;
		RenderTarget myRenderTarget;
		SkinnedModelRenderable mySkinnedModel;
      LightRenderable mySun;
      LightRenderable myPoint1;
      LightRenderable myPoint2;

		bool myShowRenderStats = true;

		World myWorld;
		TerrainRenderManager myTerrainRenderManager;

		public TestRenderer()
			: base(theWidth, theHeigth, new GraphicsMode(32, 24, 0, 0), "Haven Test", GameWindowFlags.Default, DisplayDevice.Default, 4, 4,
#if DEBUG
         GraphicsContextFlags.Debug)
#else
			GraphicsContextFlags.Default)
#endif
		{
			myInitializer = new Initializer(new String[] { "testRenderer.lua" });
			Renderer.init();
			myViewport = new Viewport(this);
			myCamera = new Camera(myViewport, 60.0f, 0.1f, 1000.0f);

			myCameraEventHandler = new GameWindowCameraEventHandler(myCamera, this);

			Keyboard.KeyUp += new EventHandler<KeyboardKeyEventArgs>(handleKeyboardUp);

			this.VSync = VSyncMode.Off;
		}

		public void handleKeyboardUp(object sender, KeyboardKeyEventArgs e)
		{
			if (e.Key == Key.Tab)
			{
				//myEditor.toggleEnabled();
			}

			if (e.Key == Key.Escape)
			{
				DialogResult res = MessageBox.Show("Are you sure", "Quit", MessageBoxButtons.OKCancel);
				if (res == DialogResult.OK)
				{
					Exit();
				}
			}

			if (e.Key == Key.F1)
			{
				myShowRenderStats = !myShowRenderStats;
			}

			if (e.Key == Key.F5)
			{
				Renderer.shaderManager.reloadShaders();
			}

			if (e.Key == Key.F8)
			{
				myCamera.toggleDebugging();
			}

			if (e.Key == Key.F10)
			{
				//myWorld.newWorld();
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

			GL.ClearColor(0.2f, 0.2f, 0.2f, 1.0f);
			GL.ClearDepth(1.0f);

			initRenderTarget();
			initRenderer();

			myCamera.position = new Vector3(0, 2, 10);

			DebugRenderer.enabled = true;

			myWorld = new World(myInitializer);
			myWorld.init();
			myTerrainRenderManager = new TerrainRenderManager(myWorld);
			myTerrainRenderManager.init();
			myWorld.newWorld();
		}

		protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
		{
			base.OnClosing(e);
			myWorld.shutdown();
		}

		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);

			initRenderTarget();
		}

		float avgFps = 0;
		protected override void OnUpdateFrame(FrameEventArgs e)
		{
			base.OnUpdateFrame(e);

			//update the camera
			myCameraEventHandler.tick((float)e.Time);

         //update the animated object
			mySkinnedModel.update((float)TimeSource.timeThisFrame());

         //update the sun and point lights
         double now = TimeSource.currentTime();
         mySun.position = new Vector3((float)Math.Sin(now) * 5.0f, 5.0f, (float)Math.Cos(now) * 5.0f);
         myPoint1.position = new Vector3(2.5f, 1, 0) + new Vector3(0.0f, (float)Math.Sin(now), (float)Math.Cos(now));
         myPoint2.position = new Vector3(5.5f, 1, 0) + new Vector3(0.0f, (float)Math.Cos(now), (float)Math.Sin(now));

         //high frequency filter on avg fps
         avgFps = (0.99f * avgFps) + (0.01f * (float)TimeSource.fps());

			ImGui.beginFrame();
			if(ImGui.beginWindow("Render Stats", ref myShowRenderStats, Window.Flags.DefaultWindow))
			{
				ImGui.setWindowPosition(new Vector2(20, 50), SetCondition.FirstUseEver);
				ImGui.setWindowSize(new Vector2(500, 650), SetCondition.FirstUseEver);
				ImGui.label("FPS: {0:0.00}", avgFps);
            ImGui.label("Camera position: {0}", myCamera.position);
            ImGui.label("Camera view vector: {0}", myCamera.viewVector);
            ImGui.separator();
            ImGui.label("Frame Time: {0:0.00}ms", (Renderer.stats.cullTime + Renderer.stats.prepareTime + Renderer.stats.generateTime + Renderer.stats.executeTime)* 1000.0);
            ImGui.label("   Cull Time: {0:0.00}ms", Renderer.stats.cullTime * 1000.0);
				ImGui.label("   Prepare Time: {0:0.00}ms", Renderer.stats.prepareTime * 1000.0);
				ImGui.label("   Submit Time: {0:0.00}ms", Renderer.stats.generateTime * 1000.0);
				ImGui.label("   Execute Time: {0:0.00}ms", Renderer.stats.executeTime * 1000.0);
				ImGui.separator();
				ImGui.label("Camera Visible Renderables");
				for (int i = 0; i < Renderer.stats.cameraVisibles.Count; i++)
				{
					ImGui.label("Camera {0}: {1} / {2}", i, Renderer.stats.cameraVisibles[i], Renderer.stats.renderableCount);
				}
				ImGui.separator();
				ImGui.label("View Stats");
				for (int i = 0; i < Renderer.stats.viewStats.Count; i++)
				{
					ImGui.label("{0} {1}", i, Renderer.stats.viewStats[i].name);
               ImGui.label("   Command List count {0}", Renderer.stats.viewStats[i].commandLists);
               ImGui.label("   Passes {0}", Renderer.stats.viewStats[i].passStats.Count);
               for(int j = 0; j < Renderer.stats.viewStats[i].passStats.Count; j++)
               {
                  PassStats ps = Renderer.stats.viewStats[i].passStats[j];
                  ImGui.label("      {0} ({1})", ps.name, ps.technique);
                  ImGui.label("         Queues: {0}", ps.queueCount);
                  ImGui.label("         Render Commands: {0}", ps.renderCalls);
               }               
				}

				ImGui.endWindow();
			}

			//ImGui.debug();
			ImGui.endFrame();

			//get any new chunks based on the camera position
			myWorld.setInterest(myCamera.position);
			myWorld.tick(e.Time);
		}

		protected override void OnRenderFrame(FrameEventArgs e)
		{
			base.OnRenderFrame(e);

			Renderer.render();

			//update the timers
			TimeSource.frameStep();
		}

		void initRenderTarget()
		{
			//init render target
			int x, y;
			x = myCamera.viewport().width;
			y = myCamera.viewport().height;

			List<RenderTargetDescriptor> rtdesc = new List<RenderTargetDescriptor>();
			rtdesc.Add(new RenderTargetDescriptor() { attach = FramebufferAttachment.ColorAttachment0, format = SizedInternalFormat.Rgba32f }); //creates a texture internally
			rtdesc.Add(new RenderTargetDescriptor() { attach = FramebufferAttachment.DepthAttachment, tex = new Texture(x, y, PixelInternalFormat.DepthComponent32f) }); //uses an existing texture

			if (myRenderTarget == null)
			{
				myRenderTarget = new RenderTarget(x, y, rtdesc);
			}
			else
			{
				myRenderTarget.update(x, y, rtdesc);
			}
		}

		void handleViewportChanged(int x, int y, int w, int h)
		{
			initRenderTarget();
		}

		void present()
		{
			RenderCommand cmd = new BlitFrameBufferCommand(myRenderTarget);
			cmd.execute();

			SwapBuffers();
		}

		public void initRenderer()
		{
			myUiEventHandler = new GuiEventHandler(this);

			//setup the main render target
			myRenderTarget = new RenderTarget();
			initRenderTarget();

         //setup the rendering scene
         Graphics.View v = new Graphics.View("Main View", myCamera, myViewport);

         Pass p = new Pass("environment", "skybox");
         p.renderTarget = myRenderTarget;
         p.filter = new TypeFilter(new List<String>() { "skybox" });
         p.clearTarget = true; //false is default setting
         v.addPass(p);

         p = new Pass("terrain", "forward-lighting");
         p.filter = new TypeFilter(new List<String>() { "terrain" });
         v.addPass(p);
         
         p = new Pass("model", "forward-lighting");
         p.filter = new TypeFilter(new List<String>() { "light", "staticModel", "skinnedModel"});
         v.addPass(p);

         //add some sub-views for debug graphics and UI
         v.addSibling(new Graphics.DebugView("Debug View", myCamera, myViewport, myRenderTarget));
         v.addSibling(new UI.GuiView("UI View", myCamera, myViewport, myRenderTarget));

         //add the view
         Renderer.views[v.name] = v;         

         //set the present function
			Renderer.present = present;

         //setup the scene by adding a bunch of lights and renderables

			//create the skybox renderable
			SkyboxRenderable skyboxRenderable = new SkyboxRenderable();
			SkyBoxDescriptor sbmd = new SkyBoxDescriptor("../data/skyboxes/interstellar/interstellar.json");
			skyboxRenderable.model = Renderer.resourceManager.getResource(sbmd) as SkyBox;
			Renderer.renderables.Add(skyboxRenderable);

			//create a tree instance
			Random rand = new Random(230877);
			for (int i = 0; i < 10000; i++)
			{
				int size = 500;
				int halfSize = size / 2;
				StaticModelRenderable smr = new StaticModelRenderable();
				ObjModelDescriptor mdesc;
				if (i % 2 == 0)
					mdesc = new ObjModelDescriptor("../data/models/vegitation/fir/fir2.obj");
				else
					mdesc = new ObjModelDescriptor("../data/models/props/rocks_3_by_nobiax-d6s8l2b/rocks_03-blend.obj");

				smr.model = Renderer.resourceManager.getResource(mdesc) as StaticModel;
				smr.setPosition(new Vector3((rand.Next() % size) - halfSize, 0, (rand.Next() % size) - halfSize));
            //for(int j=0; j< smr.model.myMeshes.Count; j++)
 				//   smr.model.myMeshes[j].material.myFeatures = Graphics.Material.Feature.DiffuseMap; //turn off lighting
				Renderer.renderables.Add(smr);
			}
          
         //create a test cube
         StaticModelRenderable testRenderable = new StaticModelRenderable();
         ObjModelDescriptor testDesc;
         testDesc = new ObjModelDescriptor("../data/models/props/testCube/testCube.obj");
         testRenderable.model = Renderer.resourceManager.getResource(testDesc) as StaticModel;
         Renderer.renderables.Add(testRenderable);
         testRenderable.setPosition(new Vector3(0, 1, 0));

         //create a skinned model instance
         mySkinnedModel = new SkinnedModelRenderable();
			//MS3DModelDescriptor skmd = new MS3DModelDescriptor("../data/models/characters/zombie/zombie.json");
			IQModelDescriptor skmd = new IQModelDescriptor("../data/models/characters/mrFixIt/mrFixIt.json");
			mySkinnedModel.model = Renderer.resourceManager.getResource(skmd) as SkinnedModel;
			mySkinnedModel.controllers.Add(new AnimationController(mySkinnedModel));
			Renderer.renderables.Add(mySkinnedModel);
			mySkinnedModel.setPosition(new Vector3(5, 0, 0));
			(mySkinnedModel.findController("animation") as AnimationController).startAnimation("idle");

			//add a sun for light
			mySun = new LightRenderable();
         mySun.myLightType = LightRenderable.Type.DIRECTIONAL;
         mySun.color = Color4.White;
         mySun.position = new Vector3(5, 5, 5);
			Renderer.renderables.Add(mySun);

			//add a point light
			myPoint1 = new LightRenderable();
			myPoint1.myLightType = LightRenderable.Type.POINT;
			myPoint1.color = Color4.Red;
			myPoint1.position = new Vector3(2.5f, 1, 0);
			myPoint1.size = 10;
			myPoint1.linearAttenuation = 1.0f;
         myPoint1.quadraticAttenuation = 0.5f;
			Renderer.renderables.Add(myPoint1);

			//add another point light
			myPoint2 = new LightRenderable();
			myPoint2.myLightType = LightRenderable.Type.POINT;
			myPoint2.color = Color4.Blue;
			myPoint2.position = new Vector3(5.5f, 1, 0);
			myPoint2.size = 10;
			myPoint2.linearAttenuation = 1.0f;
         myPoint2.quadraticAttenuation = 0.25f;
			Renderer.renderables.Add(myPoint2);

         //add a callback in case the window size changes
			myViewport.notifier += new Viewport.ViewportNotifier(handleViewportChanged);
		}
	}

	class Program
	{
		static void Main(string[] args)
		{
			using (TestRenderer example = new TestRenderer())
			{
				example.Title = "Test Renderer";
				example.Run(/*60.0*/);
			}
		}
	}
}
