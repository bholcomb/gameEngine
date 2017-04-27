using System;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Graphics 
{
   public class BindCameraCommand : RenderCommand
   {
      public BindCameraCommand(Camera c)
         : base()
      {
         camera = c;   
      }

      public Camera camera { get; set; }

      public override void execute()
      {
         camera.bind();
      }
   }

	public class SetViewportCommand : RenderCommand
	{
		Viewport myViewport;

		public SetViewportCommand(Viewport v)
			: base()
		{
			myViewport = v;
		}

		public override void execute()
		{
			myViewport.apply();
		}
	}
}