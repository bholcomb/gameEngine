using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;


using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using Util;

namespace Graphics
{
   public class SceneGraph
   {
		public bool isActive { get; set; }
      List<RenderStage> myRenderStages;

      public List<RenderStage> renderStages { get { return myRenderStages; } }

      public SceneGraph()
      {
			isActive = true;
         myRenderStages = new List<RenderStage>();
      }

      public void init(InitTable config)
      {

      }

		public RenderStage findStage(String name)
		{
			foreach(RenderStage rs in myRenderStages)
			{
				if (rs.name == name)
					return rs;
			}

			Warn.print("Failed to find renderstage {0} in scene", name);
			return null;
		}
   }
}