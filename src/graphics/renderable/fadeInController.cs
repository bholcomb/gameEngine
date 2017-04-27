using System;
using System.Collections.Generic;

using Util;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Graphics
{
   public class FadeInController : Controller
   {
      float myMaxTime = 1.0f;
      float myCurrentTime = 0.0f;
      float myAlpha = 0.0f;

      public FadeInController(Renderable instance, float time = 1.0f)
         : base(instance, "fadeIn")
      {
         myMaxTime = time;
      }


      public override bool finished()
      {
         return myAlpha >= 1.0f;
      }

      public override void update(float dt)
      {
         myCurrentTime += dt;
         myAlpha = myCurrentTime / myMaxTime;
      }
   }
}