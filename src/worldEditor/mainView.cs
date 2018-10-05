using System;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Input;

using Graphics;
using Util;
using GUI;

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
         UI.setNextWindowPosition(new Vector2(400, 0), SetCondition.FirstUseEver);
         UI.setNextWindowSize(new Vector2(820, 840), SetCondition.FirstUseEver);
         bool closed = false;
         UI.beginWindow("MainView", ref closed, Window.Flags.RootWindow);
         
         UI.beginLayout(Layout.Direction.Horizontal);
         if (UI.button("Elevation", new Vector2(150, 20)) == true)
            myViewType = ViewType.Elevation;
         if (UI.button("Heat", new Vector2(150, 20)) == true)
            myViewType = ViewType.Heat;
         if (UI.button("Moisture", new Vector2(150, 20)) == true)
            myViewType = ViewType.Moisture;
         if (UI.button("Biome", new Vector2(150, 20)) == true)
            myViewType = ViewType.Biome;
         UI.endLayout();


         UI.endWindow();
      }
   }
}
