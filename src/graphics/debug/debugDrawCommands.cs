using System;
using System.Collections;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using Util;

namespace Graphics
{
	public abstract class DebugRenderCommand
	{
		protected double myTime;
		protected bool myClip;

		public DebugRenderCommand(bool clip, double time)
		{
			myClip = clip;
			myTime = time;
		}

		public abstract void execute();

		public void reduceTime(double delta)
		{
			myTime -= delta;
		}

		public bool ended()
		{
			if (myTime <= 0.0)
			{
				return true;
			}

			return false;
		}
	}

	public class DebugRenderLineCommand : DebugRenderCommand
	{
		Vector3 myStart;
		Vector3 myEnd;
		Color4 myColor;
		Fill myFill;

		public DebugRenderLineCommand(Vector3 p1, Vector3 p2, Color4 color, Fill fill, bool clip, double time)
			: base(clip, time)
		{
			myStart = p1;
			myEnd = p2;
			myColor = color;
			myFill = fill;
		}

		public override void execute()
		{
			DebugRenderer.canvas.addLine(myStart, myEnd, myColor, myFill, myClip);
		}
	}

	public class DebugRenderLine2DCommand : DebugRenderCommand
	{
		Vector2 myStart;
		Vector2 myEnd;
		Color4 myColor;
		Fill myFill;

		public DebugRenderLine2DCommand(Vector2 p1, Vector2 p2, Color4 color, Fill fill, bool clip, double time)
			: base(clip, time)
		{
			myStart = p1;
			myEnd = p2;
			myColor = color;
			myFill = fill;
		}

		public override void execute()
		{
			DebugRenderer.canvas.addLine(myStart, myEnd, myColor, myFill, myClip);
		}
	}

	public class DebugRenderOffsetCubeCommand : DebugRenderCommand
	{
		Vector3 myMin;
		Vector3 myMax;
		Color4 myColor;
		Fill myFill;

		public DebugRenderOffsetCubeCommand(Vector3 pos, float size, Color4 color, Fill fill, bool clip, double time)
			: base(clip, time)
		{
			myMin = pos;
			myMax = pos + new Vector3(size);
			myColor = color;
			myFill = fill;
		}

		public override void execute()
		{
			DebugRenderer.canvas.addCube(myMin, myMax, myColor, myFill, myClip);
		}
	}

	public class DebugRenderCubeCommand : DebugRenderCommand
	{
		Vector3 myMin;
		Vector3 myMax;
		Color4 myColor;
		Fill myFill;

		public DebugRenderCubeCommand(Vector3 pos, float size, Color4 color, Fill fill, bool clip, double time)
			: base(clip, time)
		{
			myMin = pos - new Vector3(size / 2);
			myMax = pos + new Vector3(size / 2);
			myColor = color;
			myFill = fill;
		}

		public override void execute()
		{
			DebugRenderer.canvas.addCube(myMin, myMax, myColor, myFill, myClip);
		}
	}

	public class DebugRenderSphereCommand : DebugRenderCommand
	{
		Vector3 myPos;
		float mySize;
		Color4 myColor;
		Fill myFill;

		public DebugRenderSphereCommand(Vector3 pos, float radius, Color4 color, Fill fill, bool clip, double time)
			: base(clip, time)
		{
			myPos = pos;
			mySize = radius;
			myColor = color;
			myFill = fill;
		}

		public override void execute()
		{
			DebugRenderer.canvas.addSphere(myPos, mySize, myColor, myFill, myClip);
		}
	}

	public class DebugRenderRect2DCommand : DebugRenderCommand
	{
		Vector2 myStart;
		Vector2 myEnd;
		Color4 myColor;
		Fill myFill;

		public DebugRenderRect2DCommand(Rect r, Color4 c, Fill fill, bool clip, double time)
			: base(clip, time)
		{
			myStart = r.SW;
			myEnd = r.NE;
			myColor = c;
			myFill = fill;
		}

		public DebugRenderRect2DCommand(Vector2 start, Vector2 end, Color4 c, Fill fill, bool clip, double time)
			: base(clip, time)
		{
			myStart = start;
			myEnd = end;
			myColor = c;
			myFill = fill;
		}

		public override void execute()
		{
			DebugRenderer.canvas.addRect2d(myStart, myEnd, myColor, myFill, myClip);
		}
	}

	public class DebugRenderTexture2DCommand : DebugRenderCommand
	{
		Texture myTexture;
		TextureBufferObject myTextureBuffer;
		TextureTarget myTextureType;
		int myLayer;
		Vector2 myMin;
		Vector2 myMax;
		bool myLinearizeDepth;

		public DebugRenderTexture2DCommand(Vector2 min, Vector2 max, Texture t, bool isDepth, double time)
			: base(false, time)
		{
			myMin = min;
			myMax = max;
			myTexture = t;
			myLayer = 0;
			myTextureType = t.target;
			myLinearizeDepth = isDepth;
		}

		public DebugRenderTexture2DCommand(Vector2 min, Vector2 max, TextureBufferObject t, bool isDepth, double time)
			: base(false, time)
		{
			myMin = min;
			myMax = max;
			myTextureBuffer = t;
			myLayer = 0;
			myTextureType = TextureTarget.TextureBuffer;
			myLinearizeDepth = isDepth;
		}

		public override void execute()
		{
			if (myTexture != null)
				DebugRenderer.canvas.addTexture2d(myMin, myMax, myTexture, myLayer, myLinearizeDepth);
			else
				DebugRenderer.canvas.addTexture2d(myMax, myMax, myTextureBuffer, myLinearizeDepth);
		}
	}

	public class DebugRenderText2dCommand : DebugRenderCommand
	{
		Vector2 myPosition;
		String myText;
		Color4 myColor;

		public DebugRenderText2dCommand(Vector2 pos, String text, Color4 color, double time)
			: base(false, time)
		{
			myPosition = pos;
			myText = text;
			myColor = color;
		}

		public DebugRenderText2dCommand(float x, float y, String text, Color4 color, double time)
			: base(false, time)
		{
			myPosition = new Vector2(x, y);
			myText = text;
			myColor = color;
		}

		public override void execute()
		{
			DebugRenderer.canvas.addText2d(myPosition, myText, myColor);
		}
	}

	public class DebugRenderFrustumCommand : DebugRenderCommand
	{
		Matrix4 myInvClipMatrix;
		Color4 myColor;

		public DebugRenderFrustumCommand(Matrix4 clipMatrix, Color4 color, bool clip, double time)
			: base(clip, time)
		{
			myInvClipMatrix = clipMatrix.Inverted();
			myColor = color;
		}

		public override void execute()
		{
			DebugRenderer.canvas.addFrustum(myInvClipMatrix, myColor, myClip);
		}
	}
	/*
  

   public class DebugRenderConeCommand : DebugRenderCommand
   {
      public DebugRenderConeCommand(Vector3 p1, Vector3 p2, float thetaDegrees, Color4 color, bool clip, double time)
         : base(clip, time)
      {
//          myRc = new RenderConeCommand(p1, p2, thetaDegrees, color);
//          myRc.renderState.depthTest.depthFunc = DepthFunction.Lequal;
      }
   }
   
  

   public class DebugRenderRectCommand : DebugRenderCommand
   {
      public DebugRenderRectCommand(Rect r, Color4 c, bool clip, double time)
         : base(clip, time)
      {
         myRc = new RenderRectCommand(r, c);
      }
   }

   */
}