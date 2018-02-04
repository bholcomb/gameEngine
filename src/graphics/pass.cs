using System;
using System.Collections.Generic;

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
      string myName;
      string myTechnique;
      RenderTarget myRenderTarget;
      public bool clearTarget { get; set; }
      public View view { get; set; }

      
      public RenderableFilter filter;

      Dictionary<UInt64, BaseRenderQueue> myRenderQueues;
      public Dictionary<UInt64, BaseRenderQueue> renderQueues { get { return myRenderQueues; } }
      Dictionary<string, List<Renderable>> myVisibleRenderablesByType;
      public Dictionary<string, List<Renderable>> visibleRenderablesByType { get { return myVisibleRenderablesByType; } }
      

      public delegate void PassFunction(Pass pass);
      public event PassFunction onPrePrepare;
      public event PassFunction onPostPrepare;
      public event PassFunction onPreGenerateCommands;
      public event PassFunction onPostGenerateCommands;

      public RenderCommandList preCommands;
      public RenderCommandList postCommands;

      public PassStats stats = new PassStats();

      public Pass(string name, string technique)
      {
         myName = name;
         myTechnique = technique;
         myRenderTarget = null;
         clearTarget = false;
         myRenderQueues = new Dictionary<ulong, BaseRenderQueue>();
         myVisibleRenderablesByType = new Dictionary<string, List<Renderable>>();
         preCommands = new RenderCommandList();
         postCommands = new RenderCommandList();
      }

      public string name { get { return myName; } }
      public string technique { get { return myTechnique; } }

      public void setRenderTarget(RenderTarget rt)
      {
         myRenderTarget = rt;
      }

      public void updateVisibleRenderables(IEnumerable<Renderable> cameraVisibles)
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

      public void prepare()
      {
         if (onPrePrepare != null)
         {
            onPrePrepare(this);
         }

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

         if (onPostPrepare != null)
         {
            onPostPrepare(this);
         }
      }

      public void generateRenderCommandLists()
      {
         preCommands.Clear();
         postCommands.Clear();

         if (myRenderTarget != null)
         {
            preCommands.Add(new SetRenderTargetCommand(myRenderTarget));
            if (clearTarget == true)
            {
               preCommands.Add(new ClearCommand());
            }
         }

         //called after setting render target so that any user commands inserted will affect (or change) the render target
         if (onPreGenerateCommands != null)
         {
            onPreGenerateCommands(this);
         }

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

         if (onPostGenerateCommands != null)
         {
            onPostGenerateCommands(this);
         }
      }

      public void getRenderCommands(List<RenderCommandList> renderCmdLists)
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

      public void registerQueue(BaseRenderQueue rq)
      {
         if (myRenderQueues.ContainsKey(rq.myPipeline.id) == false)
            myRenderQueues[rq.myPipeline.id] = rq;
      }
   }
}