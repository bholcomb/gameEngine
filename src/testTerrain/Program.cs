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
using Editor;
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

		bool myShowRenderStats = true;

		World myWorld;
		TerrainRenderManager myTerrainRenderManager;
		Editor.Editor myTerrainEditor;

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
				myTerrainEditor.toggleEnabled();
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

			myTerrainEditor = new Editor.Editor(myWorld, myCamera);
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

			mySkinnedModel.update((float)TimeSource.timeThisFrame());

			avgFps = (0.99f * avgFps) + (0.01f * (float)TimeSource.fps());

			ImGui.beginFrame();
			if (ImGui.beginWindow("Render Stats", ref myShowRenderStats, Window.Flags.ShowBorders))
			{
				ImGui.setWindowPosition(new Vector2(20, 100), SetCondition.FirstUseEver);
				ImGui.setWindowSize(new Vector2(300, 600), SetCondition.FirstUseEver);
				ImGui.label("FPS: {0:0.00}", avgFps);
				ImGui.label("Cull Time: {0:0.00}ms", Renderer.stats.cullTime * 1000.0);
				ImGui.label("Extract Time: {0:0.00}ms", Renderer.stats.extractTime * 1000.0);
				ImGui.label("Prepare Time: {0:0.00}ms", Renderer.stats.prepareTime * 1000.0);
				ImGui.label("Submit Time: {0:0.00}ms", Renderer.stats.submitTime * 1000.0);
				ImGui.label("Execute Time: {0:0.00}ms", Renderer.stats.executeTime * 1000.0);
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
					ImGui.label("{0} {1}", i, Renderer.stats.viewStats[i].viewName);
					ImGui.label("   Queue count {0}", Renderer.stats.viewStats[i].queueCount);
					ImGui.label("   Render Calls {0}", Renderer.stats.viewStats[i].renderCalls);
				}

				ImGui.endWindow();
			}

			myTerrainEditor.onGui();


			//ImGui.debug();
			ImGui.endFrame();


			//get any new chunks based on the camera position
			//myWorld.setInterest(myCamera.position);
			//myWorld.tick(e.Time);
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
																																																							 //rtdesc.Add(new RenderTargetDescriptor() { attach = FramebufferAttachment.DepthAttachment, bpp = 32 });

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
			SceneGraph sg = new SceneGraph();

			//add the "environment" pass to draw the skybox
			RenderStage rs = new RenderStage("environment", "skybox");
			rs.isActive = true;
			sg.renderStages.Add(rs);
			Graphics.View view = new Graphics.View("skyboxView", myCamera, myViewport, myRenderTarget);
			view.filter = new TypeFilter(new List<String>() { "skybox" });
			view.clearTarget = false;
			rs.registerView(view);

			//add the "terrain" pass to draw the terrain
			rs = new RenderStage("terrain", "forward-lighting");
			rs.isActive = true;
			sg.renderStages.Add(rs);
			view = new Graphics.View("terrain view", myCamera, myViewport, myRenderTarget, false);
			view.filter = new TypeFilter(new List<String>() { "terrain" });
			rs.registerView(view);

			//add the "model" pass to draw models
			rs = new RenderStage("model", "forward-lighting");
			rs.isActive = true;
			sg.renderStages.Add(rs);
			view = new Graphics.View("model view", myCamera, myViewport, myRenderTarget, false);
			view.filter = new TypeFilter(new List<String>() { "light", "staticModel", "skinnedModel" });
			rs.registerView(view);

			//add the "debug" pass to draw the debug visualizers
			rs = new RenderStage("debug", "debug");
			rs.isActive = true;
			sg.renderStages.Add(rs);
			view = new Graphics.DebugView(myCamera, myViewport, myRenderTarget);
			rs.registerView(view);

			//add the "UI" pass to draw the UI
			rs = new RenderStage("UI", "UI");
			rs.isActive = true;
			sg.renderStages.Add(rs);
			view = new UI.GuiView(myCamera, myViewport, myRenderTarget);
			rs.registerView(view);

			Renderer.present = present;

			//add the scene to the renderer
			Renderer.scenes["main"] = sg;

			//create the default font
			//myFont = FontManager.findFont("DEFAULT");
			//(myFont as SDFont).myEffects = SDFont.Effects.Outline;


			//create the skybox renderable
			SkyboxRenderable skyboxRenderable = new SkyboxRenderable();
			SkyBoxDescriptor sbmd = new SkyBoxDescriptor("../data/skyboxes/interstellar/interstellar.json");
			skyboxRenderable.model = Renderer.resourceManager.getResource(sbmd) as SkyBox;
			Renderer.renderables.Add(skyboxRenderable);

			//create a tree instance
			Random rand = new Random(230877);
			for (int i = 0; i < 5000; i++)
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
				Renderer.renderables.Add(smr);
				smr.setPosition(new Vector3((rand.Next() % size) - halfSize, 0, (rand.Next() % size) - halfSize));
				//smr.model.myMeshes[0].material.myFeatures = Material.Feature.DiffuseMap; //turn off lighting
			}

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
			LightRenderable lr = new LightRenderable();
			lr.myLightType = LightRenderable.Type.DIRECTIONAL;
			lr.color = Color4.White;
			lr.direction = new Vector4(-1, 1, 0, 0);
			Renderer.renderables.Add(lr);

			//add a point light
			LightRenderable lrp1 = new LightRenderable();
			lrp1.myLightType = LightRenderable.Type.POINT;
			lrp1.color = Color4.Red;
			lrp1.position = new Vector3(2.5f, 1, 0);
			lrp1.size = 10;
			lrp1.linearAttenuation = 1.0f;
			lrp1.quadraticAttenuation = 0.5f;
			Renderer.renderables.Add(lrp1);

			//add another point light
			LightRenderable lrp2 = new LightRenderable();
			lrp2.myLightType = LightRenderable.Type.POINT;
			lrp2.color = Color4.Blue;
			lrp2.position = new Vector3(5.5f, 1, 0);
			lrp2.size = 10;
			lrp2.linearAttenuation = 1.0f;
			lrp2.quadraticAttenuation = 0.25f;
			Renderer.renderables.Add(lrp2);

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
