using System;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using Graphics;
using Terrain;
using Util;
using Engine;
using GUI;

namespace Editor
{
   public class SelectionManager
   {
      Editor myEditor;
      List<NodeLocation> mySelectedNodes = new List<NodeLocation>();
      Texture mySelectTexture;

      bool myMultiSelect = false;

      NodeHit myCurrentHit;
      NodeLocation myClampedLocation;

      bool myDebug = false;


      public enum SelectMode { NONE, ADD, REMOVE };
      public SelectMode mySelectMode = SelectMode.NONE;

      public SelectionManager(Editor e)
      {
         myEditor = e;

         TextureDescriptor td = new TextureDescriptor("../data/textures/circle.png");
         mySelectTexture = Renderer.resourceManager.getResource(td) as Texture;
      }

      public void injectRenderCmds(Pass pass)
      {
         if (myEditor.active == true)
         {
            if (myMultiSelect == true)
            {
               foreach (NodeLocation n in mySelectedNodes)
               {
                  StatelessRenderCommand cmd = new RenderWireframeCubeCommand(n.min(), n.min() +  new Vector3(n.size() + 0.001f), Color4.Blue);
						cmd.pipelineState.culling.enabled = false;
						cmd.pipelineState.depthTest.enabled = true;
                  cmd.pipelineState.depthTest.depthFunc = DepthFunction.Lequal;
						cmd.pipelineState.generateId();
						cmd.renderState.setUniformBuffer(myEditor.camera.uniformBufferId(), 0);
                  pass.postCommands.Add(cmd);
               }
            }

            if (myCurrentHit != null)
            {
               switch (myEditor.activeMode)
               {
                  case "Block Mode":
                     {
								StatelessRenderCommand cmd = new RenderWireframeCubeCommand(myClampedLocation.worldLocation(), myClampedLocation.worldLocation() + new Vector3(WorldParameters.sizeAtDepth(myEditor.cursorDepth) + 0.001f), Color4.OrangeRed);
                        cmd.pipelineState.culling.enabled = false;
                        cmd.pipelineState.depthTest.enabled = true;
								cmd.pipelineState.depthTest.depthFunc = DepthFunction.Lequal;
                        cmd.pipelineState.generateId();
								cmd.renderState.setUniformBuffer(myEditor.camera.uniformBufferId(), 0);
                        pass.postCommands.Add(cmd);

                        if (myCurrentHit.face != Face.NONE)
                        {
                           Vector3[] verts = getFaceVerts(myClampedLocation, myCurrentHit.face);
                           cmd = new RenderTexturedQuadCommand(verts, mySelectTexture);
                           cmd.pipelineState.culling.enabled = false;
                           cmd.pipelineState.depthTest.enabled = false;
                           cmd.pipelineState.depthTest.depthFunc = DepthFunction.Lequal;
                           cmd.renderState.polygonOffset.enableType = PolygonOffset.EnableType.FILL;
									cmd.renderState.polygonOffset.factor = -1.0f;
									cmd.renderState.polygonOffset.units = -1.0f;
									cmd.pipelineState.generateId();
									cmd.renderState.setUniformBuffer(myEditor.camera.uniformBufferId(), 0);
                           pass.postCommands.Add(cmd);
                        }
                        break;
                     }
                  case "Edge Mode":
                     {
                        if (myCurrentHit.face != Face.NONE && myCurrentHit.edge==-1)
                        {
                           Vector3[] verts = getFaceVerts(myClampedLocation, myCurrentHit.face);
									StatelessRenderCommand cmd = new RenderTexturedQuadCommand(verts, mySelectTexture);
									cmd.pipelineState.culling.enabled = false;
									cmd.pipelineState.depthTest.enabled = false;
									cmd.pipelineState.generateId();
									cmd.renderState.setUniformBuffer(myEditor.camera.uniformBufferId(), 0);
                           pass.postCommands.Add(cmd);
                        }
                        if(myCurrentHit.edge != -1)
                        {
                           Vector3[] verts=getEdgeVerts(myClampedLocation, myCurrentHit.edge);
									StatelessRenderCommand cmd =new RenderLineCommand(verts[0], verts[1], Color4.Aquamarine);
                           cmd.pipelineState.depthTest.enabled = false;
                           cmd.pipelineState.depthWrite.enabled = false;
									cmd.pipelineState.generateId();
									cmd.renderState.setUniformBuffer(myEditor.camera.uniformBufferId(), 0);
                           pass.postCommands.Add(cmd);

                        }
                        if(myCurrentHit.vert != -1)
                        {
                           Vector3 vert = getVert(myClampedLocation, myCurrentHit.vert);
                           RenderCommand cmd = new RenderSphereCommand(vert, myClampedLocation.node.size() / 12, Color4.Red);
                           pass.postCommands.Add(cmd);
                        }
                        break;
                     }
                  case "Face Mode":
                     {
                        if (myCurrentHit.face != Face.NONE)
                        {
                           foreach (NodeLocation n in mySelectedNodes)
                           {
                              Vector3[] verts = getFaceVerts(n, myCurrentHit.face);
                              StatelessRenderCommand cmd = new RenderQuadCommand(verts, Color4.Blue);
                              cmd.pipelineState.culling.enabled = false;
                              cmd.pipelineState.depthTest.enabled = false;
										cmd.pipelineState.generateId();
										cmd.renderState.setUniformBuffer(myEditor.camera.uniformBufferId(), 0);
										cmd.renderState.wireframe.enabled = true;

                              cmd.renderState.polygonOffset.enableType = PolygonOffset.EnableType.FILL;
                              cmd.renderState.polygonOffset.factor = -1.0f;
                              cmd.renderState.polygonOffset.units = 1.0f;
										cmd.renderState.setUniformBuffer(myEditor.camera.uniformBufferId(), 0);
                              pass.postCommands.Add(cmd);
                           }

                           Vector3[] mverts = getFaceVerts(myClampedLocation, myCurrentHit.face);
                           StatelessRenderCommand mcmd = new RenderQuadCommand(mverts, Color4.LightBlue);
                           mcmd.pipelineState.culling.enabled = false;
                           mcmd.pipelineState.depthTest.enabled = false;
									mcmd.pipelineState.generateId();
									mcmd.renderState.wireframe.enabled = true;
                           mcmd.renderState.polygonOffset.enableType = PolygonOffset.EnableType.FILL;
                           mcmd.renderState.polygonOffset.factor = -1.0f;
                           mcmd.renderState.polygonOffset.units = 1.0f;
									mcmd.renderState.setUniformBuffer(myEditor.camera.uniformBufferId(), 0);
                           pass.postCommands.Add(mcmd);
                        }
                        break;
                     }
               }
            }
         }
      }

      Vector3[] getFaceVerts(NodeLocation nl, Face face)
      {
         Vector3[] fv = new Vector3[4];
         Vector3[] v = new Vector3[8];
         Vector3 chunkLoc = nl.chunk.myLocation;
         Vector3 nodeLoc = nl.node.location;
         for (int i = 0; i < 8; i++)
         {
            v[i] = chunkLoc + (nodeLoc + (Node.theChildOffsets[i] * nl.node.size()));
         }

         int[] fi = Node.faceIndices((int)face);
         for (int i = 0; i < 4; i++)
         {
            fv[i] = v[fi[i]];
         }

         return fv;
      }

      Vector3[] getEdgeVerts(NodeLocation nl, int edge)
      {
         Vector3[] ev = new Vector3[2];
         Vector3[] v = new Vector3[8];
         Vector3 chunkLoc = nl.chunk.myLocation;
         Vector3 nodeLoc = nl.node.location;
         for (int i = 0; i < 8; i++)
         {
            v[i] = chunkLoc + (nodeLoc + (Node.theChildOffsets[i] * nl.node.size()));
         }

         int[] fi = Node.edgeIndices(edge);
         for (int i = 0; i < 2; i++)
         {
            ev[i] = v[fi[i]];
         }

         return ev;
      }

      Vector3 getVert(NodeLocation nl, int vert)
      {
         Vector3 chunkLoc = nl.chunk.myLocation;
         Vector3 nodeLoc = nl.node.location;

         return chunkLoc + (nodeLoc + (Node.theChildOffsets[vert] * nl.node.size()));
      }

      public void onGui()
      {
         if (myDebug)
         {
            UI.beginWindow("Selection Manager");
            UI.setWindowPosition(new Vector2(10, 10), SetCondition.FirstUseEver);
            UI.setWindowSize(new Vector2(400, 300), SetCondition.FirstUseEver);
            UI.label("Hit: {0}", myCurrentHit == null ? Vector3.Zero : myCurrentHit.location);
            UI.label("Depth: {0}", myEditor.cursorDepth);
            UI.label("hit node: {0}", myCurrentHit == null ? Vector3.Zero : myCurrentHit.node.location.worldLocation());
            UI.label("Node Key: {0}", myCurrentHit == null ? 0 : myCurrentHit.node.myKey.myValue);
            UI.label("Clamped Node Key: {0}", myCurrentHit == null ? 0 : myClampedLocation.node.myValue);
            UI.label("clamped hit: {0}", myCurrentHit == null ? Vector3.Zero : myClampedLocation.node.location);
            UI.label("Face: {0}", myCurrentHit == null ? Face.NONE : myCurrentHit.face);
            UI.label("Edge: {0}", myCurrentHit == null ? -1 : myCurrentHit.edge);
            UI.label("Vertex: {0}", myCurrentHit == null ? -1 : myCurrentHit.vert);
            UI.label("Visible Faces {0}", myCurrentHit == null ? "None" : Node.visiblityFlagsString(myCurrentHit.node.myFaceVisibilty));
            UI.label("Multiselect: {0}", myMultiSelect);
            UI.label("Selected Cubes: {0}", mySelectedNodes.Count);
            UI.endWindow();
         }

         handleInput();

         //update context with relevant data
         myEditor.context.currentSelectionDepth = myEditor.cursorDepth;
         myEditor.context.selectedNodes = mySelectedNodes;

         if (myCurrentHit != null)
         {
            myEditor.context.currentLocation = myClampedLocation;
            myEditor.context.currentNode = myCurrentHit.node;
            myEditor.context.previousFace = myEditor.context.currentFace;
            myEditor.context.currentFace = myCurrentHit.face;
            myEditor.context.currentEdge = myCurrentHit.edge;
            myEditor.context.currentVert = myCurrentHit.vert;
         }
         else
         {
            myEditor.context.currentNode = null;
            myEditor.context.currentFace = Face.NONE;
            myEditor.context.currentEdge = -1;
            myEditor.context.currentVert = -1;
         }
      }

      #region event handlers
      public void handleInput()
      {
         if (UI.hoveredWindow == null || UI.hoveredWindow.name == "root")
         {
            handleMouseMove();
            handleButtonDown();
            handleButtonUp();
            handleKeyDown();
            handleKeyUp();
         }
      }

      public void handleMouseMove()
      {
         int x, y;
         x = (int)UI.mouse.pos.X;
         y = (int)UI.mouse.pos.Y;

         myClampedLocation = getMouseOverLocation(x, y);
         switch (mySelectMode)
         {
            case SelectMode.NONE:
               {
               }
               break;
            case SelectMode.ADD:
               {
                  addCurrentNode();
               }
               break;
            case SelectMode.REMOVE:
               {
                  removeCurrentNode();
               }
               break;
         }
      }

      public void handleButtonDown()
      {
         if (UI.mouse.isButtonClicked(MouseButton.Left) == true)
         {
            if (myMultiSelect == false)
            {
               mySelectedNodes.Clear();
            }

            mySelectMode = SelectMode.ADD;
         }

         if (UI.mouse.isButtonClicked(MouseButton.Middle) == true)
         {
            mySelectMode = SelectMode.REMOVE;
         }

         if(UI.mouse.isButtonClicked(MouseButton.Right)== true)
         {
            if (myCurrentHit != null)
            {
               myCurrentHit.node.updateVisibility();
            }
         }
      }

      public void handleButtonUp()
      {
         if (UI.mouse.buttons[(int)MouseButton.Left].released == true)
         {
            addCurrentNode();
            mySelectMode = SelectMode.NONE;
         }
         if (UI.mouse.buttons[(int)MouseButton.Middle].released == true)
         {
            removeCurrentNode();
            mySelectMode = SelectMode.NONE;
         }
      }

      public void handleKeyDown()
      {
         if (UI.keyboard.keyPressed(Key.ShiftLeft) == true)
         {
            myMultiSelect = true;
         }
      }

      public void handleKeyUp()
      {
         //cancel selection
         if(UI.keyboard.keyReleased(Key.Space)==true)
         {
            mySelectedNodes.Clear();
         }

         if(UI.keyboard.keyReleased(Key.ShiftLeft)==true)
         {
            myMultiSelect = false;
         }
               
         if(UI.keyboard.keyReleased(Key.Plus)==true)
         {
            myEditor.cursorDepth++;
            if (myEditor.cursorDepth > WorldParameters.theMaxDepth)
            {
               myEditor.cursorDepth = WorldParameters.theMaxDepth;
            }
         }

         if(UI.keyboard.keyReleased(Key.Minus)==true)
         {
            myEditor.cursorDepth--;
            if (myEditor.cursorDepth < 0)
            {
               myEditor.cursorDepth = 0;
            }
         }

         if(UI.keyboard.keyReleased(Key.F12) == true)
         {
            myDebug = !myDebug;
         }
      }
      #endregion

      #region actions
      private void removeCurrentNode()
      {
         if (myCurrentHit != null)
         {
            if (mySelectedNodes.Contains(myClampedLocation) == true)
            {
               mySelectedNodes.Remove(myClampedLocation);
            }
         }
      }

      private void addCurrentNode()
      {
         if (myCurrentHit != null)
         {
            //in face mode, clear the selection if the face orientation changes
            if(myEditor.activeMode == "Face Mode")
            {
               if(myEditor.context.currentFace != myEditor.context.previousFace)
               {
                  mySelectedNodes.Clear();
               }
            }

            if (mySelectedNodes.Contains(myClampedLocation) == false)
            {
               mySelectedNodes.Add(myClampedLocation);
            }
         }
      }

      public NodeLocation getMouseOverLocation(int x, int y)
      {
         Ray r = myEditor.camera.getPickRay(x, (int)UI.displaySize.Y - y);
         myCurrentHit = myEditor.world.getNodeIntersection(r, myEditor.camera.near, myEditor.camera.far);
         myClampedLocation = null;

         if (myCurrentHit != null)
         {
            if (myEditor.cursorDepth <= myCurrentHit.node.depth)
            {
               Node n = myCurrentHit.node;
               myClampedLocation = myCurrentHit.node.location;
               while (n.depth != myEditor.cursorDepth)
               {
                  n = n.myParent;
                  myClampedLocation = n.location;
               }
            }
            else
            {
					myClampedLocation = new NodeLocation(myCurrentHit.location, myEditor.cursorDepth);

               //may need to adjust the clamped location to be within the node that is hit
               //since sometimes the conversion to a clamped node location causes it to be the next node since
               //nodes boundaries are [min..max) inclusion
               if (myCurrentHit.node.contains(myClampedLocation) == false)
               {
                  foreach (Face f in Enum.GetValues(typeof(Face)))
                  {
                     NodeLocation nl = myClampedLocation.getNeighborLocation(f);
                     if (myCurrentHit.node.contains(nl) == true)
                     {
                        myClampedLocation = nl;
                        break;
                     }
                  }
               }
            }
         }

         return myClampedLocation;
      }
      #endregion
   }
}