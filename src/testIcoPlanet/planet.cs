using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using Graphics;
using Util;
using GpuNoise;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Planet
{
   public struct TriId
   {
      public UInt64 myId; //all aspects of the triangle are stored in this 64 bit number

      //top 5 bits define which root triangle this triangle is derived from
      public Byte root
      {
         get
         { 
            //top 5 bits
            UInt64 temp = myId & 0xF800000000000000;
            temp = temp >> 59; //move 5 root bits down
            return (Byte)temp;
         }
         set
         {
            UInt64 temp = value;
            temp = temp << 59;
            myId = myId & 0x7FFFFFFFFFFFFFF; //keep all but the root bits
            myId = myId | temp; //mix in the new value
         }
      }

      //should never be a depth 0, thats a root.  depth gives a sense of the size of the triangle
      //2nd 5 bits
      public Byte depth
      {
         get
         {
            UInt64 temp = myId & 0x7C0000000000000;
            temp = temp >> 54; //move 5 depth bits down
            temp = temp & 0x1F; //only the bottom 5 bits, top 5 are the root
            return (Byte)temp;
         }
         set
         {
            UInt64 temp = value;
            temp = temp << 54;
            myId = myId & 0xF83FFFFFFFFFFFFF; //keep all but the depth bits
            myId = myId | temp; //mix in the new value
         }
      }

      public UInt64 id
      {
         get { return myId & 0x3FFFFFFFFFFFFF; }
         set
         {
            UInt64 temp = value & 0x3FFFFFFFFFFFFF; //make sure the index is only the bottom 54 bits
            myId = myId & 0xFFC0000000000000; //keep the root/depth bits
            myId = myId | temp; //mix in the index
         }
      }

      public Byte index
      {
         get
         {
            UInt64 temp = id;
            temp = temp >> 54 - (depth * 2);
            temp = temp & 0x3; //only the bottom 2 bits
            return (Byte)temp;
         }
         set
         {
            UInt64 temp = (UInt64)value & 0x3; //only the bottom 2 bits;
            temp = temp << 54 - (depth * 2);

            UInt64 mask = (UInt64)0x3 << 54 - (depth * 2); //create a mask for the id bits
            myId = myId & ~mask; //invert the mask and 
            myId = myId | temp; //mix in the id bits

         }
      }
   }

   [StructLayout(LayoutKind.Sequential)]
   public struct V3F1
   {
      static int theStride = Marshal.SizeOf(default(V3F1));

      public Vector3d Position;
      public float Depth;

      public static int stride { get { return theStride; } }

      static Dictionary<string, BufferBinding> theBindings = null;
      public static Dictionary<string, BufferBinding> bindings()
      {
         if(theBindings == null)
         {
            theBindings = new Dictionary<string, BufferBinding>();
            theBindings["position"] = new BufferBinding() { bufferIndex = 0, dataType = BindingDataType.Double, dataFormat = (int)VertexAttribType.Double, normalize = false, numElements = 3, offset = 0 };
            theBindings["depth"] = new BufferBinding() { bufferIndex = 0, dataType = BindingDataType.Float, dataFormat = (int)VertexAttribType.Float, normalize = false, numElements = 1, offset = 24 };
         }

         return theBindings;
      }
   }

   public class Tri
   {
      public TriId id;
      public uint i1, i2, i3; //indexes into the vertex array
      public float priority;
      public Tri n1, n2, n3; //neighbors
      public Tri c1, c2, c3, c4; //children
      public Tri parent;
      public bool backfacing;
   }

   public class Planet
   {
      static int MAX_VERTEX = 200000; 
      static int MAX_INDEX = MAX_VERTEX * 3;
      static int MAX_TRI = 1000000;

      public uint myVertCount;
      public uint myIndexCount;

      V3F1[] myVerts = new V3F1[MAX_VERTEX];
      uint[] myIndex = new uint[MAX_INDEX];

      //stores all the tris
      List<Tri> myTris = new List<Tri>(MAX_TRI);
      public int myNextTri = 0;
      public Queue<Tri> mySplitQueue = new Queue<Tri>();

      VertexBufferObject myVBO = new VertexBufferObject(BufferUsageHint.StreamDraw);
      IndexBufferObject myIBO = new IndexBufferObject(BufferUsageHint.StreamDraw);
      StatelessDrawElementsCommand myRenderPlanetCommand;
      StatelessDrawElementsCommand myRenderWaterCommand;

      Camera myCamera;
      Vector3 myLastCameraPosition;
      Quaternion myLastCameraOrientation;

      CubemapTexture myNoiseTexture;

      public float myFov;
      public float mySinFov;
      public float myCosFov;

      public float myErrorVal = 0.005f;

      public bool myRenderWireframe = true;

      public float myMinEdgesize = 0.04f;

      public float myMaxHeight = 5000.0f;

      public PlanetTextureManager myTextureManager = new PlanetTextureManager();
      public Texture myHeightTexture;

      public float myScale = 60000.0f;

      public Planet(Camera c, CubemapTexture elevationMap)
      {
         myCamera = c;
         myNoiseTexture = elevationMap;
         for (int i = 0; i < MAX_TRI; i++)
         {
            myTris.Add(new Tri());
         }

         myFov = MathHelper.DegreesToRadians(myCamera.fieldOfView);
         mySinFov = (float)Math.Sin(myFov);
         myCosFov = (float)Math.Cos(myFov);

         init();

         myTextureManager.init();

         //setup the shader
         List<ShaderDescriptor> desc = new List<ShaderDescriptor>();
         desc.Add(new ShaderDescriptor(ShaderType.VertexShader, "Test IcoPlanet.shaders.draw-planet-vs.glsl"));
         //desc.Add(new ShaderDescriptor(ShaderType.GeometryShader, "Test IcoPlanet.shaders.draw-planet-gs.glsl"));
         desc.Add(new ShaderDescriptor(ShaderType.FragmentShader, "Test IcoPlanet.shaders.draw-planet-ps.glsl"));
         ShaderProgramDescriptor sd = new ShaderProgramDescriptor(desc);
         ShaderProgram shader = Renderer.resourceManager.getResource(sd) as ShaderProgram;

         myRenderPlanetCommand = new StatelessDrawElementsCommand(PrimitiveType.Triangles, (int)myIndexCount, 0, IndexBufferObject.IndexBufferDatatype.UnsignedInt);
         myRenderPlanetCommand.pipelineState.shaderState.shaderProgram = shader;
         myRenderPlanetCommand.pipelineState.vaoState.vao = new VertexArrayObject();
         myRenderPlanetCommand.pipelineState.vaoState.vao.bindVertexFormat(shader, V3F1.bindings());
         myRenderPlanetCommand.pipelineState.generateId();

         desc.Clear();
         desc.Add(new ShaderDescriptor(ShaderType.VertexShader, "Test IcoPlanet.shaders.planet-water-vs.glsl"));
         desc.Add(new ShaderDescriptor(ShaderType.FragmentShader, "Test IcoPlanet.shaders.planet-water-ps.glsl"));
         sd = new ShaderProgramDescriptor(desc);
         shader = Renderer.resourceManager.getResource(sd) as ShaderProgram;

         myRenderWaterCommand = new StatelessDrawElementsCommand(PrimitiveType.Triangles, (int)myIndexCount, 0, IndexBufferObject.IndexBufferDatatype.UnsignedInt);
         myRenderWaterCommand.pipelineState.shaderState.shaderProgram = shader;
         myRenderWaterCommand.pipelineState.vaoState.vao = new VertexArrayObject();
         myRenderWaterCommand.pipelineState.vaoState.vao.bindVertexFormat(shader, V3F1.bindings());
         myRenderWaterCommand.pipelineState.blending.enabled = true;
         myRenderWaterCommand.pipelineState.generateId();


         myHeightTexture = new Texture("../data/textures/EarthLookupTable.png");
      }

      public void reset()
      {
         myLastCameraPosition = myCamera.position;
         myLastCameraOrientation = myCamera.myOrientation;
         myNextTri = 0;
         mySplitQueue.Clear();
         init();
      }

      public void init()
      {
         myLastCameraOrientation = myCamera.myOrientation;
         myLastCameraPosition = myCamera.position;

         //6 verts for the octahedron (8 sided dice)
         myVertCount = 0;
         myVerts[myVertCount].Position = new Vector3d(0.0, 1.0, 0.0) * myScale; //0-top
         myVerts[myVertCount].Depth = 0;
         myVertCount++;

         myVerts[myVertCount].Position = new Vector3d(0.0, 0.0, -1.0) * myScale; //1-front
         myVerts[myVertCount].Depth = 0;
         myVertCount++;

         myVerts[myVertCount].Position = new Vector3d(1.0, 0.0, 0.0) * myScale; //2-right
         myVerts[myVertCount].Depth = 0;
         myVertCount++;

         myVerts[myVertCount].Position = new Vector3d(0.0, 0.0, 1.0) * myScale; //3-back
         myVerts[myVertCount].Depth = 0;
         myVertCount++;

         myVerts[myVertCount].Position = new Vector3d(-1.0, 0.0, 0.0) * myScale; //4-left
         myVerts[myVertCount].Depth = 0;
         myVertCount++;

         myVerts[myVertCount].Position = new Vector3d(0.0, -1.0, 0.0) * myScale; //5-bottom
         myVerts[myVertCount].Depth = 0;
         myVertCount++;

         //8 faces
         int myIndexCount = 0;
         myIndex[myIndexCount++] = 0; myIndex[myIndexCount++] = 2; myIndex[myIndexCount++] = 1;
         myIndex[myIndexCount++] = 0; myIndex[myIndexCount++] = 3; myIndex[myIndexCount++] = 2;
         myIndex[myIndexCount++] = 0; myIndex[myIndexCount++] = 4; myIndex[myIndexCount++] = 3;
         myIndex[myIndexCount++] = 0; myIndex[myIndexCount++] = 1; myIndex[myIndexCount++] = 4;

         myIndex[myIndexCount++] = 5; myIndex[myIndexCount++] = 1; myIndex[myIndexCount++] = 2;
         myIndex[myIndexCount++] = 5; myIndex[myIndexCount++] = 2; myIndex[myIndexCount++] = 3;
         myIndex[myIndexCount++] = 5; myIndex[myIndexCount++] = 3; myIndex[myIndexCount++] = 4;
         myIndex[myIndexCount++] = 5; myIndex[myIndexCount++] = 4; myIndex[myIndexCount++] = 1;
         

         //create the triangles 
         ushort vi = 0;
         for (int i = 0; i < 8; i++)
         {
            Tri t = myTris[myNextTri++];
            t.id.myId = 0;
            t.id.root = (Byte)i;
            t.i1 = myIndex[vi++];
            t.i2 = myIndex[vi++];
            t.i3 = myIndex[vi++];
            t.c1 = t.c2 = t.c3 = t.c4 = null;
            t.priority = 1.0f;
            t.backfacing = false;
            mySplitQueue.Enqueue(t);
         }
      }
      Tri createTriangle(Tri p, Byte childIndex, uint i1, uint i2, uint i3)
      {
         Tri t = myTris[myNextTri++];
         t.parent = t;
         t.id = p.id;
         t.id.depth += 1;
         //t.id.index = childIndex;

         t.i1 = i1;
         t.i2 = i2;
         t.i3 = i3;

         t.c1 = t.c2 = t.c3 = t.c4 = null;

         t.priority = (float)distancePriority(t);

         backfacing(t);

         return t;
      }

      void subdivideTri(Tri t)
      {
         if (shouldSplit(t) == false) return;

         Vector3d v1, v2, v3;
         Vector3d t1, t2, t3;
         uint i12, i23, i31;

         V3F1 v12 = new V3F1();
         V3F1 v23 = new V3F1();
         V3F1 v31 = new V3F1();

         //get existing verts
         v1 = myVerts[t.i1].Position;
         v2 = myVerts[t.i2].Position;
         v3 = myVerts[t.i3].Position;

         //work with normalized positions
         v1.Normalize();
         v2.Normalize();
         v3.Normalize();

         //new verts between the existing verts
         t1 =  (v1 + v2) / 2;
         t2 =  (v2 + v3) / 2;
         t3 =  (v3 + v1) / 2;

         //push out to unit sphere
         t1.Normalize();
         t2.Normalize();
         t3.Normalize();

         v12.Position = t1 * myScale;
         v23.Position = t2 * myScale;
         v31.Position = t3 * myScale;

         //set the height
         v12.Depth = (float)t.id.depth; //TextureManager.heightAt(v12.Position);
         v23.Depth = (float)t.id.depth; //TextureManager.heightAt(v23.Position);
         v31.Depth = (float)t.id.depth; //TmyTextureManager.heightAt(v31.Position);

         //get the new vertex indexes
         i12 = myVertCount++;
         i23 = myVertCount++;
         i31 = myVertCount++;

         //add new verts to vert buffer at the new indexes
         myVerts[i12] = v12;
         myVerts[i23] = v23;
         myVerts[i31] = v31;

         //create new triangles
         t.c1 = createTriangle(t, 0, i12, i23, i31);
         t.c2 = createTriangle(t, 1, t.i1, i12, i31);
         t.c3 = createTriangle(t, 2, i12, t.i2, i23);
         t.c4 = createTriangle(t, 3, i23, t.i3, i31);

         mySplitQueue.Enqueue(t.c1);
         mySplitQueue.Enqueue(t.c2);
         mySplitQueue.Enqueue(t.c3);
         mySplitQueue.Enqueue(t.c4);

      }

      void addTriangleToVbo(Tri t)
      {
         if (myIndexCount + 3 > MAX_INDEX) return;
         //if (t.backfacing == true) return;

         if (t.c1 != null) addTriangleToVbo(t.c1);
         if (t.c2 != null) addTriangleToVbo(t.c2);
         if (t.c3 != null) addTriangleToVbo(t.c3);
         if (t.c4 != null) addTriangleToVbo(t.c4);

         //if there are no children add this triangle
         if (t.c1 == null && t.c2 == null && t.c3 == null && t.c4 == null)
         {
            myIndex[myIndexCount++] = t.i1;
            myIndex[myIndexCount++] = t.i2;
            myIndex[myIndexCount++] = t.i3;
         }
      }

      void updateVbo()
      {
         myIndexCount = 0;
         for (int i = 0; i < 8; i++) //addTriangleToVbo is recursive, so only update the roots
         {
            addTriangleToVbo(myTris[i]);
         }

         myVBO.setData(myVerts, (int)myVertCount);
         myIBO.setData(myIndex, (int)myIndexCount);
      }

      bool backfacing(Tri t)
      {
         t.backfacing = false;

         //is rear facing
         Vector3d v1, v2, v3;
         v1 = myVerts[t.i1].Position;
         v2 = myVerts[t.i2].Position;
         v3 = myVerts[t.i3].Position;

         Vector3d edge1 = v2 - v1;
         Vector3d edge2 = v3 - v1;
         Vector3d faceNormal = Vector3d.Cross(edge1, edge2);
                
         double angle = Vector3d.Dot((v1 - (Vector3d)myCamera.position), faceNormal);

         if (angle >= 0)
         {
            t.backfacing = true;
         }

         return t.backfacing;
      }

      bool culled(Tri t)
      {
         if (triInsideViewCone(t) == true)
         {
            return false;
         }

         return true;
      }

      bool triInsideViewCone(Tri t)
      {
         Vector3d v1, v2, v3;
         v1 = myVerts[t.i1].Position;
         v2 = myVerts[t.i2].Position;
         v3 = myVerts[t.i3].Position;

         Vector3d sphereCenter = (v1 + v2 + v3) / 3;
         double sphereRadius = (v1 - sphereCenter).Length;

         //get the camera position on the sphere surface
         Vector3d camPos = ((Vector3d)myCamera.position).Normalized() * myScale;
         Vector3d U = camPos - (sphereRadius / mySinFov) * (Vector3d)myCamera.viewVector;
         Vector3d D = sphereCenter - U;
         if (Vector3d.Dot((Vector3d)myCamera.viewVector, D) >= D.Length * myCosFov)
         {
            // center is inside K’’
            D = sphereCenter - camPos;
            if (-Vector3d.Dot((Vector3d)myCamera.viewVector, D) >= D.Length * mySinFov)
            {
               // center is inside K’’ and inside K’
               return D.Length <= sphereRadius;
            }
            else
            {
               // center is inside K’’ and outside K’
               return true;
            }
         }
         else
         {
            // center is outside K’’
            return false;
         }
      }

      bool triangleTooSmall(Tri t)
      {
         Vector3d v1, v2, v3;
         v1 = myVerts[t.i1].Position;
         v2 = myVerts[t.i2].Position;
         v3 = myVerts[t.i3].Position;

         double dist = (((v1 + v2 + v3) / 3.0f) - (Vector3d)myCamera.position).Length;

         double e1 = (v2 - v1).Length;
         double e2 = (v2 - v3).Length;
         double e3 = (v3 - v1).Length;

         return (((e1 + e2 + e3) / 3.0f) / dist) < myMinEdgesize; //avg edge length
      }

      bool elevationErrorTooLarge(Tri t)
      {
         float error = t.priority;

         if (error < myErrorVal)
         {
            return true;
         }

         return false;
      }

      double elevationError(Tri t)
      {
         Vector3d v1, v2, v3, v12, v23, v31;
         v1 = myVerts[t.i1].Position;
         v2 = myVerts[t.i2].Position;
         v3 = myVerts[t.i3].Position;

         //calculate the mid points
         v12 = (v1 + v2) / 2.0f;
         v23 = (v2 + v3) / 2.0f;
         v31 = (v3 + v1) / 2.0f;

         //heights at those points
         double h12, h23, h31;
         h12 = 1.0f + myTextureManager.heightAt(v12);
         h23 = 1.0f + myTextureManager.heightAt(v23);
         h31 = 1.0f + myTextureManager.heightAt(v31);

         //distance to those points
         double d12, d23, d31;
         d12 = (v12 - (Vector3d)myCamera.position).Length;
         d23 = (v23 - (Vector3d)myCamera.position).Length;
         d31 = (v31 - (Vector3d)myCamera.position).Length;

         //error at those points
         double e12, e23, e31;
         e12 = (h12 - v12.Length) / d12;
         e23 = (h23 - v23.Length) / d23;
         e31 = (h31 - v31.Length) / d31;

         //should already be in the range of 0..1
         return (e12 + e23 + e31) / 3.0f;
      }

      public bool freezeRebuild = false;

      double distancePriority(Tri t)
      {
         Vector3d v1, v2, v3;
         v1 = myVerts[t.i1].Position;
         v2 = myVerts[t.i2].Position;
         v3 = myVerts[t.i3].Position;

         Vector3d triCenter = (v1 + v2 + v3) / 3.0f;

         //get the camera position on the sphere surface
         Vector3d camPos = ((Vector3d)myCamera.position).Normalized() * myScale;
         double distanceToCamera = (triCenter - camPos).Length;
         return 1.0f / distanceToCamera;  //further things get smaller priority
      }

      bool shouldSplit(Tri t)
      {
         //check if already split
         if (t.c1 != null || t.c2 != null || t.c3 != null || t.c4 != null) return false;

         //depth is too small.  the triangles are huuuuuuge, split them anyways
         if (t.id.depth < 3) return true;

         //check if it's back facing
         if (backfacing(t) == true) return false;

         //is it in the view frustum (uses a bounding sphere/cone test)
         if (culled(t) == true) return false;

         //if the triangle's size is too small to be seen, don't split it
         if (triangleTooSmall(t) == true) return false;

         //can't split any more than this anyways
         if (t.id.depth >= 27) return false;

         return true;
      }

      public bool shouldKeepTesselating(double start)
      {
         //check time
         if (TimeSource.currentTime() - start >= 0.033) //more than 30hz to update
            return false;

         //check tri count
         if ((myVertCount + 3) > MAX_VERTEX) return false;
         if ((myNextTri + 3) > MAX_TRI) return false;

         if (mySplitQueue.Count == 0) return false;

         return true;
      }

      public void update()
      {
         if (freezeRebuild == true)
            return;

         if (needsRebuild())
         {
            reset();
         }

         double start = TimeSource.currentTime();
         bool vboChanged = false;
         while (shouldKeepTesselating(start) == true)
         {
            Tri t = mySplitQueue.Dequeue();
            if (t != null)
            {
               subdivideTri(t);
               vboChanged = true;
            }
            else
            {
               break;
            }
         }

         if (vboChanged)
         {
            updateVbo();
         }
      }

      public bool needsRebuild()
      {
         if ((myCamera.position - myLastCameraPosition).Length > 1.0f) return true;
         if ((myCamera.myOrientation - myLastCameraOrientation).Length > 0.1f) return true;
         return false;
      }

      public void render()
      {
         myRenderPlanetCommand.renderState.reset();

         myRenderPlanetCommand.renderState.setVertexBuffer(myVBO.id, 0, 0, V3F1.stride);
         myRenderPlanetCommand.renderState.setIndexBuffer(myIBO.id);
         myRenderPlanetCommand.renderState.setUniform(new UniformData(0, Uniform.UniformType.Float, myMaxHeight));
         myRenderPlanetCommand.renderState.setUniform(new UniformData(1, Uniform.UniformType.Int, 0));
         myRenderPlanetCommand.renderState.setTexture(myNoiseTexture.id(), 0, TextureTarget.TextureCubeMap);
         myRenderPlanetCommand.renderState.setUniformBuffer(myCamera.uniformBufferId(), 0);

         myRenderPlanetCommand.renderState.setUniform(new UniformData(20, Uniform.UniformType.Int, 1));
         myRenderPlanetCommand.renderState.setTexture(myHeightTexture.id(), 1, TextureTarget.Texture2D);

         myRenderPlanetCommand.renderState.wireframe.enabled = false;
         myRenderPlanetCommand.myCount = (int)myIndexCount;
         myRenderPlanetCommand.renderState.setUniform(new UniformData(22, Uniform.UniformType.Bool, false));
         myRenderPlanetCommand.execute();

         if (myRenderWireframe == true)
         {
            myRenderPlanetCommand.renderState.wireframe.enabled = true;
            myRenderPlanetCommand.renderState.polygonOffset.enableType = PolygonOffset.EnableType.LINE;
            myRenderPlanetCommand.renderState.polygonOffset.factor = -1.0f;
            myRenderPlanetCommand.renderState.polygonOffset.units = -1.0f;
            myRenderPlanetCommand.renderState.setUniform(new UniformData(21, Uniform.UniformType.Color4, Color4.Black));
            myRenderPlanetCommand.renderState.setUniform(new UniformData(22, Uniform.UniformType.Bool, true));
            myRenderPlanetCommand.execute();
         }

         //draw the water using the same triangles, but without any elevation change
         myRenderWaterCommand.renderState.reset();
         myRenderWaterCommand.renderState.setVertexBuffer(myVBO.id, 0, 0, V3F1.stride);
         myRenderWaterCommand.renderState.setIndexBuffer(myIBO.id);
         myRenderWaterCommand.renderState.setUniformBuffer(myCamera.uniformBufferId(), 0);
         myRenderWaterCommand.myCount = (int)myIndexCount;
         myRenderWaterCommand.execute();
      }
   }
}
