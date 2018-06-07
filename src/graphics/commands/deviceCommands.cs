using System;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Graphics
{
   public class PushDebugMarkerCommand : RenderCommand
   {
      string marker;

      public PushDebugMarkerCommand(String m) : base()
      {
         marker = m;
      }

      public override void execute()
      {
#if DEBUG || TRACE
         GL.PushDebugGroup(DebugSourceExternal.DebugSourceApplication, 0, marker.Length, marker);
#endif
      }
   }

   public class PopDebugMarkerCommand : RenderCommand
   {
      public PopDebugMarkerCommand() : base()
      {

      }

      public override void execute()
      {
#if DEBUG || TRACE
         GL.PopDebugGroup();
#endif
      }
   }

	public class SetRenderTargetCommand : RenderCommand
	{
		RenderTarget myRenderTarget;

		public SetRenderTargetCommand(RenderTarget rt) : base()
		{
			myRenderTarget = rt;
		}

		public override void execute()
		{
			Renderer.device.setRenderTarget(myRenderTarget);
		}
	}

	public class SetPipelineCommand : RenderCommand
	{
		PipelineState myPs;

		public SetPipelineCommand(PipelineState ps) : base()
		{
			myPs = ps;
		}

		public override void execute()
		{
			Renderer.device.bindPipeline(myPs);
		}
	}

	public class SetRenderStateCommand : RenderCommand
	{
		RenderState myRenderState;
		public SetRenderStateCommand(RenderState state)
			: base()
		{
			myRenderState = state;
		}

		public override void execute()
		{
			//apply the renderstate
			myRenderState.apply();
		}
	}
   public class ClearColorCommand : RenderCommand
   {
      Color4 myColor;

      public ClearColorCommand(Color4 color)
         : base()
      {
         myColor = color;
      }

      public override void execute()
      {
         GL.ClearColor(myColor);
      }
   }

   public class ClearCommand : RenderCommand
	{
		ClearBufferMask myClearMask;

		public ClearCommand() : this(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit) { }
		public ClearCommand(ClearBufferMask mask)
			: base()
		{
			myClearMask = mask;
		}

		public override void execute()
		{
			GL.Clear(myClearMask);
		}
	}

	public class BindUniformCommand : RenderCommand
	{
		int myBufferId;
		int myLocation;

		public BindUniformCommand(int bufferId, int location) : base()
		{
			myBufferId = bufferId;
			myLocation = location;
		}

		public override void execute()
		{
			Renderer.device.bindUniformBuffer(myBufferId, myLocation);
		}
	}

	public class BindStorageBufferCommand : RenderCommand
	{
		int myBufferId;
		int myLocation;

		public BindStorageBufferCommand(int bufferId, int location) : base()
		{
			myBufferId = bufferId;
			myLocation = location;
		}

		public override void execute()
		{
			Renderer.device.bindStorageBuffer(myBufferId, myLocation);
		}
	}

	public class BindImagebufferCommand : RenderCommand
	{
		int myBufferId;
		int myLocation;
		TextureAccess myAccess;
		SizedInternalFormat myFormat;

		public BindImagebufferCommand(int bufferId, int location, TextureAccess access, SizedInternalFormat format) : base()
		{
			myBufferId = bufferId;
			myLocation = location;
			myAccess = access;
			myFormat = format;
		}

		public override void execute()
		{
			Renderer.device.bindImageBuffer(myBufferId, myLocation, myAccess, myFormat);
		}
	}

	public class BindTextureCommand : RenderCommand
	{
		int myBufferId;
		int myLocation;
		TextureTarget myType;

		public BindTextureCommand(int bufferId, int location, TextureTarget type) : base()
		{
			myBufferId = bufferId;
			myLocation = location;
			myType = type;
		}

		public override void execute()
		{
			Renderer.device.bindTexture(myBufferId, myLocation, myType);
		}
	}

	public class BindVertexBufferCommand : RenderCommand
	{
		int myBufferId;
		int myLocation;
		int myOffset;
		int myStride;

		public BindVertexBufferCommand(int bufferId, int location, int offset, int stride) : base()
		{
			myBufferId = bufferId;
			myLocation = location;
			myOffset = offset;
			myStride = stride;
		}

		public override void execute()
		{
			Renderer.device.bindVertexBuffer(myBufferId, myLocation, myOffset, myStride);
		}
	}

	public class BindIndexBufferCommand : RenderCommand
	{
		int myBufferId;

		public BindIndexBufferCommand(int bufferId) : base()
		{
			myBufferId = bufferId;
		}

		public override void execute()
		{
			Renderer.device.bindIndexBuffer(myBufferId);
		}
	}

	public class DrawArraysCommand : RenderCommand
	{
		PrimitiveType myType;
		int myFirst;
		int myCount;

		public DrawArraysCommand(PrimitiveType type, int first, int count) : base()
		{
			myType = type;
			myFirst = first;
			myCount = count;
		}

		public override void execute()
		{
			Renderer.device.drawArray(myType, myFirst, myCount);
		}
	}

	public class DrawIndexedCommand : RenderCommand
	{
		PrimitiveType myType;
		int myCount;
		int myOffset;
		DrawElementsType myIndexType;

		public DrawIndexedCommand(PrimitiveType type, int count, int offset = 0, DrawElementsType indexType = DrawElementsType.UnsignedShort) : base()
		{
			myType = type;
			myCount = count;
			myOffset = offset;
			myIndexType = indexType;
		}

		public override void execute()
		{
			Renderer.device.drawIndexed(myType, myCount, myOffset, myIndexType);
		}
	}

   public class DeviceResetCommand : RenderCommand
   {
      public DeviceResetCommand() : base()
      {
      }

      public override void execute()
      {
         Renderer.device.reset();
      }
   }
}