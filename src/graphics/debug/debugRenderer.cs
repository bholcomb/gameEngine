using System;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using Util;

namespace Graphics
{
   public static class DebugRenderer
   {
      static List<DebugRenderCommand> myCommands = new List<DebugRenderCommand>();
      static bool myIsEnabled;
      static double myLastRenderTime;
      static DebugCanvas myCanvas;

      static DebugRenderer()
      {
         myLastRenderTime = TimeSource.currentTime();
#if DEBUG
         myIsEnabled = true;
#else
         myIsEnabled=false;
#endif
      }

      public static DebugCanvas canvas { get { return myCanvas; } }

      public static void init()
      {
         myCanvas = new DebugCanvas();
         myCanvas.init();
      }

      public static bool enabled
      {
         get {return myIsEnabled;}
         set {myIsEnabled=value;}
      }

      public static void update()
      {
         myCanvas.reset();

         if (myIsEnabled == false)
            return;

         List<DebugRenderCommand> toRemove = new List<DebugRenderCommand>();

         double delta = TimeSource.currentTime() - myLastRenderTime;
         myLastRenderTime = TimeSource.currentTime();

         foreach(DebugRenderCommand rc in myCommands)
         {
            rc.execute();

            rc.reduceTime(delta);
            if (rc.ended() == true)
            {
               toRemove.Add(rc);
            }
         }

         foreach (DebugRenderCommand rc in toRemove)
         {
            myCommands.Remove(rc);
         }

         myCanvas.updateBuffers();
      }

      public static void addCube(Vector3 pos, float size, Color4 color, Fill fill, bool clip, double time)
      {
         if (myIsEnabled == false) return;
			DebugRenderCubeCommand rc = new DebugRenderCubeCommand(pos, size, color, fill, clip, time);
         myCommands.Add(rc);
      }

      public static void addOffsetCube(Vector3 pos, float size, Color4 color, Fill fill, bool clip, double time)
      {
         if (myIsEnabled == false) return;
         DebugRenderOffsetCubeCommand rc = new DebugRenderOffsetCubeCommand(pos, size, color, fill, clip, time);
         myCommands.Add(rc);
      }

      public static void addSphere(Vector3 pos, float radius, Color4 color, Fill fill, bool clip, double time)
      {
         if (myIsEnabled == false) return;
         DebugRenderSphereCommand rc = new DebugRenderSphereCommand(pos, radius, color, fill, clip, time);
         myCommands.Add(rc);
      }

      public static void addLine(Vector3 p1, Vector3 p2, Color4 color, Fill fill, bool clip, double time)
      {
         if (myIsEnabled == false) return;
         DebugRenderLineCommand rc = new DebugRenderLineCommand(p1, p2, color, fill, clip, time);
         myCommands.Add(rc);
      }

		public static void addText(float x, float y, String text, Color4 color, double time)
		{
			if (myIsEnabled == false) return;
			DebugRenderText2dCommand rc = new DebugRenderText2dCommand(new Vector2(x, y), text, color, time);
			myCommands.Add(rc);
		}

		public static void addText(Vector2 pos, String text, Color4 color, double time)
		{
			if (myIsEnabled == false) return;
			DebugRenderText2dCommand rc = new DebugRenderText2dCommand(pos, text, color, time);
			myCommands.Add(rc);
		}

		public static void addTexture(Vector2 min, Vector2 max, Texture t, bool isDepth, double time)
		{
			if (myIsEnabled == false) return;
			DebugRenderTexture2DCommand rc = new DebugRenderTexture2DCommand(min, max, t, isDepth, time);
			myCommands.Add(rc);
		}

		public static void addTexture(Vector2 min, Vector2 max, TextureBufferObject t, bool isDepth, double time)
		{
			if (myIsEnabled == false) return;
			DebugRenderTexture2DCommand rc = new DebugRenderTexture2DCommand(min, max, t, isDepth, time);
			myCommands.Add(rc);
		}

		public static void addRect2D(Rect r, Color4 c, Fill fill, bool clip, double time)
		{
			if (myIsEnabled == false) return;
			DebugRenderRect2DCommand rc = new DebugRenderRect2DCommand(r, c, fill, clip, time);
			myCommands.Add(rc);
		}

		public static void addFrustum(Matrix4 clipMatrix, Color4 color, bool clip, double time)
		{
			if (myIsEnabled == false) return;
			DebugRenderFrustumCommand rc = new DebugRenderFrustumCommand(clipMatrix, color, clip, time);
			myCommands.Add(rc);
		}


		/*
      public static void addText(Vector3 pos, Color4 color, String text, bool clip, double time)
      {
         if (myIsEnabled == false) return;
         DebugRenderTextCommand rc = new DebugRenderTextCommand(pos, text, color, clip, time);
         myCommands.Add(rc);
      }

      public static void addCone(Vector3 p1, Vector3 p2, float radius, Color4 color, bool clip, double time)
      {
         if (myIsEnabled == false) return;
         DebugRenderConeCommand rc = new DebugRenderConeCommand(p1, p2, radius, color, clip, time);
         myCommands.Add(rc);
      }

      public static void addRect(Rect r, Color4 c, bool clip, double time)
      {
         if (myIsEnabled == false) return;
         DebugRenderRectCommand rc = new DebugRenderRectCommand(r, c, clip, time);
         myCommands.Add(rc);
      }

      */
	}
}