using System;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Input;

using Util;
using UI;

namespace WorldEditor
{
   public class LeftBar
   {
      public LeftBar()
      {

      }

      public void onGui()
      {
         ImGui.setNextWindowPosition(new Vector2(400, 20), SetCondition.FirstUseEver);
         ImGui.setNextWindowSize(new Vector2(820, 840), SetCondition.FirstUseEver);
         bool closed = false;
         ImGui.beginWindow("World", ref closed);


         ImGui.endWindow();
      }
   }
}
