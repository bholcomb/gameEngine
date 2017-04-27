using System;
using System.Collections.Generic;

using OpenTK;

using Util;
using UI;

namespace Editor
{
   public abstract class Layer
   {
      public Layer()
      {

      }

      public abstract void onGui();
   }

   public class LayerManager
   {
      public LayerManager(Editor e)
      {
      }
   }
}