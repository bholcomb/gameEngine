using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using Graphics;
using Util;

using Lua;

namespace GpuNoise
{
   public class AutoCorrect : Module
   {
      ShaderStorageBufferObject mySSbo = new ShaderStorageBufferObject(BufferUsageHint.StreamRead);
      ShaderProgram myMinMaxShader;
      ShaderProgram myMinMaxPass2Shader;
      ShaderProgram myAutoCorrectShader;

      public Module source { get { return inputs[0]; } set { inputs[0] = value; } }

      public AutoCorrect(int x, int y) : base(Type.AutoCorrect, x, y)
      {
         output = new Texture(x, y, PixelInternalFormat.R32f);
         output.setName("AutoCorrect output");

         List<ShaderDescriptor> shadersDesc = new List<ShaderDescriptor>();
         shadersDesc.Add(new ShaderDescriptor(ShaderType.ComputeShader, "GpuNoise.shaders.minmax-cs.glsl"));
         ShaderProgramDescriptor sd = new ShaderProgramDescriptor(shadersDesc);
         myMinMaxShader = Renderer.resourceManager.getResource(sd) as ShaderProgram;

         shadersDesc = new List<ShaderDescriptor>();
         shadersDesc.Add(new ShaderDescriptor(ShaderType.ComputeShader, "GpuNoise.shaders.minmax-pass2-cs.glsl"));
         sd = new ShaderProgramDescriptor(shadersDesc);
         myMinMaxPass2Shader = Renderer.resourceManager.getResource(sd) as ShaderProgram;

         shadersDesc = new List<ShaderDescriptor>();
         shadersDesc.Add(new ShaderDescriptor(ShaderType.ComputeShader, "GpuNoise.shaders.autocorrect-cs.glsl"));
         sd = new ShaderProgramDescriptor(shadersDesc);
         myAutoCorrectShader = Renderer.resourceManager.getResource(sd) as ShaderProgram;

         reset();
      }

      public void reset()
      {
         mySSbo.resize(4 * 2 * myX * myY); //2 floats, 1024 length array for each work group (32, 32)
      }

      public override void Dispose()
      {
         mySSbo.Dispose();
      }

      public override bool update(bool force = false)
      {
         if (source.update(force) == true)
         {
            findMinMax(source.output);
            correct(source.output);

            return true;
         }

         return false;
      }

      public void findMinMax(Texture t)
      {
         if (mySSbo.sizeInBytes < 4 * 2 * t.width)
         {
            mySSbo.resize(4 * 2 * t.width);
         }

         ComputeCommand cmd = new ComputeCommand(myMinMaxShader, t.width / 32, t.height / 32);
         cmd.addImage(t, TextureAccess.ReadOnly, 0);
         cmd.renderState.setStorageBuffer(mySSbo.id, 1);
         cmd.execute();
         GL.MemoryBarrier(MemoryBarrierFlags.ShaderStorageBarrierBit);

         cmd = new ComputeCommand(myMinMaxPass2Shader, 1);
         cmd.renderState.setStorageBuffer(mySSbo.id, 1);
         cmd.renderState.setUniform(new UniformData(0, Uniform.UniformType.Int, (t.width / 32) * (t.height / 32)));
         cmd.execute();
         GL.MemoryBarrier(MemoryBarrierFlags.ShaderStorageBarrierBit);
      }

      public void findMinMax(Texture[] t)
      {
         //assumes all images are same size
         if (mySSbo.sizeInBytes < 4 * 2 * t[0].width)
         {
            mySSbo.resize(4 * 2 * t[0].width);
         }

         for (int i = 0; i < t.Length; i++)
         {
            ComputeCommand cmd = new ComputeCommand(myMinMaxShader, t[i].width / 32, t[i].height / 32);
            cmd.addImage(t[i], TextureAccess.ReadOnly, 0);
            cmd.renderState.setStorageBuffer(mySSbo.id, 1);
            cmd.execute();
            GL.MemoryBarrier(MemoryBarrierFlags.ShaderStorageBarrierBit);

            cmd = new ComputeCommand(myMinMaxPass2Shader, 1);
            cmd.renderState.setStorageBuffer(mySSbo.id, 1);
            cmd.renderState.setUniform(new UniformData(0, Uniform.UniformType.Int, (t[i].width / 32) * (t[i].height / 32)));
            cmd.execute();
            GL.MemoryBarrier(MemoryBarrierFlags.ShaderStorageBarrierBit);
         }
      }

      public void correct(Texture t)
      {
         ComputeCommand cmd = new ComputeCommand(myAutoCorrectShader, t.width / 32, t.height / 32);
         cmd.addImage(t, TextureAccess.ReadOnly, 0);
         cmd.addImage(output, TextureAccess.WriteOnly, 1);
         cmd.renderState.setStorageBuffer(mySSbo.id, 1);
         cmd.execute();
         GL.MemoryBarrier(MemoryBarrierFlags.ShaderImageAccessBarrierBit);
      }

      public static Module create(ModuleTree tree, LuaObject config)
      {
         AutoCorrect m = new AutoCorrect(tree.size.X, tree.size.Y);
         m.myName = config.get<String>("name");

         m.source = tree.findModule(config.get<String>("source"));

         tree.addModule(m);
         return m;
      }

      public static void serialize(Module mm, LuaObject obj)
      {
         AutoCorrect m = mm as AutoCorrect;
         obj.set(m.myType.ToString(), "type");
         obj.set(m.myName, "name");
         obj.set(m.source.myName, "source");
      }
   }
}