using System;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Graphics 
{
   public class MultiDrawArraysCommand : RenderCommand
   {
		PrimitiveType myPrimativeType;
      int myDrawCount;
		int[] myOffsets;
		int[] myCounts;

      public MultiDrawArraysCommand(PrimitiveType type, int[] offsets, int[] counts, int drawCount)
         : base()
      {
			myPrimativeType = type;
			myOffsets = offsets;
			myCounts = counts;
			myDrawCount = drawCount;
      }

      public override void execute()
      {
			GL.MultiDrawArrays(myPrimativeType, myOffsets, myCounts, myDrawCount);
      }
   }

	public class MultiDrawElementsCommand : RenderCommand
	{
		PrimitiveType myPrimativeType;
		int myDrawCount;
		IntPtr[] myOffsets;
		int[] myCounts;

		public MultiDrawElementsCommand(PrimitiveType type, int[] counts, IntPtr[] offsets, int drawCount)
			: base()
		{
			myPrimativeType = type;
			myCounts = counts;
			myOffsets = offsets;
			myDrawCount = drawCount;
		}

		public override void execute()
		{
			GL.MultiDrawElements(myPrimativeType, myCounts, DrawElementsType.UnsignedShort, myOffsets, myDrawCount);
		}
	}
}