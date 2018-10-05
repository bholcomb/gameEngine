using System;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Input;

using Util;
using GUI;

namespace WorldEditor
{
   public class LeftBar
   {
      public LeftBar()
      {

      }

      public void onGui()
      {
         UI.setNextWindowPosition(new Vector2(400, 20), SetCondition.FirstUseEver);
         UI.setNextWindowSize(new Vector2(820, 840), SetCondition.FirstUseEver);
         bool closed = false;
         UI.beginWindow("World", ref closed);


         UI.endWindow();
      }
   }
}
