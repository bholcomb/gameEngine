using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;


using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using Util;

namespace Graphics
{
	public abstract class RenderableFilter
	{
		public RenderableFilter() { }
		public abstract bool shouldAccept(Renderable r);
	}

	#region common filters
	public class NullFilter : RenderableFilter
	{
		public NullFilter() : base() { }
		public override bool shouldAccept(Renderable r)
		{
			return false;
		}
	}

	public class TypeFilter : RenderableFilter
	{
		List<String> myTypes;
		public TypeFilter(List<String> acceptedTypes) : base() { myTypes = acceptedTypes; }
		public override bool shouldAccept(Renderable r)
		{
			return myTypes.Contains(r.type);
		}
	}

	public class DistanceFilter : RenderableFilter
	{
		Camera myCamera;
		float myDistanceSquared;
		public DistanceFilter(Camera c, float minDist): base() { myCamera = c; myDistanceSquared = minDist * minDist; }
		public override bool shouldAccept(Renderable r)
		{
			float dist = (r.position - myCamera.position).LengthSquared;
			return dist < myDistanceSquared;
		}
	}

	public class InstanceFilter : RenderableFilter
	{
		List<Renderable> myInstances;
		public InstanceFilter(List<Renderable> renderables) : base () { myInstances = renderables; }
		public override bool shouldAccept(Renderable r)
		{
			return myInstances.Contains(r);
		}
	}

	#endregion

	public class RenderInfo
	{
		public UInt64 sortId;
		public float distToCamera;
		public RenderState renderState = new RenderState();
		public PipelineState pipeline;

		public RenderInfo() { }
	}

   public class ViewStats
   {
      public String viewName;
      public int queueCount;
      public int renderCalls;
   }

	public class View
   {
		public delegate void ViewFunction(View view);

		public bool isActive { get; set; }
      public String name { get; set; }

      public Camera camera { get; set; }
      public Viewport viewport { get; set; }
		public RenderTarget renderTarget { get; set; }
		public String passType { get; set; }
		public bool clearTarget { get; set; }

		Dictionary<string, List<Renderable>> myVisibleRenderables;
		Dictionary<UInt64, BaseRenderQueue> myRenderQueues;

      public ViewStats stats = new ViewStats();

		public RenderableFilter filter;

		public event ViewFunction onPreExtract;
		public event ViewFunction onPostExtract;
		public event ViewFunction onPrePrepare;
		public event ViewFunction onPostPrepare;
		public event ViewFunction onPreSubmit;
		public event ViewFunction onPostSubmit;

		public View(String viewName, Camera c, Viewport v, RenderTarget rt, bool shouldClear=true)
      {
			name = viewName;
			camera = c;
			viewport = v;
			renderTarget = rt;
			clearTarget = shouldClear;

			isActive = true;
			filter =  new NullFilter();
			myVisibleRenderables = new Dictionary<string, List<Renderable>>();
			myRenderQueues = new Dictionary<UInt64, BaseRenderQueue>();
      }

		public virtual void updateVisibles(IEnumerable<Renderable> renderables)
		{
			//cleanup previous frames data
			foreach (List<Renderable> tl in myVisibleRenderables.Values)
				tl.Clear();

			//reset the render queues
			foreach (BaseRenderQueue rq in myRenderQueues.Values)
				rq.reset();

			foreach (Renderable r in renderables)
			{
				if(filter.shouldAccept(r) == true)
				{
					List<Renderable> tl = null;
					if (myVisibleRenderables.TryGetValue(r.type, out tl) == false)
					{
						tl = new List<Renderable>();
						myVisibleRenderables[r.type] = tl;
					}

					tl.Add(r);
				}
			}
		}

      public virtual void extract()
      {
			//create the render infos for each renderable and put it in it's appropriate render queue
			foreach(String visType in myVisibleRenderables.Keys)
			{
				Visualizer visualizer = Renderer.visualizers[visType];
				List<Renderable> renderables = myVisibleRenderables[visType];

				foreach (Renderable r in renderables)
				{
					visualizer.extractPerView(r, this);
				}
			}

			//finalize each render queue's list of render infos 
			foreach (BaseRenderQueue rq in myRenderQueues.Values)
			{
				rq.visualizer.extractPerViewFinalize(rq, this);
				rq.sort();
			}
		}

      public virtual void prepare()
      {
			camera.updateCameraUniformBuffer();

			foreach (BaseRenderQueue rq in myRenderQueues.Values)
			{
				rq.preparetRenderInfo(this);

				rq.visualizer.preparePerViewFinalize(rq, this);
			}
		}

      public virtual void submit()
      {
         stats.queueCount = myRenderQueues.Count;
         stats.renderCalls = 0;
         stats.viewName = name;

			//add the view specific commands for each render queue
			foreach(BaseRenderQueue rq in myRenderQueues.Values)
			{
				rq.commands.Clear();
				rq.addCommand(new SetViewportCommand(camera.viewport()));
				rq.addCommand(new SetRenderTargetCommand(renderTarget));
				rq.addCommand(new SetPipelineCommand(rq.myPipeline));
				if (clearTarget == true)
					rq.addCommand(new ClearCommand());

				rq.addCommand(new BindCameraCommand(camera));

				rq.visualizer.onSubmitNodeBlockBegin(rq);

				rq.submitRenderInfo();

				rq.visualizer.onSubmitNodeBlockEnd(rq);

            stats.renderCalls += rq.commands.Count;
         }
      }

		public IEnumerable<BaseRenderQueue> getRenderQueues()
		{
			List<BaseRenderQueue> rq = new List<BaseRenderQueue>();
			foreach(BaseRenderQueue r in myRenderQueues.Values)
			{
				rq.Add(r);
			}

			//sort by pipeline state, puts opaques before transparents
			rq.Sort((a, b) => a.myPipeline.id.CompareTo(b.myPipeline.id));

			return rq;
		}

		public void registerQueue(BaseRenderQueue rq)
		{
			if (myRenderQueues.ContainsKey(rq.myPipeline.id) == false)
				myRenderQueues[rq.myPipeline.id] = rq;
		}
	}
}