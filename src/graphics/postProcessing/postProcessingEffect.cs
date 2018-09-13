using System;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using Util;

namespace Graphics
{
   public abstract class PostEffect
   {
      public PostEffect()
      {
         enabled = true;
         index = 0;
      }

      public String name { get; protected set; }
      public PostEffectPass postPass { get; set; }

      public bool enabled { get; set; }

      public Texture output { get; set; }

      public int index { get; set; }

      public List<RenderCommand> getCommands()
      {
         List<RenderCommand> cmds = new List<RenderCommand>();
         if(enabled == false)
         {
            output = postPass.previousEffectOutput(this);
            return cmds;
         }

         getCommands(cmds);

         return cmds;
      }

      public abstract void getCommands(List<RenderCommand> cmds);
   }
}