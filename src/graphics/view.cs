using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;


using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using Util;

namespace Graphics
{
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
      public String name;
      public int passes;
      public int renderCalls;
   }

	public class View
   {
      public View child = null;
      public View sibling = null;

		public bool isActive { get; set; }
      public String name { get; set; }

      public Camera camera { get; set; }
      public Viewport viewport { get; set; }

      protected List<Pass> myPasses;
      public List<Pass> passes { get { return myPasses; } }
      protected List<RenderCommandList> myRenderCommandLists;
      public List<RenderCommandList> renderCommandLists { get { return myRenderCommandLists; } }

      protected Dictionary<string, List<Renderable>> myVisibleRenderablesByType;
      public Dictionary<string, List<Renderable>> visibleRenderablesByType { get { return myVisibleRenderablesByType; } }

      public ViewStats stats = new ViewStats();

      public RenderCommandList preCommands;
      public RenderCommandList postCommands;

      public delegate void ViewFunction(View view);
      public event ViewFunction onPrePrepare;
      public event ViewFunction onPostPrepare;
      public event ViewFunction onPreGenerateCommands;
      public event ViewFunction onPostGenerateCommands;

      public View(String viewName, Camera c, Viewport v)
      {
			name = viewName;
			camera = c;
			viewport = v;
			isActive = true;
         myPasses = new List<Pass>();
         myRenderCommandLists = new List<RenderCommandList>();
         myVisibleRenderablesByType = new Dictionary<string, List<Renderable>>();

         preCommands = new RenderCommandList();
         postCommands = new RenderCommandList();
      }

		public virtual void updateVisableRenderables(IEnumerable<Renderable> cameraVisibles)
      {
         foreach (List<Renderable> tl in myVisibleRenderablesByType.Values)
         {
            tl.Clear();
         }

         foreach (Renderable r in cameraVisibles)
         {
            List<Renderable> typeList = null;
            if (myVisibleRenderablesByType.TryGetValue(r.type, out typeList) == false)
            {
               typeList = new List<Renderable>();
               myVisibleRenderablesByType[r.type] = typeList;
            }

            typeList.Add(r);
         }

         foreach(Pass p in myPasses)
         {
            p.updateVisibleRenderables(cameraVisibles);
         }
      }

      public virtual void prepare()
      {
         if (onPrePrepare != null)
         {
            onPrePrepare(this);
         }

         camera.updateCameraUniformBuffer();

         //per view prepare
         foreach (String visType in myVisibleRenderablesByType.Keys)
         {
            Visualizer visualizer = Renderer.visualizers[visType];

            visualizer.preparePerViewBegin(this);

            List<Renderable> renderables = myVisibleRenderablesByType[visType];

            foreach (Renderable r in renderables)
            {
               visualizer.preparePerView(r, this);
            }

            visualizer.preparePerViewFinalize(this);
         }

         //per pass prepare
         foreach (Pass p in passes)
         {
            p.prepare();
         }

         if (onPostPrepare != null)
         {
            onPostPrepare(this);
         }
		}

      public virtual void generateRenderCommandLists()
      {
         preCommands.Clear();
         postCommands.Clear();

         if (onPreGenerateCommands != null)
         {
            onPreGenerateCommands(this);
         }

         //reset the device so this view can update as appropriate
         preCommands.Add(new DeviceResetCommand());


         foreach (Pass p in myPasses)
         {
            p.generateRenderCommandLists();
         }

         if (onPostGenerateCommands != null)
         {
            onPostGenerateCommands(this);
         }
      }

      public virtual List<RenderCommandList> getRenderCommandLists()
      { 
         //update stats
         stats.passes = myPasses.Count;
         stats.name = name;
         stats.renderCalls = 0;

         myRenderCommandLists.Clear();

         myRenderCommandLists.Add(preCommands);

         foreach (Pass p in myPasses)
         {
            p.getRenderCommands(myRenderCommandLists);
         }

         myRenderCommandLists.Add(postCommands);

         //update render call stats
         foreach(RenderCommandList rcl in myRenderCommandLists)
         {
            stats.renderCalls += rcl.Count;
         }

			return myRenderCommandLists;
		}

      #region pass and child/sibling view management
      public void addPass(Pass p)
      {
         p.view = this;
         myPasses.Add(p);
      }

      public void removePass(Pass p)
      {
         myPasses.Remove(p);
      }

      public Pass findPass(string name)
      {
         foreach(Pass p in myPasses)
         {
            if(p.name == name)
            {
               return p;
            }
         }

         return null;
      }

      public void getViews(List<View> views)
      {
         if(child != null)
         {
            child.getViews(views);
         }

         views.Add(this);

         if(sibling != null)
         {
            sibling.getViews(views);
         }

      }

      public void addChild(View v)
      {
         if(child == null)
         {
            child = v;
         }
         else
         {
            child.addSibling(v);
         }
      }

      public void addSibling(View v)
      {
         if(sibling == null)
         {
            sibling = v;
         }
         else
         {
            sibling.addSibling(v);
         }
      }

      #endregion
   }
}