using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;


using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using Util;

namespace Graphics
{
   public class SkydomeRenderable : Renderable
   {
      public Model skydome;
      public Vector3 sunPosition;
      public float starRotation;
      public float weatherSpeed;
      public float weatherIntensity = 1.0f;
      public float sunSize = 0.3f;
      public float moonSize = 0.07f;

      public SkydomeRenderable() 
         : base("skydome")
      {

      }

      public override bool isVisible(Camera c)
      {
         return true;
      }
   }
}