using System;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using Util;

namespace Graphics
{
   public class PassStats
   {
      public string name;
      public string technique;
      public int queueCount;
      public int renderCalls;
   }

   public class Pass
   {
      protected string myName;
      protected string myTechnique;
      public RenderTarget renderTarget { get; set; }
      public bool clearTarget { get; set; }
      public Color4 clearColor { get; set; }
      public ClearBufferMask clearMask { get; set; }
      public View view { get; set; }

      public string name { get { return myName; } }
      public string technique { get { return myTechnique; } }


      public RenderableFilter filter;

      protected Dictionary<UInt64, BaseRenderQueue> myRenderQueues;
      public Dictionary<UInt64, BaseRenderQueue> renderQueues { get { return myRenderQueues; } }
      protected Dictionary<string, List<Renderable>> myVisibleRenderablesByType;
      public Dictionary<string, List<Renderable>> visibleRenderablesByType { get { return myVisibleRenderablesByType; } }
      

      public delegate void PassFunction(Pass pass);
      public event PassFunction PrePrepare;
      public event PassFunction PostPrepare;
      public event PassFunction PreGenerateCommands;
      public event PassFunction PostGenerateCommands;

      public RenderCommandList preCommands;
      public RenderCommandList postCommands;

      public PassStats stats = new PassStats();

      public Pass(string name, string technique)
      {
         myName = name;
         myTechnique = technique;
         renderTarget = null;
         clearTarget = false;
         myRenderQueues = new Dictionary<ulong, BaseRenderQueue>();
         myVisibleRenderablesByType = new Dictionary<string, List<Renderable>>();
         preCommands = new RenderCommandList();
         postCommands = new RenderCommandList();

         clearTarget = false;
         clearColor = new Color4(0.0f, 0.0f, 0.0f, 0.0f);
         clearMask = ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit;
      }

      public Pass(Pass p) 
         : this(p.name, p.technique)
      {
         filter = p.filter;
         renderTarget = p.renderTarget;
         clearTarget = p.clearTarget;
         clearColor = p.clearColor;
         clearMask = p.clearMask;
      }

      public virtual void updateVisibleRenderables(IEnumerable<Renderable> cameraVisibles)
      {
         foreach (List<Renderable> tl in myVisibleRenderablesByType.Values)
         {
            tl.Clear();
         }

         foreach (Renderable r in cameraVisibles)
         {
            if (filter.shouldAccept(r) == true)
            {
               List<Renderable> tl = null;
               if (myVisibleRenderablesByType.TryGetValue(r.type, out tl) == false)
               {
                  tl = new List<Renderable>();
                  myVisibleRenderablesByType[r.type] = tl;
               }

               tl.Add(r);
            }
         }
      }

      public virtual void prepare()
      {
         Renderer.device.pushDebugMarker(String.Format("Pass {0}:{1}-prepare", view.name, name));

         onPrePrepare();

         foreach (BaseRenderQueue rq in myRenderQueues.Values)
         {
            rq.reset();
         }

         //create the render infos for each renderable and put it in it's appropriate render queue
         foreach (String visType in myVisibleRenderablesByType.Keys)
         {
            Visualizer visualizer = Renderer.visualizers[visType];

            visualizer.preparePerPassBegin(this);

            List<Renderable> renderables = myVisibleRenderablesByType[visType];

            foreach (Renderable r in renderables)
            {
               visualizer.preparePerPass(r, this);
            }

            visualizer.preparePerPassFinalize(this);
         }

         onPostPrepare();

         Renderer.device.popDebugMarker();
      }

      public virtual void generateRenderCommandLists()
      {
         preCommands.Clear();
         postCommands.Clear();

         preCommands.Add(new PushDebugMarkerCommand(String.Format("Pass {0}:{1}-execute", view.name, name)));

         if (renderTarget != null)
         {
            preCommands.Add(new SetRenderTargetCommand(renderTarget));
            if (clearTarget == true)
            {
               preCommands.Add(new ClearColorCommand(clearColor));
               preCommands.Add(new ClearCommand(clearMask));
            }
         }

         //called after setting render target so that any user commands inserted will affect (or change) the render target
         onPreGenerateCommands();

         stats.queueCount = myRenderQueues.Count;
         stats.renderCalls = 0;
         stats.name = name;
         stats.technique = technique;

         //add the view specific commands for each render queue
         foreach (BaseRenderQueue rq in myRenderQueues.Values)
         {
            rq.commands.Clear();
            
            rq.generateRenderCommands();

            stats.renderCalls += rq.commands.Count;
         }

         onPostGenerateCommands();

         postCommands.Add(new PopDebugMarkerCommand());
      }

      public virtual void getRenderCommands(List<RenderCommandList> renderCmdLists)
      {
         renderCmdLists.Add(preCommands);

         List<BaseRenderQueue> rqList = new List<BaseRenderQueue>();
         foreach (BaseRenderQueue r in myRenderQueues.Values)
         {
            rqList.Add(r);
         }

         //sort by pipeline state, puts opaques before transparents
         rqList.Sort((a, b) => a.myPipeline.id.CompareTo(b.myPipeline.id));

         foreach (BaseRenderQueue rq in rqList)
         {
            renderCmdLists.Add(rq.commands);
         }

         renderCmdLists.Add(postCommands);
      }

      public virtual BaseRenderQueue findRenderQueue(UInt64 pipelineId)
      {
         BaseRenderQueue rq = null;
         myRenderQueues.TryGetValue(pipelineId, out rq);
         return rq;
      }

      public virtual void registerQueue(BaseRenderQueue rq)
      {
         if (myRenderQueues.ContainsKey(rq.myPipeline.id) == false)
         {
            myRenderQueues[rq.myPipeline.id] = rq;
         }
      }

      #region protected onEvent functions
      protected void onPrePrepare()
      {
         if (PrePrepare != null)
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