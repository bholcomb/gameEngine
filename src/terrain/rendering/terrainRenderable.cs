using System;

using Graphics;
using Physics;

namespace Terrain
{
	public class TerrainRenderable : Renderable
	{
		public Chunk myChunk;

		public TerrainRenderable() 
         : base("terrain")
		{
		}

		public override bool isVisible(Camera c)
		{
			return Intersections.AACubeInFrustum(c.frustum, myChunk.myRoot.min, myChunk.myRoot.max) != Intersections.IntersectionResult.OUTSIDE;
		}
	}
	
}