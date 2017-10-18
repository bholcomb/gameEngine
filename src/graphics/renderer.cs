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

		public static Dictionary<String, SceneGraph> scenes;
		public static Dictionary<String, View> views;
		public static Dictionary<String, Visualizer> visualizers;
		public static List<Camera> activeCameras;
		public static List<View> activeViews;

		public static Device device;
		public static ResourceManager resourceManager;
		public static ShaderManager shaderManager;
		public static VisibilityManager visiblityManager;
		public static List<Renderable> renderables;
		public static int frameNumber { get; set; }

		public static PresentFunction present;

		public class RenderStats
		{
			public float alpha = 0.1f;
			public int renderableCount;
			public double cullTime;
			public double extractTime;
			public double prepareTime;
			public double submitTime;
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

			scenes = new Dictionary<string, SceneGraph>();
			views = new Dictionary<string, View>();
			visualizers = new Dictionary<string, Visualizer>();
			renderables = new List<Renderable>();

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
				InitTable sceneConfigs = initializer.findDataOr<InitTable>("scenes", null);
				if (sceneConfigs != null)
				{
					List<string> keys = sceneConfigs.keys;
					for (int i = 0; i < keys.Count; i++)
					{
						SceneGraph sg = new SceneGraph();
						sg.init(sceneConfigs.findData<InitTable>(keys[i]));
						scenes[sceneConfigs.keys[i]] = sg;
					}
				}
			}
		}

		public static void render()
		{
			//prep the frame
			frameNumber++;
			stats.reset();

			double tdiff = 0;

			#region cull phase
			stats.renderableCount = renderables.Count;
			tdiff = TimeSource.currentTime();
			//get a list of all the views in the order they should be processed from the scene graph
			//views in the same renderstage are processed in order of registration
			updateActiveViews();
			updateActiveCameras();

			//update cameras being used
			visiblityManager.updateCameraVisibilityList(activeViews);

			//update renderable objects in each camera
			visiblityManager.updateCameraVisibleObjects(renderables);  //actual camera culling happens here

			//get camera visible stats
			foreach (Camera c in activeCameras)
				stats.cameraVisibles.Add(visiblityManager.renderableCount(c));

			//for each state in the scenegraph update the view visibility list
			//done in parallel
			updateSceneViewVisibility(activeViews);

			tdiff = TimeSource.currentTime() - tdiff;
			stats.cullTime = (1.0f - stats.alpha) * stats.cullTime + (stats.alpha) * tdiff;
			#endregion

			#region extract phase
			tdiff = TimeSource.currentTime();

			//prep each visualizer for the beginning of the frame
			foreach (Visualizer vis in visualizers.Values)
				vis.onFrameBeginExtract();

			//run per-frame extract for each visible renderable, visualizer will need to protect against the same 
			//renderable being submitted more than once for a different camera
			foreach (Camera c in activeCameras)
			{
				foreach (Renderable r in visiblityManager.camaraVisibles(c))
				{
					Visualizer vis = visualizers[r.type];
					vis.extractPerFrame(r);
				}
			}

			//run extract for each view
			foreach (View v in activeViews)
			{
				v.extract();
			}

			//finalize extract for each visualizer
			foreach (Visualizer vis in visualizers.Values)
				vis.onFrameExtractFinalize();

			tdiff = TimeSource.currentTime() - tdiff;
			stats.extractTime = (1.0f - stats.alpha) * stats.extractTime + (stats.alpha) * tdiff;
			#endregion

			#region prepare phase
			tdiff = TimeSource.currentTime();

			foreach (Visualizer vis in visualizers.Values)
				vis.onFrameBeginPrepare();

			//run per-frame prepare for each visible renderable, visualizer will need to protect against the same 
			//renderable being submitted more than once for a different camera
			foreach (Camera c in activeCameras)
			{
				foreach (Renderable r in visiblityManager.camaraVisibles(c))
				{
					Visualizer vis = visualizers[r.type];
					vis.preparePerFrame(r);					
				}
			}

			foreach (View v in activeViews)
			{
				v.prepare();
			}

			foreach (Visualizer vis in visualizers.Values)
				vis.onFramePrepareFinalize();

			tdiff = TimeSource.currentTime() - tdiff;
			stats.prepareTime = (1.0f - stats.alpha) * stats.prepareTime + (stats.alpha) * tdiff;
			#endregion

			#region submit phase
			tdiff = TimeSource.currentTime();
			foreach (View v in activeViews)
			{
				//generate the commands
				v.submit();
				stats.viewStats.Add(v.stats);
			}
			tdiff = TimeSource.currentTime() - tdiff;
			stats.submitTime = (1.0f - stats.alpha) * stats.submitTime + (stats.alpha) * tdiff;
			#endregion

			#region execute phase
			tdiff = TimeSource.currentTime();
			foreach (SceneGraph sg in scenes.Values)
			{
				if (sg.isActive == false)
					continue;

				foreach (RenderStage rs in sg.renderStages)
				{
					if (rs.isActive == false)
						continue;

					rs.preExectue();

					device.executeCommandList(rs.preStageCommands);

					foreach (View v in rs.views)
					{
						if (v.isActive == true)
						{
							IEnumerable<BaseRenderQueue> renderQueues = v.getRenderQueues();
							foreach (BaseRenderQueue rq in renderQueues)
								device.executeRenderQueue(rq);
						}
					}

					device.executeCommandList(rs.postStageCommands);

					rs.postExectue();

					//cleanup the pre/post command queues
					rs.preStageCommands.Clear();
					rs.postStageCommands.Clear();
				}
			}
			tdiff = TimeSource.currentTime() - tdiff;
			stats.executeTime = (1.0f - stats.alpha) * stats.executeTime + (stats.alpha) * tdiff;
			#endregion

			//present
			if(present != null)
				present();
		}

		public static void registerVisualizer(String name, Visualizer vis)
		{
			visualizers[name] = vis;
		}

		#region culling functions
		static void updateActiveViews()
		{
			activeViews.Clear();

			foreach (SceneGraph sg in scenes.Values)
			{
				if (sg.isActive == false)
					continue;

				foreach (RenderStage rs in sg.renderStages)
				{
					if (rs.isActive == false)
						continue;

					foreach (View v in rs.views)
					{
						if (v.isActive == true)
							activeViews.Add(v);
					}
				}
			}
		}

		static void updateSceneViewVisibility(List<View> sceneviews)
		{
			Parallel.ForEach(sceneviews, (view) =>
			{
				IEnumerable<Renderable> cameraVisibles = visiblityManager.camaraVisibles(view.camera);
				view.updateVisibles(cameraVisibles);
			});
		}

		#endregion

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

		static void installDefaultVisualizers()
		{
			registerVisualizer("skybox", new SkyboxVisualizer());
			registerVisualizer("light", new LightVisualizer());
			registerVisualizer("staticModel", new StaticModelVisualizer());
			registerVisualizer("skinnedModel", new SkinnedModelVisualizer());
			//          visualizers["particle"] = new ParticleVisualizer();
		}

		static void installDefaultShaders()
		{
			ShaderProgramDescriptor sd;
			ShaderProgram sp;
			List<ShaderDescriptor> desc = new List<ShaderDescriptor>();

			desc.Add(new ShaderDescriptor(ShaderType.VertexShader, "..\\src\\Graphics\\shaders\\skybox-vs.glsl", ShaderDescriptor.Source.File));
			desc.Add(new ShaderDescriptor(ShaderType.FragmentShader, "..\\src\\Graphics\\shaders\\skybox-ps.glsl", ShaderDescriptor.Source.File));
			sd = new ShaderProgramDescriptor(desc);
			sp = resourceManager.getResource(sd) as ShaderProgram;
			visualizers["skybox"].registerEffect("skybox", new SkyboxEffect(sp));


			desc.Clear();
			desc.Add(new ShaderDescriptor(ShaderType.VertexShader, "..\\src\\Graphics\\shaders\\staticModel-vs.glsl", ShaderDescriptor.Source.File));
			desc.Add(new ShaderDescriptor(ShaderType.FragmentShader, "..\\src\\Graphics\\shaders\\unlit-ps.glsl", ShaderDescriptor.Source.File));
			sd = new ShaderProgramDescriptor(desc);
			sp = resourceManager.getResource(sd) as ShaderProgram;
			visualizers["staticModel"].registerEffect("forward-lighting", new UnlitEffect(sp));

			desc.Clear();
			desc.Add(new ShaderDescriptor(ShaderType.VertexShader, "..\\src\\Graphics\\shaders\\staticModel-vs.glsl", ShaderDescriptor.Source.File));
			desc.Add(new ShaderDescriptor(ShaderType.FragmentShader, "..\\src\\Graphics\\shaders\\perpixel-lighting-ps.glsl", ShaderDescriptor.Source.File));
			sd = new ShaderProgramDescriptor(desc);
			sp = resourceManager.getResource(sd) as ShaderProgram;
			visualizers["staticModel"].registerEffect("forward-lighting", new PerPixelLightinglEffect(sp));


			desc.Clear();
			desc.Add(new ShaderDescriptor(ShaderType.VertexShader, "..\\src\\Graphics\\shaders\\skinnedModel-vs.glsl", ShaderDescriptor.Source.File));
			desc.Add(new ShaderDescriptor(ShaderType.FragmentShader, "..\\src\\Graphics\\shaders\\unlit-ps.glsl", ShaderDescriptor.Source.File));
			sd = new ShaderProgramDescriptor(desc);
			sp = resourceManager.getResource(sd) as ShaderProgram;
			visualizers["skinnedModel"].registerEffect("forward-lighting", new UnlitEffect(sp));


			desc.Clear();
			desc.Add(new ShaderDescriptor(ShaderType.VertexShader, "..\\src\\Graphics\\shaders\\skinnedModel-vs.glsl", ShaderDescriptor.Source.File));
			desc.Add(new ShaderDescriptor(ShaderType.FragmentShader, "..\\src\\Graphics\\shaders\\perpixel-lighting-ps.glsl", ShaderDescriptor.Source.File));
			sd = new ShaderProgramDescriptor(desc);
			sp = resourceManager.getResource(sd) as ShaderProgram;
			visualizers["skinnedModel"].registerEffect("forward-lighting", new PerPixelLightinglEffect(sp));
		}
	}
}