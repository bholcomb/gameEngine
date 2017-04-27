using System;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Input;

using Graphics;
using Terrain;
using Util;
using UI;
using Events;
using Physics;

namespace Editor
{
   public class Editor
   {
      bool myIsActive = false;
      World myWorld;
      Camera myCamera;

      Context myContext = new Context();
      SelectionManager mySelectionManager;
      Dictionary<String, Mode> myModes = new Dictionary<String, Mode>();
      Mode myActiveMode = null;

      public Editor(World w, Camera c)
      {
         myWorld = w;
         myCamera = c;
         cursorDepth = 5;

         addMode(new BlockMode(this));
         addMode(new FaceMode(this));
         addMode(new EdgeMode(this));
         addMode(new MaterialMode(this));
         activateMode("Block Mode");

			RenderStage rs = Renderer.scenes["main"].findStage("terrain");
			rs.onPreExecute += new RenderStage.RenderStageFunction(injectEditorRenderCmds);
			context.currentMaterial = "dirt";

         mySelectionManager = new SelectionManager(this);
      }

      public bool active { get { return myIsActive; } }

      void addMode(Mode m)
      {
         myModes[m.name] = m;
      }

      void activateMode(String name)
      {
         Mode m = null;
         if (myModes.TryGetValue(name, out m) == true)
         {
            myActiveMode = m;
         }
         else
         {
            Warn.print("Failed to find mode {0}", name);
         }
      }

      void injectEditorRenderCmds(RenderStage stage)
      {
         if (myIsActive == true)
         {
//             foreach (Chunk tc in myTerrainRenderer.visibleChunks)
//             {
//                stage.postStageCommands.AddRange(myTerrainRenderer.renderManager.renderChunkBounds(tc));
// 
//                if (context.currentNode != null && tc == context.currentNode.myChunk)
//                {
//                   //List<RenderCommand> cmds = new List<RenderCommand>();
//                   //myTerrainRenderer.renderManager.renderNodeOutlines(ref cmds, tc.myRoot);
//                   //stage.myCommands.AddRange(cmds);
//                }
//             }
         } 
      }

      public Context context { get { return myContext; } }
      public World world { get { return myWorld; } }
      public Camera camera { get { return myCamera; } }
      public String activeMode { get { return myActiveMode.name; } }
      public int cursorDepth {get; set;}

      public void toggleEnabled()
      {
         myIsActive = !myIsActive;
      }

      public void onGui()
      {
         if (myIsActive == false)
            return;

         ImGui.label(String.Format("Active Mode: {0}", activeMode), ImGui.width / 2 - 100, 10);
         ImGui.label(String.Format("Undo Memory Usage: {0}", Formatter.bytesHumanReadable(myWorld.undoUsage), ImGui.width / 2 - 100, 35));

         mySelectionManager.onGui();

         if (ImGui.keyboard.keyReleased(Key.F1))
         {
            activateMode("Block Mode");
         }

         if (ImGui.keyboard.keyReleased(Key.F2))
         {
            activateMode("Edge Mode");
         }

         if (ImGui.keyboard.keyReleased(Key.F3))
         {
            activateMode("Face Mode");
         }

         if (ImGui.keyboard.keyReleased(Key.F4))
         {
            activateMode("Material Mode");
         }

         if (ImGui.keyboard.keyReleased(Key.F5))
         {
            if (ImGui.keyboard.keyPressed(Key.ControlLeft))
            {
               if (ImGui.keyboard.keyPressed(Key.ShiftLeft))
                  world.reset();
//                else
//                   myTerrainRenderer.renderManager.clear();
            }
            else
               world.regenCurrentChunk();
         }

         if (ImGui.keyboard.keyReleased(Key.Z) && ImGui.keyboard.keyPressed(Key.ControlLeft))
         {
            world.undoLastCommand();
         }

         if (myActiveMode != null)
         {
            myActiveMode.onGui();
         }
      }
   }
}