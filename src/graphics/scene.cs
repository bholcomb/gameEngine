using System;
using System.Collections.Generic;

namespace Graphics
{
   public class Scene
   {
      public List<Renderable> renderables;

      public Scene()
      {
         renderables = new List<Renderable>();
      }
   }
}