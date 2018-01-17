using System;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Input;

using Graphics;
using Util;
using UI;

namespace WorldEditor
{
   public class MainView
   {
      enum ViewType { Elevation, Heat, Moisture, Biome };
      ViewType myViewType;

      public MainView()
      {
         myViewType = ViewType.Biome;
      }

      public void onGui()
      {
         ImGui.setNextWindowPosition(new Vector2(400, 0), SetCondition.FirstUseEver);
         ImGui.setNextWindowSize(new Vector2(820, 840), SetCondition.FirstUseEver);
         bool closed = false;
         ImGui.beginWindow("MainView", ref closed, Window.Flags.NoResize | Window.Flags.AlwaysAutoResize);
         
         ImGui.beginGroup();
         ImGui.setWindowLayout(Window.Layout.Horizontal);
         if (ImGui.button("Elevation", new Vector2(150, 20)) == true)
            myViewType = ViewType.Elevation;
         if (ImGui.button("Heat", new Vector2(150, 20)) == true)
            myViewType = ViewType.Heat;
         if (ImGui.button("Moisture", new Vector2(150, 20)) == true)
            myViewType = ViewType.Moisture;
         if (ImGui.button("Biome", new Vector2(150, 20)) == true)
            myViewType = ViewType.Biome;
         ImGui.endGroup();


         ImGui.endWindow();
      }
   }
}
