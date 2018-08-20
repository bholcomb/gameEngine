using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using Util;

namespace Graphics
{
	public static class Renderer
	{
		public delegate void PresentFunction();

		public static Dictionary<String, View> views;
		public static Dictionary<String, Visualizer> visualizers;
		public static List<Camera> activeCameras;
		public static List<View> activeViews;

		public static Device device;
		public static ResourceManager resourceManager;
		public static ShaderManager shaderManager;
		public static VisibilityManager visiblityManager;
		public static Scene scene;
		public static int frameNumber { get; set; }

      public delegate void RendererFunction();
      public static event RendererFunction onPreRender;
      public static event RendererFunction onPostRender;
      public static event RendererFunction onPreCull;
      public static event RendererFunction onPostCull;
      public static event RendererFunction onPrePrepare;
      public static event RendererFunction onPostPrepare;
      public static event RendererFunction onPreGenerateCommands;
      public static event RendererFunction onPostGenerateCommands;
      public static event RendererFunction onPreExecuteCommands;
      public static event RendererFunction onPostExecuteCommands;
      public static event RendererFunction onPrePresent;
      public static event RendererFunction onPostPresent;

      public static PresentFunction present;

		public class RenderStats
		{
			public float alpha = 0.1f;
			public int renderableCount;
			public double cullTime;
			public double prepareTime;
			public double generateTime;
			public double executeTime;
			public List<int> cameraVisibles = new List<int>();
			public List<ViewStats> viewStats = new List<ViewStats>();

			public void reset()
			{
				cameraVisibles.Clear();
				viewStats.Clear();
			}
		}

		public static RenderStats stats = new RenderStats();

		static Renderer()
		{
			frameNumber = 0;

			device = new Device();
			device.init();

			resourceManager = new ResourceManager();
			shaderManager = new ShaderManager();
			visiblityManager = new VisibilityManager();

			views = new Dictionary<string, View>();
			visualizers = new Dictionary<string, Visualizer>();
			scene = new Scene();

			activeViews = new List<View>();
			activeCameras = new List<Camera>();

			DebugRenderer.init();

			present = null;

			installDefaultVisualizers();
			installDefaultShaders();
		}

		public static void init(InitTable initializer = null)
		{
			if (initializer != null)
			{
            //configure any render specific stuff here
			}
		}

		public static void render()
		{
			//prep the frame
         if(onPreRender != null)
         {
            onPreRender();
         }

			frameNumber++;
			stats.reset();

			double tdiff = 0;

         #region cull phase
         if (onPreCull != null)
         {
            onPreCull();
         }

         stats.renderableCount = scene.renderables.Count;
			tdiff = TimeSource.currentTime();
			//get a list of all the views in the order they should be processed
			updateActiveViews();
			updateActiveCameras();

			//update renderable objects in each camera
			visiblityManager.cullRenderablesPerCamera(scene.renderables, activeViews); 

         //get camera visible stats
         foreach (Camera c in activeCameras)
         {
            stats.cameraVisibles.Add(visiblityManager.renderableCount(c));
         }

			//for each view, update the list of renderables used in each of its passes
			//done in parallel
			updateViewRenderables(activeViews);

			tdiff = TimeSource.currentTime() - tdiff;
			stats.cullTime = (1.0f - stats.alpha) * stats.cullTime + (stats.alpha) * tdiff;

         if (onPostCull != null)
         {
            onPostCull();
         }
         #endregion

         #region prepare phase
         if (onPrePrepare != null)
         {
            onPrePrepare();
         }

         tdiff = TimeSource.currentTime();

         foreach (Visualizer vis in visualizers.Values)
         {
            vis.prepareFrameBegin();
         }

			//run per-frame prepare for each visible renderable, visualizer will need to protect against the same 
			//renderable being prepared more than once for a different camera
			foreach (Camera c in activeCameras)
			{
				foreach (Renderable r in visiblityManager.camaraVisibles(c))
				{
					Visualizer vis = visualizers[r.type];
					vis.preparePerFrame(r);					
				}
			}

         //prepare each renderable per view (will process per-pass)
			foreach (View v in activeViews)
			{
				v.prepare();
			}

         foreach (Visualizer vis in visualizers.Values)
         {
            vis.prepareFrameFinalize();
         }

			tdiff = TimeSource.currentTime() - tdiff;
			stats.prepareTime = (1.0f - stats.alpha) * stats.prepareTime + (stats.alpha) * tdiff;

         if (onPostPrepare != null)
         {
            onPostPrepare();
         }
         #endregion

         #region generate command list phase
         if (onPreGenerateCommands != null)
         {
            onPreGenerateCommands();
         }

         tdiff = TimeSource.currentTime();

			foreach (View v in activeViews)
			{
				//generate the commands
				v.generateRenderCommandLists();
				stats.viewStats.Add(v.stats);
			}

			tdiff = TimeSource.currentTime() - tdiff;
			stats.generateTime = (1.0f - stats.alpha) * stats.generateTime + (stats.alpha) * tdiff;

         if (onPostGenerateCommands != null)
         {
            onPostGenerateCommands();
         }
         #endregion

         #region execute phase
         if (onPreExecuteCommands != null)
         {
            onPreExecuteCommands();
         }

         tdiff = TimeSource.currentTime();
         foreach (View v in activeViews)
         {
            foreach(RenderCommandList cmdList in v.getRenderCommandLists())
            {
               if (cmdList.Count > 0)
               {
                  device.executeCommandList(cmdList);
               }
            }
			}
			tdiff = TimeSource.currentTime() - tdiff;
			stats.executeTime = (1.0f - stats.alpha) * stats.executeTime + (stats.alpha) * tdiff;

         if (onPostExecuteCommands != null)
         {
            onPostExecuteCommands();
         }
         #endregion

         #region present phase
         if (present != null)
         {
            device.pushDebugMarker("Renderer Present");

            if (onPrePresent != null)
            {
               onPrePresent();
            }

            present();

            if (onPostPresent != null)
            {
               onPostPresent();
            }

            device.popDebugMarker();
         }
         #endregion

         if (onPostRender != null)
         {
            onPostRender();
         }
      }

		public static void registerVisualizer(String name, Visualizer vis)
		{
			visualizers[name] = vis;
		}

      public static void addView(View v)
      {
         views[v.name] = v;
      }

      public static void removeView(View v)
      {
         if(views.ContainsKey(v.name))
         {
            views.Remove(v.name);
         }
      }


		#region culling functions
		static void updateActiveViews()
		{
			activeViews.Clear();

         foreach(View v in views.Values)
         {
            v.getViews(activeViews);
         }
		}

		static void updateViewRenderables(List<View> activeViews)
		{
			Parallel.ForEach(activeViews, (view) =>
			{
            if (view.processRenderables == true)
            {
               IEnumerable<Renderable> cameraVisibles = visiblityManager.camaraVisibles(view.camera);
               view.updateVisableRenderables(cameraVisibles);
            }
			});
		}

		static void updateActiveCameras()
		{
			activeCameras.Clear();

			foreach (View view in activeViews)
			{
            if(activeCameras.Contains(view.camera) == false)
				{
					activeCameras.Add(view.camera);
				}
			}
		}

		#endregion

      #region default visualizers and effects/shaders

      static void installDefaultVisualizers()
		{
			registerVisualizer("skybox", new SkyboxVisualizer());
			registerVisualizer("light", new LightVisualizer());
			registerVisualizer("staticModel", new StaticModelVisualizer());
			registerVisualizer("skinnedModel", new SkinnedModelVisualizer());
			registerVisualizer("particle", new ParticleVisualizer());
		}

		static void installDefaultShaders()
		{
			ShaderProgramDescriptor sd;
			ShaderProgram sp;
			List<ShaderDescriptor> desc = new List<ShaderDescriptor>();

			desc.Add(new ShaderDescriptor(ShaderType.VertexShader, "..\\src\\Graphics\\shaders\\skybox-vs.glsl", ShaderDescriptor.Source.File));
			desc.Add(new ShaderDescriptor(ShaderType.FragmentShader, "..\\src\\Graphics\\shaders\\skybox-ps.glsl", ShaderDescriptor.Source.File));
			sd = new ShaderProgramDescriptor(desc, null, "skyboxShader");
			sp = resourceManager.getResource(sd) as ShaderProgram;
			visualizers["skybox"].registerEffect("skybox", new SkyboxEffect(sp));


			desc.Clear();
			desc.Add(new ShaderDescriptor(ShaderType.VertexShader, "..\\src\\Graphics\\shaders\\staticModel-vs.glsl", ShaderDescriptor.Source.File));
			desc.Add(new ShaderDescriptor(ShaderType.FragmentShader, "..\\src\\Graphics\\shaders\\unlit-ps.glsl", ShaderDescriptor.Source.File));
			sd = new ShaderProgramDescriptor(desc, null, "static:unlit");
			sp = resourceManager.getResource(sd) as ShaderProgram;
			visualizers["staticModel"].registerEffect("forward-lighting", new UnlitEffect(sp));

			desc.Clear();
			desc.Add(new ShaderDescriptor(ShaderType.VertexShader, "..\\src\\Graphics\\shaders\\staticModel-vs.glsl", ShaderDescriptor.Source.File));
			desc.Add(new ShaderDescriptor(ShaderType.FragmentShader, "..\\src\\Graphics\\shaders\\perpixel-lighting-ps.glsl", ShaderDescriptor.Source.File));
			sd = new ShaderProgramDescriptor(desc, null, "static:perpixel");
			sp = resourceManager.getResource(sd) as ShaderProgram;
			visualizers["staticModel"].registerEffect("forward-lighting", new PerPixelLightinglEffect(sp));


			desc.Clear();
			desc.Add(new ShaderDescriptor(ShaderType.VertexShader, "..\\src\\Graphics\\shaders\\skinnedModel-vs.glsl", ShaderDescriptor.Source.File));
			desc.Add(new ShaderDescriptor(ShaderType.FragmentShader, "..\\src\\Graphics\\shaders\\unlit-ps.glsl", ShaderDescriptor.Source.File));
			sd = new ShaderProgramDescriptor(desc, null, "skinned:unlit");
			sp = resourceManager.getResource(sd) as ShaderProgram;
			visualizers["skinnedModel"].registerEffect("forward-lighting", new UnlitEffect(sp));


			desc.Clear();
			desc.Add(new ShaderDescriptor(ShaderType.VertexShader, "..\\src\\Graphics\\shaders\\skinnedModel-vs.glsl", ShaderDescriptor.Source.File));
			desc.Add(new ShaderDescriptor(ShaderType.FragmentShader, "..\\src\\Graphics\\shaders\\perpixel-lighting-ps.glsl", ShaderDescriptor.Source.File));
			sd = new ShaderProgramDescriptor(desc, null, "skinned:perpixel");
			sp = resourceManager.getResource(sd) as ShaderProgram;
			visualizers["skinnedModel"].registerEffect("forward-lighting", new PerPixelLightinglEffect(sp));
		}

      #endregion
   }
}
