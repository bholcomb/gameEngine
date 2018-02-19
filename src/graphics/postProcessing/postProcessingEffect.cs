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
      }

      public String name { get; protected set; }
      public PostEffectPass postPass { get; set; }

      public Texture output { get; set; }

      public abstract List<RenderCommand> getCommands();
   }
}