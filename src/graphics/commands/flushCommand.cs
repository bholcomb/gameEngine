using System;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Graphics 
{
   public class FlushCommand : RenderCommand
   {
      public FlushCommand():base()
      {
      }

      public override void execute()
      {
         GL.Flush();
      }
   }
}