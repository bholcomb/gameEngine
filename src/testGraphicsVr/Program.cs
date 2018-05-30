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
using VR;
//using Midi;

namespace testRenderer
{
   public class TestRenderer : GameWindow
	{
		public static int theWidth = 1920;
		public static int theHeight = 1280;

		Initializer myInitializer;
		Viewport myViewport;
		Camera myCamera;
		GameWindowCameraEventHandler myCameraEventHandler;
		UI.GuiEventHandler myUiEventHandler;
		RenderTarget myRenderTarget;
      RenderTarget myUiRenderTarget;
		SkinnedModelRenderable mySkinnedModel;
      LightRenderable mySun;
      LightRenderable myPoint1;
      LightRenderable myPoint2;

      Overlay myUiOverlay;

      StaticModel mask;


		bool myRenderStatsWindowClosed = true;

		World myWorld;
		TerrainRenderManager myTerrainRenderManager;

      HMD myHmd;
      TrackedCamera myHmdCamera;

		public TestRenderer()
			: base(theWidth, theHeight, new GraphicsMode(32, 24, 0, 0), "Haven Test", GameWindowFlags.Default, DisplayDevice.Default, 4, 4,
#if DEBUG
         GraphicsContextFlags.Debug)
#else
			GraphicsContextFlags.Default)
#endif
		{
			myInitializer = new Initializer(new String[] { "testRenderer.lua" });

			Keyboard.KeyUp += new EventHandler<KeyboardKeyEventArgs>(handleKeyboardUp);

			this.VSync = VSyncMode.Off;
         this.Location = new Point(0, 0);
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
				myRenderStatsWindowClosed = !myRenderStatsWindowClosed;
			}

			if (e.Key == Key.F5)
			{
				Renderer.shaderManager.reloadShaders();
            reloadStuff();
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

         Renderer.init();
         myViewport = new Viewport(this);
         myCamera = new Camera(myViewport, 60.0f, 0.1f, 1000.0f);

         myCameraEventHandler = new GameWindowCameraEventHandler(myCamera, this);

         string version = GL.GetString(StringName.Version);
			int major = System.Convert.ToInt32(version[0].ToString());
			int minor = System.Convert.ToInt32(version[2].ToString());
         Console.WriteLine("Found OpenGL Version: {0}.{1}", major, minor);
			if (major < 4 && minor < 4)
			{
				MessageBox.Show("You need at least OpenGL 4.4 to run this example. Aborting.", "Ooops", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            Environment.Exit(0);
			}

         bool runtimeFound = VR.VR.vrAvailable();
         bool hmdPresent = VR.VR.hmdAttached();

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

         if(VR.VR.init() == false)
         {
            MessageBox.Show("Failed to initialize HMD", "Oooops", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            Environment.Exit(0);
         }

			GL.ClearColor(0.2f, 0.2f, 0.2f, 1.0f);
			GL.ClearDepth(1.0f);

         myHmd = new HMD();

         if(TrackedCamera.hasTrackedCamera() == true)
         {
            myHmdCamera = new TrackedCamera();
            myHmdCamera.startStream();
         }

         myUiOverlay = new Overlay("UI", "UI", null); //texture set later
         
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

//          inputDevice = InputDevice.InstalledDevices[0];
//          inputDevice.Open();
//          inputDevice.ControlChange += controlChanged;
//          inputDevice.StartReceiving(null);  // Note events will be received in another thread
      }

//       private void controlChanged(ControlChangeMessage msg)
//       {
//          float min = 0.0f;
//          float max = 2.0f;
//          float range = max - min;
// 
//          if(msg.Control == (Midi.Control)1)
//          {
//             a = ((float)msg.Value / 128.0f) * range + min;
//          }
// 
//          if (msg.Control == (Midi.Control)2)
//          {
//             b = ((float)msg.Value / 128.0f) * range + min;
//          }
// 
//          Info.print("{0}, {1}", a, b);
//       }
// 
//       InputDevice inputDevice;
      float a = 1.0f;
      float b = 1.0f;

      protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
		{
			base.OnClosing(e);
         if (myHmdCamera != null)
         {
            myHmdCamera.stopStream();
         }
         myUiOverlay.release();
         VR.VR.shutdown();
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
			//mySkinnedModel.update((float)TimeSource.timeThisFrame());

         //update the sun and point lights
         double now = TimeSource.currentTime();
         mySun.position = new Vector3((float)Math.Sin(now) * 5.0f, 5.0f, (float)Math.Cos(now) * 5.0f);
//          myPoint1.position = new Vector3(2.5f, 1, 0) + new Vector3(0.0f, (float)Math.Sin(now), (float)Math.Cos(now));
//          myPoint2.position = new Vector3(5.5f, 1, 0) + new Vector3(0.0f, (float)Math.Cos(now), (float)Math.Sin(now));

         //high frequency filter on avg fps
         avgFps = (0.99f * avgFps) + (0.01f * (float)TimeSource.fps());

         myUiOverlay.visible = !myRenderStatsWindowClosed;

         ImGui.beginFrame();
			if(ImGui.beginWindow("Render Stats", ref myRenderStatsWindowClosed, Window.Flags.DefaultWindow))
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

         if(myHmdCamera != null)
         {
            myHmdCamera.tick();
         }

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


         List<RenderTargetDescriptor> uidesc = new List<RenderTargetDescriptor>();
         uidesc.Add(new RenderTargetDescriptor() { attach = FramebufferAttachment.ColorAttachment0, format = SizedInternalFormat.Rgba8 }); //creates a texture internally
         uidesc.Add(new RenderTargetDescriptor() { attach = FramebufferAttachment.DepthAttachment, tex = new Texture(x, y, PixelInternalFormat.DepthComponent32f) }); //uses an existing texture			

         if (myRenderTarget == null)
			{
				myRenderTarget = new RenderTarget(x, y, rtdesc);
            myUiRenderTarget = new RenderTarget(x, y, uidesc);
			}
			else
			{
				myRenderTarget.update(x, y, rtdesc);
            myUiRenderTarget.update(x, y, uidesc);
			}

         myUiOverlay.texture = myUiRenderTarget.buffers[FramebufferAttachment.ColorAttachment0];
      }

      void handleViewportChanged(int x, int y, int w, int h)
		{
			initRenderTarget();
		}

		void present()
		{
         myUiOverlay.submit();

         myCamera.bind();
         GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
         GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

			RenderCommand cmd = new BlitFrameBufferCommand(myHmd.myRenderTargets[0]);
         cmd.execute();

         myHmd.present();

			SwapBuffers();
		}


      StatelessRenderCommand drawCmd;
      #region Projected Shader
      string projVs = @"
#version 430

layout(location = 0) in vec3 position;

layout(std140, binding = 0) uniform camera {
   mat4 view; //aligned 4N
   mat4 projection; //aligned 4N
   mat4 viewProj; //aligned 4N
   mat4 ortho; //aligned 4N
   vec4 viewVector; //aligned 4N
   vec4 eyeLocation; //aligned 4N
   float left, right, top, bottom; //aligned 4N
   float zNear, zFar, aspect, fov; //aligned 4N
   int frame;
   float dt;
};

layout(location = 1) uniform mat4 camProj;
layout(location = 2) uniform mat4 camView;

smooth out vec4 texCoord;

mat4 correction = mat4(
      vec4(0.5, 0, 0, 0),
      vec4(0, 0.5, 0, 0),
      vec4(0, 0, 0.5, 0),
      vec4(0.5, 0.5, 0.5, 1)
   );

void main()
{
   mat4 viewNoTrans = view;
   viewNoTrans[3] = vec4(0,0,0,1);

   texCoord = correction * camProj * camView * vec4(position, 1);
   gl_Position = projection * viewNoTrans * vec4(position,1);
}
";
      string projPs = @"
#version 430

layout(location = 20) uniform sampler2D tex;
layout(location = 21) uniform vec3 maskColor;

layout(location = 22) uniform float acceptance;
layout(location = 23) uniform float cutoff;
layout(location = 24) uniform float gain;

layout(std140, binding = 0) uniform camera{
	mat4 view; //aligned 4N
	mat4 projection; //aligned 4N
	mat4 viewProj; //aligned 4N
	mat4 ortho; //aligned 4N
	vec4 viewVector; //aligned 4N
	vec4 eyeLocation; //aligned 4N
	float left, right, top, bottom; //aligned 4N
	float zNear, zFar, aspect, fov; //aligned 4N
	int frame;
	float dt;
};

smooth in vec4 texCoord;

out vec4 FragColor;

vec3 rgb2hsv(vec3 c)
{
    vec4 K = vec4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
    vec4 p = mix(vec4(c.bg, K.wz), vec4(c.gb, K.xy), step(c.b, c.g));
    vec4 q = mix(vec4(p.xyw, c.r), vec4(c.r, p.yzx), step(p.x, c.r));

    float d = q.x - min(q.w, q.y);
    float e = 1.0e-10;
    return vec3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
}

vec3 rgb2yuv(vec3 c)
{
   vec3 yuv;
   yuv.x = c.r * 0.299 + c.g * 0.587 + c.b * 0.114;
	yuv.y = c.r * -0.169 + c.g * -0.331 + c.b * 0.5 + 0.5;
	yuv.z = c.r * 0.5 + c.g * -0.419 + c.b * -0.081 + 0.5;

   return yuv;
}

vec3 yuv2rgb(vec3 c)
{
   vec3 rgb;
   vec3 yuv = vec3(c.x, c.y - 0.5, c.z - 0.5);

   rgb.r = yuv.x * 1.0 + yuv.y * 0.0    + yuv.z * 1.4;
	rgb.g = yuv.x * 1.0 + yuv.y * -0.343 + yuv.z * -0.711;
	rgb.b = yuv.x * 1.0 + yuv.y * 1.765  + yuv.z * 0.0;

   return rgb;
}

float[9] kerGausBlur = float[]  (1., 2., 1.,
                                 2., 4., 2.,
                                 1., 2., 1.);

vec3[9] getData(vec2 t)
{
   ivec2 tSize = textureSize(tex,0);  
   vec2 d = vec2(1 / float(tSize.x), 1 / float(tSize.y));
   vec3[9] mat;
   int k = -1;
   for (int i=-1; i<2; i++) 
   {   
      for(int j=-1; j<2; j++) 
      {    
         k++;    
         mat[k] = texture(tex, t + vec2( float(j)*d.x, float(i)*d.y)).rgb;
      }
   }

   return mat;
}

float convolve(float[9] kernel, float[9] matrix, float denom, float offset) 
{
   float res = 0.0;
   for (int i=0; i<9; i++) 
   {
      res += kernel[i]*matrix[i];
   }
   
   return clamp(res/denom + offset, 0.0, 1.0);
}

void main()
{
   vec2 t = texCoord.xy / texCoord.w;
   if(t.x < 0 || t.x > 1) discard;
   if(t.y < 0 || t.y > 1) discard;

	vec4 texColor = textureProj(tex, texCoord);

   if(texColor.a <= 0.05) discard;

//hsv check
/*
   vec3 hsv = rgb2hsv(texColor.xyz);
   vec3 maskHsv = rgb2hsv(maskColor.xyz);
   if(abs(hsv.x - maskHsv.x) < acceptance)
      discard;

   if(abs(hsv.x - maskHsv.x) < (acceptance * 2))
   {
      texColor.a = (abs(hsv.x - maskHsv.x) - acceptance) / maskThreshold;
   }
*/

//vector parallel check
 /*
   //0.075 works ok
   float backgroundiness = dot( normalize(maskColor), normalize(texColor.rgb) );
   backgroundiness = 1 - backgroundiness;
   if(backgroundiness < acceptance)
      discard;

   if(backgroundiness < (acceptance * 2))
   {
      float alpha = clamp( (backgroundiness - acceptance) / acceptance, 0.0, 1.0 );
      texColor.a = alpha;
   }
*/

//rgb distance test
/*
   vec4 delta = texColor - vec4(maskColor, 1);
   float distance2 = dot( delta, delta );
   float minDist = acceptance;
   float maxDist = acceptance * 2;

   float alpha = clamp( (distance2 - minDist) / (maxDist - minDist), 0.0, 1.0 );
   texColor.a = alpha;
*/

//historic greenscreen algorithm with chroma blur
   //blur the chroma channels first
   vec3[9] pixels = getData(t.xy);
   float[9] cr;
   float[9] cb;
   //convert to yuv
   for(int i=0; i<9; i++)
   {
      pixels[i] = rgb2yuv(pixels[i]);
      cr[i] = pixels[i].g;
      cb[i] = pixels[i].b;
   }

   float crr = convolve(kerGausBlur, cr, 16, 0);
   float cbr = convolve(kerGausBlur, cb, 16, 0);
   
   vec3 res = yuv2rgb(vec3(pixels[4].r, crr, cbr));   

   float A = acceptance;
   float B = cutoff;

   float alpha = A * (res.r + res.b) - B * res.g;   
   texColor.rgb = vec3(texColor.r, min(texColor.g, texColor.b), texColor.b); //despill
   texColor.a = alpha;

//historic greenscreen algorithm
/*
   float A = acceptance;
   float B = cutoff;

   float alpha = A * (texColor.r + texColor.b) - B * texColor.g;   
   texColor.rgb = vec3(texColor.r, min(texColor.g, texColor.b), texColor.b); //despill
   texColor.a = alpha;
*/

//chroma distance
/*
   float A = acceptance;
   float B = cutoff;

   vec3 p = rgb2yuv(texColor.rgb);
   vec3 k = rgb2yuv(maskColor.rgb);
   vec3 dist = k - p;

   float d = sqrt((dist.g * dist.g) + (dist.b * dist.b));

   float alpha = 0.0;
   if(d < A)
      alpha = 0.0;
   else if(d < B)
      alpha = (d - A) / (B-A);
   else
      alpha = 1.0;
   
   //set alpha
   texColor.a = alpha;
   
   //spill reduction 
   texColor.rgb = vec3(texColor.r, min(texColor.g, texColor.b), texColor.b);
*/

//chroma test from video demistified
/*
   vec3 p = rgb_2_yuv(texColor.rgb, itu_601);
   vec3 k = rgb_2_yuv(maskColor.rgb, itu_601);

   float theta = atan(k.z, k.y);

   float x_angle = p.y * cos(theta) + p.z * sin(theta);
   float z_angle = p.z * cos(theta) + p.y * sin(theta);

//   float kfg = x_angle - (abs(z_angle) / tan(acceptance/2.0f));
//    
//    float alpha = 1;
//    if(kfg > 0.0f) 
//    {
//       alpha = (1.0 - kfg ) * gain;
//       float beta = atan(z_angle, x_angle);
// 
//       if(abs(beta) < (cutoff / 2.0))
//       {
//          alpha = 0;
//       }
//    }

   float a = clamp(pow(z_angle, 4), 0, 1);

   if(a > cutoff) a = 1;
   texColor.a = a;
*/
   FragColor = texColor;
}";
      #endregion
     
      void reloadStuff()
      {
         StaticModelRenderable testRenderable = new StaticModelRenderable();
         ObjModelDescriptor maskDesc;
         maskDesc = new ObjModelDescriptor("../data/models/props/cockpitMask/mask.obj");
         mask = Renderer.resourceManager.getResource(maskDesc) as StaticModel;

         List<ShaderDescriptor> sdesc = new List<ShaderDescriptor>();
         sdesc.Add(new ShaderDescriptor(ShaderType.VertexShader, projVs, ShaderDescriptor.Source.String));
         sdesc.Add(new ShaderDescriptor(ShaderType.FragmentShader, projPs, ShaderDescriptor.Source.String));
         ShaderProgramDescriptor desc = new ShaderProgramDescriptor(sdesc);
         myProjShader = Renderer.resourceManager.getResource(desc) as ShaderProgram;
      }

      ShaderProgram myProjShader;

      void drawHmdCamera(Graphics.View view)
      {
         if (myHmdCamera != null)
         {
            if (drawCmd == null)
            {
               Vector3[] verts = new Vector3[4];
               verts[0] = new Vector3(-5f, -5f, -2.0f);
               verts[1] = new Vector3(5f, -5f, -2.0f);
               verts[2] = new Vector3(5f, 1f, -2.0f);
               verts[3] = new Vector3(-5f, 1f, -2.0f);
               //drawCmd = new RenderTexturedQuadCommand(verts, myHmdCamera.texture);
               //drawCmd = new RenderTextureCubeCommand(new Vector3(-1, -1, -1), new Vector3(1,1,1), myHmdCamera.texture);               
               //drawCmd = new RenderSphereCommand(Vector3.Zero, 1, Color4.White);
               drawCmd = new StatelessDrawElementsCommand(mask.myMeshes[0]);
               drawCmd.renderState.setVertexBuffer(mask.myVbo.id, 0, 0, V3N3T2.stride);
               drawCmd.renderState.setIndexBuffer(mask.myIbo.id);

               drawCmd.renderState.setUniform(new UniformData(0, Uniform.UniformType.Int, 0));
               drawCmd.renderState.setUniform(new UniformData(21, Uniform.UniformType.Vec3, new Vector3(0.15f, 0.70f, 0.30f)));
               drawCmd.renderState.setUniform(new UniformData(22, Uniform.UniformType.Float, 0.7f));
               drawCmd.renderState.setTexture(myHmdCamera.texture.id(), 0, TextureTarget.Texture2D);
               List<ShaderDescriptor> sdesc = new List<ShaderDescriptor>();
               sdesc.Add(new ShaderDescriptor(ShaderType.VertexShader, projVs, ShaderDescriptor.Source.String));
               sdesc.Add(new ShaderDescriptor(ShaderType.FragmentShader, projPs, ShaderDescriptor.Source.String));
               ShaderProgramDescriptor desc = new ShaderProgramDescriptor(sdesc);
               myProjShader = Renderer.resourceManager.getResource(desc) as ShaderProgram;

               drawCmd.pipelineState.shaderState.shaderProgram = myProjShader;
               drawCmd.pipelineState.depthTest.enabled = false;
               drawCmd.pipelineState.culling.enabled = false;
               drawCmd.pipelineState.blending.enabled = true;
               VertexArrayObject vao = new VertexArrayObject();
               vao.bindVertexFormat<V3T2B4>(myProjShader);
               drawCmd.pipelineState.vaoState.vao = vao;
            }

            drawCmd.pipelineState.shaderState.shaderProgram = myProjShader;
            drawCmd.renderState.myVertexBuffers[0] = new RenderState.VertexInfo() { id = mask.myVbo.id, location =0, offset =0, stride =V3N3T2.stride };
            drawCmd.renderState.setIndexBuffer(mask.myIbo.id);

            drawCmd.renderState.resetUniforms();
            drawCmd.renderState.setUniform(new UniformData(1, Uniform.UniformType.Mat4, myHmdCamera.projection));
            drawCmd.renderState.setUniform(new UniformData(2, Uniform.UniformType.Mat4, myHmdCamera.pose.Inverted()));
            
            drawCmd.renderState.setUniform(new UniformData(20, Uniform.UniformType.Int, 0));
            drawCmd.renderState.setUniform(new UniformData(21, Uniform.UniformType.Vec3, new Vector3(0.15f, 0.70f, 0.30f)));
            drawCmd.renderState.setUniform(new UniformData(22, Uniform.UniformType.Float, a)); //acceptance 30 degrees = 0.523599 rad
            drawCmd.renderState.setUniform(new UniformData(23, Uniform.UniformType.Float, b)); //cutoff 10 degrees = 0.174533 rad
            drawCmd.renderState.setUniform(new UniformData(24, Uniform.UniformType.Float, 0.5f)); //gain


            view.postCommands.Add(drawCmd);

            //view.postCommands.Add(new RenderFrustumCommand(pLookAt * myHmdCamera.projection, Color4.Green));
         }
      }

		public void initRenderer()
		{
			myUiEventHandler = new GuiEventHandler(this);

         //add the "environment" pass to draw the skybox
         //setup the rendering scene
         HmdView v = new HmdView("HMD View", myHmd);

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

         //setup UI 
         Graphics.View uiView = new Graphics.View("ui", myCamera, myViewport);
         uiView.processRenderables = false; //GUI pass will generate all the renderables we need
         GuiPass uiPass = new GuiPass(myUiRenderTarget);
         uiPass.clearTarget = true;
         uiPass.clearColor = new Color4(0.0f, 0.0f, 0.0f, 0.0f);
         uiView.addPass(uiPass);
         v.addSibling(uiView);

         //add the view
         Renderer.views[v.name] = v;

         v.leftEyeView.PostGenerateCommands += drawHmdCamera;
         v.rightEyeView.PostGenerateCommands += drawHmdCamera;

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
				BobStaticModelDescriptor mdesc;
//             if (i % 2 == 0)
//             {
//                mdesc = new BobStaticModelDescriptor("../data/models/vegetation/birch/birch.bob");
//             }
//             else
//             {
               mdesc = new BobStaticModelDescriptor("../data/models/props/rocks_3_by_nobiax-d6s8l2b/rocks_03.bob ");
//            }

				smr.model = Renderer.resourceManager.getResource(mdesc) as StaticModel;
				Renderer.renderables.Add(smr);
				smr.setPosition(new Vector3((rand.Next() % size) - halfSize, 0, (rand.Next() % size) - halfSize));
				//smr.model.myMeshes[0].material.myFeatures = Material.Feature.DiffuseMap; //turn off lighting
			}

         //create a test cube
         //          StaticModelRenderable testRenderable = new StaticModelRenderable();
         //          ObjModelDescriptor testDesc;
         //          testDesc = new ObjModelDescriptor("../data/models/props/testCube/testCube.obj");
         //          testRenderable.model = Renderer.resourceManager.getResource(testDesc) as StaticModel;
         //          Renderer.renderables.Add(testRenderable);
         //          testRenderable.setPosition(new Vector3(0, 1, -2));

         //create a skinned model instance
         //          mySkinnedModel = new SkinnedModelRenderable();
         // 			//MS3DModelDescriptor skmd = new MS3DModelDescriptor("../data/models/characters/zombie/zombie.json");
         // 			IQModelDescriptor skmd = new IQModelDescriptor("../data/models/characters/mrFixIt/mrFixIt.json");
         // 			mySkinnedModel.model = Renderer.resourceManager.getResource(skmd) as SkinnedModel;
         // 			mySkinnedModel.controllers.Add(new AnimationController(mySkinnedModel));
         // 			Renderer.renderables.Add(mySkinnedModel);
         // 			mySkinnedModel.setPosition(new Vector3(5, 0, 0));
         // 			(mySkinnedModel.findController("animation") as AnimationController).startAnimation("idle");

         StaticModelRenderable testRenderable = new StaticModelRenderable();
         ObjModelDescriptor  maskDesc;
         maskDesc = new ObjModelDescriptor("../data/models/props/cockpitMask/mask.obj");
         mask = Renderer.resourceManager.getResource(maskDesc) as StaticModel;

         //add a sun for light
         mySun = new LightRenderable();
         mySun.myLightType = LightRenderable.Type.DIRECTIONAL;
         mySun.color = Color4.White;
         mySun.position = new Vector3(5, 5, 5);
			Renderer.renderables.Add(mySun);

			//add a point light
// 			myPoint1 = new LightRenderable();
// 			myPoint1.myLightType = LightRenderable.Type.POINT;
// 			myPoint1.color = Color4.Red;
// 			myPoint1.position = new Vector3(2.5f, 1, 0);
// 			myPoint1.size = 10;
// 			myPoint1.linearAttenuation = 1.0f;
//          myPoint1.quadraticAttenuation = 0.5f;
// 			Renderer.renderables.Add(myPoint1);

			//add another point light
// 			myPoint2 = new LightRenderable();
// 			myPoint2.myLightType = LightRenderable.Type.POINT;
// 			myPoint2.color = Color4.Blue;
// 			myPoint2.position = new Vector3(5.5f, 1, 0);
// 			myPoint2.size = 10;
// 			myPoint2.linearAttenuation = 1.0f;
//          myPoint2.quadraticAttenuation = 0.25f;
// 			Renderer.renderables.Add(myPoint2);

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


