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
      public int commandLists;
      public List<PassStats> passStats = new List<PassStats>();
   }

	public class View
   {
      public View child = null;
      public View sibling = null;

		public bool isActive { get; set; }
      public String name { get; set; }
      public bool processRenderables { get; set; } //used by UI and debug views so they don't process renderables since they generate their own

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
      public event ViewFunction PrePrepare;
      public event ViewFunction PostPrepare;
      public event ViewFunction PreGenerateCommands;
      public event ViewFunction PostGenerateCommands;

      public View(String viewName, Camera c, Viewport v)
      {
			name = viewName;
			camera = c;
			viewport = v;
			isActive = true;
         myPasses = new List<Pass>();
         myRenderCommandLists = new List<RenderCommandList>();
         myVisibleRenderablesByType = new Dictionary<string, List<Renderable>>();
         processRenderables = true;

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
         Renderer.device.pushDebugMarker(String.Format("View {0}-prepare", name));
         onPrePrepare();

         if (camera != null)
         {
            camera.updateCameraUniformBuffer();
         }

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

         onPostPrepare();
         Renderer.device.popDebugMarker();
      }

      public virtual void generateRenderCommandLists()
      {
         preCommands.Clear();
         postCommands.Clear();

         preCommands.Add(new PushDebugMarkerCommand(String.Format("View {0}-execute", name)));


         onPreGenerateCommands();

         //reset the device so this view can update as appropriate
         preCommands.Add(new DeviceResetCommand());

         //bind the camera for the view
         preCommands.Add(new BindCameraCommand(camera));

         foreach (Pass p in myPasses)
         {
            p.generateRenderCommandLists();
         }

         onPostGenerateCommands();

         postCommands.Add(new PopDebugMarkerCommand());
      }

      public virtual List<RenderCommandList> getRenderCommandLists()
      { 
         //update stats
         stats.name = name;
         stats.passStats.Clear();


         myRenderCommandLists.Clear();

         myRenderCommandLists.Add(preCommands);

         foreach (Pass p in myPasses)
         {
            p.getRenderCommands(myRenderCommandLists);
            stats.passStats.Add(p.stats);
         }

         myRenderCommandLists.Add(postCommands);

         //update render call stats
         stats.commandLists = myRenderCommandLists.Count;

			return myRenderCommandLists;
		}

      #region pass and child/sibling view management
      public virtual void addPass(Pass p)
      {
         p.view = this;
         myPasses.Add(p);
      }

      public virtual void removePass(Pass p)
      {
         myPasses.Remove(p);
      }

      public virtual void removePass(String name)
      {
         foreach(Pass p in myPasses)
         {
            if(p.name == name)
            {
               myPasses.Remove(p);
               return;
            }
         }
      }

      public virtual Pass findPass(string name)
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

      public virtual void getViews(List<View> views)
      {
         if(child != null)
         {
            child.getViews(views);
         }

         if (camera != null)
         {
            views.Add(this);
         }

         if(sibling != null)
         {
            sibling.getViews(views);
         }

      }

      public virtual void addChild(View v)
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

      public virtual void addSibling(View v)
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

      #region protected onEvent functions
      protected void onPrePrepare()
      {
         if(PrePrepare != null)
         {
            PrePrepare(this);
         }
      }

      protected void onPostPrepare()
      {
         if (PostPrepare != null)
         {
            PostPrepare(this);
         }
      }

      protected void onPreGenerateCommands()
      {
         if (PreGenerateCommands != null)
         {
            PreGenerateCommands(this);
         }
      }

      protected void onPostGenerateCommands()
      {
         if (PostGenerateCommands != null)
         {
            PostGenerateCommands(this);
         }
      }

      #endregion

   }
}