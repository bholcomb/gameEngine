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

using Valve.VR;

namespace testRenderer
{
   public class HMD
   {
      public Vector3 position = Vector3.Zero;
      public Quaternion orientation = Quaternion.Identity;

      public RenderTarget[] myRenderTargets = new RenderTarget[2];
      public Camera[] myCameras = new Camera[2];
      Matrix4[] myEyeTransform = new Matrix4[2];

      static CVRSystem vrSystem;

      TrackedDevicePose_t[] renderPoseArray = new TrackedDevicePose_t[OpenVR.k_unMaxTrackedDeviceCount];
      TrackedDevicePose_t[] gamePoseArray = new TrackedDevicePose_t[OpenVR.k_unMaxTrackedDeviceCount];

      public HMD()
      {
         initEyes();
      }

      public static bool init()
      {
         EVRInitError error = EVRInitError.None;
         vrSystem = OpenVR.Init(ref error, EVRApplicationType.VRApplication_Scene);

         if(error != EVRInitError.None)
         {
            Warn.print("Error initializing OpenVR: {0}", error);
            return false;
         }

         if(OpenVR.Compositor == null)
         {
            Warn.print("Failed to initialize OpenVR compositor");
            return false;
         }

         //setup a seated environment
         OpenVR.Compositor.SetTrackingSpace(ETrackingUniverseOrigin.TrackingUniverseSeated);

         Info.print("OpenVR Manufacturer: {0}", getTrackedDeviceString(ETrackedDeviceProperty.Prop_ManufacturerName_String));
         Info.print("OpenVR Model Number: {0}", getTrackedDeviceString(ETrackedDeviceProperty.Prop_ModelNumber_String));
         Info.print("OpenVR Tracking System: {0}", getTrackedDeviceString(ETrackedDeviceProperty.Prop_TrackingSystemName_String));
         Info.print("OpenVR Driver Version: {0}", getTrackedDeviceString(ETrackedDeviceProperty.Prop_DriverVersion_String));

         return true;
      }

      static string getTrackedDeviceString(ETrackedDeviceProperty prop)
      {
         var error = ETrackedPropertyError.TrackedProp_Success;
         uint bufferLength = vrSystem.GetStringTrackedDeviceProperty(OpenVR.k_unTrackedDeviceIndex_Hmd, prop, null, 0, ref error);
         if(bufferLength > 1)
         {
            var stringBuilder = new System.Text.StringBuilder((int)bufferLength);
            vrSystem.GetStringTrackedDeviceProperty(OpenVR.k_unTrackedDeviceIndex_Hmd, prop, stringBuilder, bufferLength, ref error);

            return stringBuilder.ToString();
         }

         if(error != ETrackedPropertyError.TrackedProp_Success)
         {
            return error.ToString();
         }

         return "unkown";
      }

      public static bool shutdown()
      {
         OpenVR.Shutdown();
         return true;
      }

      public void resetPose()
      {
         vrSystem.ResetSeatedZeroPose();
      }

      public static Matrix4 convertToMatrix4(HmdMatrix34_t m)
      {
         Matrix4 mat = new  Matrix4(
            m.m0, m.m4, m.m8, 0.0f,
            m.m1, m.m5, m.m9, 0.0f,
            m.m2, m.m6, m.m10, 0.0f,
            m.m3, m.m7, m.m11, 1.0f
            );

         return mat;
      }

      void initEyes()
      {
         List<RenderTargetDescriptor> rtdesc = new List<RenderTargetDescriptor>();
         rtdesc.Add(new RenderTargetDescriptor() { attach = FramebufferAttachment.ColorAttachment0, format = SizedInternalFormat.Rgba8 }); //creates a texture internally
         rtdesc.Add(new RenderTargetDescriptor() { attach = FramebufferAttachment.DepthAttachment, bpp = 32 });

         uint w = 0;
         uint h = 0;
         vrSystem.GetRecommendedRenderTargetSize(ref w, ref h);

         for (int i =0; i < 2; i++)
         {
            myRenderTargets[i] = new RenderTarget((int)w, (int)h, rtdesc);
            myCameras[i] = new Camera(new Viewport(0, 0, (int)w, (int)h));
            
            float leftHalfTan = 0.0f;
            float rightHalfTan = 0.0f;
            float topHalfTan = 0.0f;
            float bottomHalfTan = 0.0f;
            //NOTE: top and bottom are still backwards
            vrSystem.GetProjectionRaw((EVREye)i, ref leftHalfTan, ref rightHalfTan, ref bottomHalfTan, ref topHalfTan);

            //convert to frustum edges to create projection matrix
            float zNear = 0.01f;
            float zFar = 1000.0f;
            float left = leftHalfTan * zNear;
            float right = rightHalfTan * zNear;
            float bottom = bottomHalfTan * zNear;
            float top = topHalfTan * zNear;

            myCameras[i].setProjection(Matrix4.CreatePerspectiveOffCenter(left, right, bottom, top, 0.01f, 1000.0f));

            //openVR uses a right-back-up system, just like our convention, so no conversion necessary
            myEyeTransform[i] = convertToMatrix4(vrSystem.GetEyeToHeadTransform((EVREye)i));
         }
      }

      public bool update()
      {
         var error = EVRCompositorError.None;
         error = OpenVR.Compositor.WaitGetPoses(renderPoseArray, gamePoseArray);

         if (error != EVRCompositorError.None)
         {
            Warn.print("Error in WaitGetPoses: {0}", error);
            return false;
         }

         //get head position
         TrackedDevicePose_t pose = renderPoseArray[OpenVR.k_unTrackedDeviceIndex_Hmd];
         Matrix4 headPose = Matrix4.Identity;
         if (pose.bDeviceIsConnected == true && pose.bPoseIsValid == true)
         {
             headPose = convertToMatrix4(pose.mDeviceToAbsoluteTracking);
         }
         else
         {
            Warn.print("Error getting pose information for the HMD");
            return false;
         }

         //update camera matrix for each eye
         for (int i = 0; i < 2; i++)
         {
            //TODO:  this doesn't look right, but seems to work.  Work through the math.
            Matrix4 view = Matrix4.CreateTranslation(-position) * Matrix4.CreateFromQuaternion(orientation) * myEyeTransform[i].Inverted() * headPose.Inverted();
            myCameras[i].setView(view);
         }

         return true;
      }

      public void present()
      {
         var error = EVRCompositorError.None;
         VRTextureBounds_t bounds = new VRTextureBounds_t();
         bounds.uMin = 0.0f;
         bounds.uMax = 1.0f;
         bounds.vMin = 0.0f;
         bounds.vMax = 1.0f;

         for (int i =0; i< 2; i++)
         {
            Texture_t tex = new Texture_t();
            tex.handle = (IntPtr)(myRenderTargets[i].buffers[FramebufferAttachment.ColorAttachment0].id());
            tex.eColorSpace = EColorSpace.Gamma;
            tex.eType = ETextureType.OpenGL;

            error = OpenVR.Compositor.Submit((EVREye)i, ref tex, ref bounds, EVRSubmitFlags.Submit_Default);

            if(error != EVRCompositorError.None)
            {
               Warn.print("Error submtting texture to OpenVR: {0}", error);
            }
         }
      }
   }

   public class HmdView : Graphics.View
   {
      HMD myHmd;
      Graphics.View myLeftEyeView;
      Graphics.View myRightEyeView;

      public HmdView(string name, HMD hmd) : base(name, null, null)
      {
         myHmd = hmd;
         myLeftEyeView = new Graphics.View("Left Eye", myHmd.myCameras[0], myHmd.myCameras[0].viewport());
         myRightEyeView = new Graphics.View("Right Eye", myHmd.myCameras[1], myHmd.myCameras[1].viewport());
         addSibling(myLeftEyeView);
         addSibling(myRightEyeView);
      }

      public override void addPass(Pass p)
      {
         Pass lp = new Pass(p);
         if (p.renderTarget == null)
            lp.renderTarget = myHmd.myRenderTargets[0];
         myLeftEyeView.addPass(lp);

         Pass rp = new Pass(p);
         if(p.renderTarget == null)
            rp.renderTarget = myHmd.myRenderTargets[1];
         myRightEyeView.addPass(rp);
      }

      public override void removePass(String passName)
      {
         myLeftEyeView.removePass(passName);
         myRightEyeView.removePass(passName);
      }
   }

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

      HMD myHmd;

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

         if(e.Key == Key.F12)
         {
            myHmd.resetPose();
         }
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			string version = GL.GetString(StringName.Version);
			int major = System.Convert.ToInt32(version[0].ToString());
			int minor = System.Convert.ToInt32(version[2].ToString());
         Console.WriteLine("Found OpenGL Version: {0}.{1}", major, minor);
			if (major < 4 && minor < 4)
			{
				MessageBox.Show("You need at least OpenGL 4.4 to run this example. Aborting.", "Ooops", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            Environment.Exit(0);
			}

         bool runtimeFound = OpenVR.IsRuntimeInstalled();
         bool hmdPresent = OpenVR.IsHmdPresent();

         if (runtimeFound == false)
         {
            MessageBox.Show("OpenVR runtime not found", "Oooops", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            Environment.Exit(0);
         }

         if(hmdPresent == false)
         {
            MessageBox.Show("OpenVR HMD not found", "Oooops", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            Environment.Exit(0);
         }

         if(HMD.init() == false)
         {
            MessageBox.Show("Failed to initialize HMD", "Oooops", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            Environment.Exit(0);
         }

			GL.ClearColor(0.2f, 0.2f, 0.2f, 1.0f);
			GL.ClearDepth(1.0f);

         myHmd = new HMD();


			initRenderTarget();
			initRenderer();

			myCamera.position = new Vector3(0, 2, 10);
         myHmd.position = myCamera.position;
         myHmd.orientation = myCamera.myOrientation;

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

         //move the HMD with the main camera
         myHmd.position = myCamera.position;
         myHmd.orientation = myCamera.myOrientation;

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
			if(ImGui.beginWindow("Render Stats", ref myShowRenderStats, Window.Flags.Borders))
			{
            ImGui.setWindowPosition(new Vector2(20, 50), SetCondition.FirstUseEver);
            ImGui.setWindowSize(new Vector2(500, 650), SetCondition.FirstUseEver);
            ImGui.label("FPS: {0:0.00}", avgFps);
            ImGui.label("Camera position: {0}", myCamera.position);
            ImGui.label("Camera view vector: {0}", myCamera.viewVector);
            ImGui.separator();
            ImGui.label("Frame Time: {0:0.00}ms", (Renderer.stats.cullTime + Renderer.stats.prepareTime + Renderer.stats.generateTime + Renderer.stats.executeTime) * 1000.0);
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
               for (int j = 0; j < Renderer.stats.viewStats[i].passStats.Count; j++)
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

         //update the HMD
         myHmd.update();

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
         myCamera.bind();
         GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
         GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

			RenderCommand cmd = new BlitFrameBufferCommand(myHmd.myRenderTargets[0]);
			cmd.execute();

         myHmd.present();

			SwapBuffers();
		}

		public void initRenderer()
		{
			myUiEventHandler = new GuiEventHandler(this);

         //add the "environment" pass to draw the skybox
         //setup the rendering scene
         Graphics.View v = new HmdView("HMD View", myHmd);

         Pass p = new Pass("environment", "skybox");
         p.filter = new TypeFilter(new List<String>() { "skybox" });
         p.clearTarget = true; //false is default setting
         v.addPass(p);

         p = new Pass("terrain", "forward-lighting");
         p.filter = new TypeFilter(new List<String>() { "terrain" });
         v.addPass(p);

         p = new Pass("model", "forward-lighting");
         p.filter = new TypeFilter(new List<String>() { "light", "staticModel", "skinnedModel" });
         v.addPass(p);

         //add the view
         Renderer.views[v.name] = v;

         //set the present function
         Renderer.present = present;

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
            {
               mdesc = new ObjModelDescriptor("../data/models/vegetation/birch/birch_01_a.obj");
            }
            else
            {
               mdesc = new ObjModelDescriptor("../data/models/props/rocks_3_by_nobiax-d6s8l2b/rocks_03-blend.obj");
            }

				smr.model = Renderer.resourceManager.getResource(mdesc) as StaticModel;
				Renderer.renderables.Add(smr);
				smr.setPosition(new Vector3((rand.Next() % size) - halfSize, 0, (rand.Next() % size) - halfSize));
				//smr.model.myMeshes[0].material.myFeatures = Material.Feature.DiffuseMap; //turn off lighting
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

			myViewport.notifier += new Viewport.ViewportNotifier(handleViewportChanged);
		}
	}

	class Program
	{
		static void Main(string[] args)
		{
			using (TestRenderer example = new TestRenderer())
			{
				example.Title = "Test VR Renderer";
				example.Run(/*60.0*/);
			}

		}
	}
}
