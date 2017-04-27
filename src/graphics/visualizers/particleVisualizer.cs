using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;


using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using Util;

namespace Graphics
{
   public class ParticleVisualizer : Visualizer
   {
      public ParticleVisualizer() : base()
      {
         myType = "particle";
      }

		#region extract phase
		//public override void onFrameBeginExtract() { }
		//public override void extractPerFrame(Renderable r) { }
		//public override void extractPerView(Renderable r, View v) { }
		//public override void extractPerViewFinalize(RenderQueue q, View v) { }
		//public override void onFrameExtractFinalize() { }
		#endregion

		#region prepare phase
		//public override void onFrameBeginPrepare() { }
		//public override void preparePerFrame(Renderable r) { }
		//public override void preparePerView(RenderInfo info, View v) { }
		//public override void preparePerViewFinalize(RenderQueue q, View v) { }
		//public override void onFramePrepareFinalize() { } 
		#endregion

		#region submit phase
		//public override void onSubmitNodeBlockBegin(RenderQueue q) { }
		//public override void submitRenderInfo(RenderInfo r, RenderQueue q) { }
		//public override void onSubmitNodeBlockEnd(RenderQueue q) { }
		#endregion
	}
}