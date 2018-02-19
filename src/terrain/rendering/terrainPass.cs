using System;
using System.Collections.Generic;

using Graphics;
using Util;

namespace Terrain
{
   public class TerrainPass : Pass
   {
      TerrainRenderManager myRenderManager;

      BaseRenderQueue myOpaqueTerrainQueue;
      BaseRenderQueue myTransparentTerrainQueue;
      BaseRenderQueue myWaterTerrainQueue;

      RenderTarget myWaterRenderTarget;

      public TerrainPass(TerrainRenderManager rm) 
         : base("terrain" , "terrain")
      {
         myRenderManager = rm;
      }

      public override void registerQueue(BaseRenderQueue rq)
      {
         base.registerQueue(rq);

         if (rq.myPipeline.id == 1) myOpaqueTerrainQueue = rq;
         if (rq.myPipeline.id == 2) myTransparentTerrainQueue = rq;
         if (rq.myPipeline.id == 3) myWaterTerrainQueue = rq;
      }

      public override void getRenderCommands(List<RenderCommandList> renderCmdLists)
      {
         renderCmdLists.Add(preCommands);

         //render opaque and transparent terrain objects
         renderCmdLists.Add(myOpaqueTerrainQueue.commands);
         renderCmdLists.Add(myTransparentTerrainQueue.commands);

         if (myWaterTerrainQueue.commands.Count > 0)
         {
            //setup the water render target
            RenderCommandList w = new RenderCommandList();
            w.Add(new SetRenderTargetCommand(myWaterRenderTarget));
            renderCmdLists.Add(w);

            //render the water commands
            renderCmdLists.Add(myWaterTerrainQueue.commands);

            //setup the water post render command
            w = new RenderCommandList();
            w.Add(new SetRenderTargetCommand(renderTarget));
            

            renderCmdLists.Add(w);
         }

         renderCmdLists.Add(postCommands);
      }
   }
}