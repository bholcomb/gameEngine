using System;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Graphics 
{
   public class RenderDebugViewCommand : RenderCommand
   {
      public RenderDebugViewCommand():base()
      {
      }

      public override void execute()
      {
			if (DebugRenderer.enabled)
			{
				List<RenderCommand> cmds = DebugRenderer.canvas.getRenderCommands();
				foreach(RenderCommand cmd in cmds)
				{
					cmd.execute();
				}
			}
      }
   }
}