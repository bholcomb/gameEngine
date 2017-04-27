using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;


using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using Util;

namespace Graphics
{
	public abstract class Effect
	{
		public ShaderProgram myShader;
		public Material.Feature myFeatures;
		
		public Effect(ShaderProgram sp)
		{
			myShader = sp;
		}

		public UInt32 effectType { get { return (UInt32)myFeatures; } }

		public abstract void updateRenderState(Material m, RenderState state);

		public abstract PipelineState getPipeline(Material m);

		public bool isValid()
		{
			return myShader.isLinked;
		}
	}
}