using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;

using OpenTK;
using OpenTK.Graphics.OpenGL;

using Graphics;
using Util;

namespace Terrain
{
	public class DrawChunk
	{
		public int changeNumber;
		public int lastFrameUsed;
		public BufferMemoryManager.Page mem;

		//counted in vertexes
		public int firstOffset;

		public int solidOffset;
		public int solidCount;
		public int transOffset;
		public int transCount;
		public int waterOffset;
		public int waterCount;
	};


	public class TerrainRenderManager
   {
		public Dictionary<UInt64, TerrainRenderable> myRenderables = new Dictionary<ulong, TerrainRenderable>();

		protected BufferMemoryManager myMemory = new BufferMemoryManager(1024 * 1024 * 30); //30MB
		protected Dictionary<UInt64, DrawChunk> myLoadedChunks = new Dictionary<UInt64, DrawChunk>();
		protected HashSet<UInt64> myRequestedIds = new HashSet<UInt64>();
		Object myLock = new Object();

		public RenderTarget myWaterRenderTarget = null;
		Texture myWaterDepthBuffer;
		Texture myWaterColorBuffer;

		public BufferMemoryManager memoryManager { get { return myMemory; } }

		public TerrainRenderManager(World w)
      {
			world = w;
         world.chunkAdded += handleChunkAdded;
         world.chunkRemoved += handleChunkRemoved;
         world.chunksReset += handleChunksReset;

         TriangleVisualizer vis = new TriangleVisualizer(this);
         Renderer.registerVisualizer("terrain", vis);

         ShaderProgramDescriptor sd;
         ShaderProgram sp;
         List<ShaderDescriptor> desc = new List<ShaderDescriptor>();

         desc.Add(new ShaderDescriptor(ShaderType.VertexShader, "..\\src\\Terrain\\rendering\\shaders\\terrainTri-vs.glsl", ShaderDescriptor.Source.File));
         desc.Add(new ShaderDescriptor(ShaderType.GeometryShader, "..\\src\\Terrain\\rendering\\shaders\\terrainTri-gs.glsl", ShaderDescriptor.Source.File));
         desc.Add(new ShaderDescriptor(ShaderType.FragmentShader, "..\\src\\Terrain\\rendering\\shaders\\terrainTri-ps.glsl", ShaderDescriptor.Source.File));
         sd = new ShaderProgramDescriptor(desc, null, "terrain:perpixel");
         sp = Renderer.resourceManager.getResource(sd) as ShaderProgram;
         vis.registerEffect("forward-lighting", new PerPixelSolidEffect(sp));
         vis.registerEffect("forward-lighting", new PerPixelTransparentEffect(sp));

         desc.Clear();
         desc.Add(new ShaderDescriptor(ShaderType.VertexShader, "..\\src\\Terrain\\rendering\\shaders\\terrainTri-vs.glsl", ShaderDescriptor.Source.File));
         desc.Add(new ShaderDescriptor(ShaderType.GeometryShader, "..\\src\\Terrain\\rendering\\shaders\\terrainTri-gs.glsl", ShaderDescriptor.Source.File));
         desc.Add(new ShaderDescriptor(ShaderType.FragmentShader, "..\\src\\Terrain\\rendering\\shaders\\water-ps.glsl", ShaderDescriptor.Source.File));
         sd = new ShaderProgramDescriptor(desc, null, "terrain:water");
         sp = Renderer.resourceManager.getResource(sd) as ShaderProgram;
         vis.registerEffect("forward-lighting", new PerPixelWaterEffect(sp));
      }

		public void init(Vector2 size)
		{
         //cleanup any previous 
         if(myWaterRenderTarget != null)
         {
            myWaterRenderTarget.Dispose();
            myWaterColorBuffer.Dispose();
            myWaterDepthBuffer.Dispose();
         }

			myWaterColorBuffer = new Texture((int)size.X, (int)size.Y, PixelInternalFormat.Rgba8);
			myWaterDepthBuffer = new Texture((int)size.X, (int)size.Y, PixelInternalFormat.DepthComponent32f);

			List<RenderTargetDescriptor> rtdesc = new List<RenderTargetDescriptor>();
			rtdesc.Add(new RenderTargetDescriptor() { attach = FramebufferAttachment.ColorAttachment0, tex = myWaterColorBuffer }); //uses an existing texture
			rtdesc.Add(new RenderTargetDescriptor() { attach = FramebufferAttachment.DepthAttachment, tex = myWaterDepthBuffer }); //uses an existing texture
			myWaterRenderTarget = new RenderTarget((int)size.X, (int)size.Y, rtdesc);
		}

      public World world { get; set; }

		public void handleChunkAdded(Chunk c)
		{
			TerrainRenderable r = new TerrainRenderable();
			r.myChunk = c;
			myRenderables[c.key] = r;
			Renderer.scene.renderables.Add(r);
		}

		public void handleChunkRemoved(Chunk c)
		{
			TerrainRenderable r = null;
			if(myRenderables.TryGetValue(c.key, out r)== true)
			{
				Renderer.scene.renderables.Remove(r);
				myRenderables.Remove(c.key);
			}
		}

		public void handleChunksReset()
		{
			foreach (TerrainRenderable r in myRenderables.Values)
				Renderer.scene.renderables.Remove(r);

			myRenderables.Clear();
		}

		#region Buffer Management

		public void debugBuffer()
		{
			myMemory.visualDebug();
		}

		public int memoryUsage()
		{
			return myMemory.used;
		}

		public void removeStale()
		{
			int currentFrame = Renderer.frameNumber;

			List<UInt64> toRemove = new List<UInt64>();
			lock (myLock)
			{
				foreach (KeyValuePair<UInt64, DrawChunk> pair in myLoadedChunks)
				{
					if (pair.Value.lastFrameUsed + 5 < currentFrame) //not used in the last 5 frames
					{
						myMemory.dealloc(pair.Value.mem);
						toRemove.Add(pair.Key);
					}
				}
			}

			foreach (UInt64 k in toRemove)
			{
				lock (myLock)
				{
					myLoadedChunks.Remove(k);
				}
			}
		}

		public void clear()
		{
			lock (myLock)
			{
				foreach (KeyValuePair<UInt64, DrawChunk> pair in myLoadedChunks)
				{
					myMemory.dealloc(pair.Value.mem);
				}

				myLoadedChunks.Clear();
			}
		}

		public Task<DrawChunk> bufferChunkAsync(Chunk chunk, BufferChunkFunc bufferFunc)
		{
			return Task.Run(() => bufferFunc(chunk));
		}

		public delegate DrawChunk BufferChunkFunc(Chunk chunk);

		public bool isLoaded(Chunk chunk, BufferChunkFunc bufferFunc)
		{
			DrawChunk dc = findDrawChunk(chunk);
			if (dc != null)
			{
				if (chunk.changeNumber == dc.changeNumber)
				{
					dc.lastFrameUsed = Renderer.frameNumber;
					return true;
				}

				//it's out of date, remove it to trigger a reload
				updateChunk(chunk, bufferFunc);
				return true;
			}

			//it's been requested, we're working on it
			if (myRequestedIds.Contains(chunk.key))
			{
				return true;
			}

			return false;
		}

		public async void loadChunk(Chunk chunk, BufferChunkFunc bufferFunc)
		{
			if (myRequestedIds.Contains(chunk.key) == true) //already requested
				return;

			lock (myLock)
			{
				myRequestedIds.Add(chunk.key);
			}

			DrawChunk dc = await bufferChunkAsync(chunk, bufferFunc);
			//DrawChunk dc = bufferFunc(chunk);

			//finished the async call
			lock (myLock)
			{
				myLoadedChunks[chunk.key] = dc;
				myRequestedIds.Remove(chunk.key);
			}
		}

		public void removeChunk(Chunk chunk)
		{
			//actually free the area in memory
			DrawChunk dc = findDrawChunk(chunk);
			if (dc != null)
			{
				myMemory.dealloc(dc.mem);
			}

			lock (myLock)
			{
				myLoadedChunks.Remove(chunk.key);
			}
		}

		public void updateChunk(Chunk chunk, BufferChunkFunc bufferfunc)
		{
			removeChunk(chunk);

			DrawChunk dc = bufferfunc(chunk);
			lock (myLock)
			{
				myLoadedChunks[chunk.key] = dc;
			}
		}

		public DrawChunk findDrawChunk(Chunk chunk)
		{
			DrawChunk dc;
			if (myLoadedChunks.TryGetValue(chunk.key, out dc) == true)
			{
				return dc;
			}

			return null;
		}

		#endregion
	}
}