using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;


using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using Util;

namespace Graphics
{
   public abstract class RenderableFilter
   {
      public RenderableFilter() { }
      public abstract bool shouldAccept(Renderable r);
   }

   #region common filters
   public class NullFilter : RenderableFilter
   {
      public NullFilter() : base() { }
      public override bool shouldAccept(Renderable r)
      {
         return false;
      }
   }

   public class TypeFilter : RenderableFilter
   {
      List<String> myTypes;
      public TypeFilter(List<String> acceptedTypes) : base() { myTypes = acceptedTypes; }
      public override bool shouldAccept(Renderable r)
      {
         return myTypes.Contains(r.type);
      }
   }

   public class DistanceFilter : RenderableFilter
   {
      Camera myCamera;
      float myDistanceSquared;
      public DistanceFilter(Camera c, float minDist) : base() { myCamera = c; myDistanceSquared = minDist * minDist; }
      public override bool shouldAccept(Renderable r)
      {
         float dist = (r.position - myCamera.position).LengthSquared;
         return dist < myDistanceSquared;
      }
   }

   public class InstanceFilter : RenderableFilter
   {
      List<Renderable> myInstances;
      public InstanceFilter(List<Renderable> renderables) : base() { myInstances = renderables; }
      public override bool shouldAccept(Renderable r)
      {
         return myInstances.Contains(r);
      }
   }

   #endregion
}