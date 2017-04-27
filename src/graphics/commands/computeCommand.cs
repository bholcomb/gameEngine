using System;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using Util;

namespace Graphics 
{
   public class ComputeCommand : StatelessRenderCommand
   {
      int myWorgroupX = 1;
      int myWorkgroupY = 1;
      int myWorkgroupZ = 1;

      public ComputeCommand(ShaderProgram shader, int x) : this(shader, x, 1, 1) { }
      public ComputeCommand(ShaderProgram shader, int x, int y) : this(shader, x, y, 1) { }
      public ComputeCommand(ShaderProgram shader, int x, int y, int z)
         : base()
      {
			pipelineState.shaderProgram = shader;
			pipelineState.generateId();
         myWorgroupX = x;
         myWorkgroupY = y;
         myWorkgroupZ = z;
      }

      public void addImage(Texture t, TextureAccess access, int bindPoint)
      {
			renderState.setImageBuffer((int)t.id(), bindPoint, access, (SizedInternalFormat)t.pixelFormat);
      }

      public override void execute()
      {
			base.execute();

         GL.DispatchCompute(myWorgroupX, myWorkgroupY, myWorkgroupZ);
      }
   }
}