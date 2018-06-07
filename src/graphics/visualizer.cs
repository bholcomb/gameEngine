using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;


using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using Util;

namespace Graphics
{
	public abstract class Visualizer
	{
		protected String myType;
		protected Dictionary<String, Dictionary<UInt32, MaterialEffect>> myTechniqueEffects = new Dictionary<string, Dictionary<UInt32, MaterialEffect>>();

		public string type { get { return myType; } }
		public Visualizer(string visType)
		{
         myType = visType;
		}

		public virtual UInt64 getSortId(RenderInfo info)
		{
			UInt64 sortId = 0;
			if (info.pipeline.blending.enabled == true) //this is a transparent object
			{
				//sort these back to front so transparent objects draw properly
				//regardless of material or object type
				sortId |= (UInt64)((1.0f / info.distToCamera) * UInt64.MaxValue);
			}
			else
			{
				//sort these front to back to reduce overdraw, but also in groups of similar
				//objects so we reduce state switches.  Put renderables in depth buckets 
				// which then get sorted by the texture or vbo id (depending on which is available)
				float maxDistance = 2000.0f;
				Byte distBucket = (Byte)((info.distToCamera/maxDistance) * 255); //number of  distance buckets
            sortId = 0;
            sortId |= (UInt64)info.renderState.myVertexBuffers[0].id << 32;
            sortId |= (UInt64)distBucket;
			}

			return sortId;
		}

		public void registerEffect(String technique, MaterialEffect effect)
		{
			Dictionary<UInt32, MaterialEffect> effectMap = null;
			if (myTechniqueEffects.TryGetValue(technique, out effectMap) == false)
			{
				myTechniqueEffects[technique] = new Dictionary<UInt32, MaterialEffect>();
			}

			myTechniqueEffects[technique][effect.effectType] = effect;
		}

		public MaterialEffect getEffect(String technique, UInt32 effectType)
		{
			Dictionary<UInt32, MaterialEffect> effectMap = null;
			if (myTechniqueEffects.TryGetValue(technique, out effectMap) == false)
			{
				String err = String.Format("Cannot find effect map for technique {0} in visualizer {1}", technique, myType);
				Warn.print(err);
				throw new Exception(err);
			}

			MaterialEffect effect = null;
			if (effectMap.TryGetValue(effectType, out effect) == false)
			{
				//search for best match
				UInt32 bestType = 0;
				foreach (MaterialEffect e in effectMap.Values)
				{
					UInt32 match = e.effectType & effectType;
					if (match > bestType)
						bestType = e.effectType;
				}

				if (effectMap.TryGetValue(bestType, out effect) == false)
				{
					String err = String.Format("Cannot find effect type {0} in visualizer {1} for technique {2}", ((Material.Feature)effectType).ToString(), myType, technique);
					Warn.print(err);
					throw new Exception(err);
				}
			}

			return effect;
		}

      //prepare phase
      public virtual void prepareFrameBegin() { }
      public virtual void preparePerFrame(Renderable r) { }
      public virtual void preparePerViewBegin(View v) { }
      public virtual void preparePerView(Renderable r, View v) { }
      public virtual void preparePerViewFinalize(View v) { }
      public virtual void preparePerPassBegin(Pass p) { }
      public virtual void preparePerPass(Renderable r, Pass p) { }
      public virtual void preparePerPassFinalize(Pass p) { }
      public virtual void prepareFrameFinalize() { }

      //generate command phase phase
      public virtual void generateRenderCommandsBegin(BaseRenderQueue q) { }
      public virtual void generateRenderCommand(RenderInfo r, BaseRenderQueue q) { }
      public virtual void generateRenderCommandsFinalize(BaseRenderQueue q) { }

   }
}
/*  Template to copy to new visualizers 
 
#region prepare phase
   public override void prepareFrameBegin() { }
   public override void preparePerFrame(Renderable r) { }
   public override void preparePerViewBegin(View v) { }
   public override void preparePerView(Renderable r, View v) { }
   public override void preparePerViewFinalize(View v) { }
   public override void preparePerPassBegin(Pass p) { }
   public override void preparePerPass(Renderable r, Pass p) { }
   public override void preparePerPassFinalize(Pass p) { }
   public override void prepareFrameFinalize() { }
#endregion

#region generate command phase
	public override void generateRenderCommandsBegin(BaseRenderQueue q) { }
	public override void generateRenderCommand(RenderInfo r, BaseRenderQueue q) { }
	public override void generateRenderCommandsFinalize(BaseRenderQueue q) { }                 
#endregion

*/
