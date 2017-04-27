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
		protected Dictionary<String, Dictionary<UInt32, Effect>> myTechniqueEffects = new Dictionary<string, Dictionary<UInt32, Effect>>();

		public string type { get { return myType; } }
		public Visualizer()
		{
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
				//sort these front to back to prevent overdraw, but also in groups of similar
				//objects so we reduce state switches.  Put renderables in depth buckets 
				// which then get sorted by the texture or vbo id (depending on which is available)
				float maxDistance = 2000.0f;
				Byte distBucket = (Byte)((info.distToCamera/maxDistance) * 5); //number of  distance buckets
				sortId = (UInt64)distBucket << 56; //highest 8 bits
				if (info.renderState.currentTextureInfo >= 0)
					sortId |= (UInt64)info.renderState.myTextures[0].id;
				else
					sortId |= (UInt16)info.renderState.myVertexBuffers[0].id;
			}

			return sortId;
		}

		public void registerEffect(String technique, Effect effect)
		{
			Dictionary<UInt32, Effect> effectMap = null;
			if (myTechniqueEffects.TryGetValue(technique, out effectMap) == false)
			{
				myTechniqueEffects[technique] = new Dictionary<UInt32, Effect>();
			}

			myTechniqueEffects[technique][effect.effectType] = effect;
		}

		public Effect getEffect(String technique, UInt32 effectType)
		{
			Dictionary<UInt32, Effect> effectMap = null;
			if (myTechniqueEffects.TryGetValue(technique, out effectMap) == false)
			{
				String err = String.Format("Cannot find effect map for technique {0} in visualizer {1}", technique, myType);
				Warn.print(err);
				throw new Exception(err);
			}

			Effect effect = null;
			if (effectMap.TryGetValue(effectType, out effect) == false)
			{
				//search for best match
				UInt32 bestType = 0;
				foreach (Effect e in effectMap.Values)
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


		//extract phase
		public virtual void onFrameBeginExtract() { }
		public virtual void extractPerFrame(Renderable r) { }
		public virtual void extractPerView(Renderable r, View v) { }
		public virtual void extractPerViewFinalize(BaseRenderQueue q, View v) { }
		public virtual void onFrameExtractFinalize() { }

		//prepare phase
		public virtual void onFrameBeginPrepare() { }
		public virtual void preparePerFrame(Renderable r) { }
		public virtual void preparePerView(RenderInfo info, View v) { }
		public virtual void preparePerViewFinalize(BaseRenderQueue q, View v) { }
		public virtual void onFramePrepareFinalize() { }

		//submit phase
		public virtual void onSubmitNodeBlockBegin(BaseRenderQueue q) { }
		public virtual void submitRenderInfo(RenderInfo r, BaseRenderQueue q) { }
		public virtual void onSubmitNodeBlockEnd(BaseRenderQueue q) { }
	}
}
/*  Template to copy to new visualizers 
 
#region extract phase
	public override void onFrameBeginExtract() { }
	public override void extractPerFrame(Renderable r) { }
	public override void extractPerView(Renderable r, View v) { }
	public override void extractPerViewFinalize(RenderQueue q, View v) { }
	public override void onFrameExtractFinalize() { }
#endregion

#region prepare phase
	public override void onFrameBeginPrepare() { }
	public override void preparePerFrame(Renderable r) { }
	public override void preparePerView(RenderInfo info, View v) { }
	public override void preparePerViewFinalize(BaseRenderQueue q, View v) { }
	public override void onFramePrepareFinalize() { } 
#endregion

#region submit phase
	public override void onSubmitNodeBlockBegin(BaseRenderQueue q) { }
	public override void submitRenderInfo(RenderInfo r, BaseRenderQueue q) { }
	public override void onSubmitNodeBlockEnd(BaseRenderQueue q) { }
#endregion

*/
