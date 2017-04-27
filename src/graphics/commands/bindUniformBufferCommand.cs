using System;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Graphics 
{
   public class BindUniformBufferCommand : RenderCommand
   {
		String myLocation;
		UniformBufferObject myUbo;

      public BindUniformBufferCommand(UniformBufferObject ubo, String location)
         : base()
      {
         myUbo = ubo;
			myLocation = location;
      }
		
      public override void execute()
      {
			myUbo.bind();
      }
   }
}