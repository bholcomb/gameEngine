using System;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;

using UI;
using Util;
using Events;
using Graphics;
using Terrain;


namespace Editor
{
   public class MaterialMode : Mode
   {
      const int maxMaterials = 8;
      List<Terrain.Material> myMaterialPallete = new List<Terrain.Material>();
      int myActiveIndex = 0;
      int myFirstVisible = 0;

      public MaterialMode(Editor e)
         : base(e, "Material Mode")
      {
         foreach (Terrain.Material mat in Terrain.MaterialManager.myMaterials.Values)
         {
            if (mat.name != "air")
               myMaterialPallete.Add(mat);
         }
      }

      public override void onGui()
      {
         int size = ImGui.width / 10;
         int x = ImGui.width / 10;
         int y = ImGui.height-200;
         ImGui.beginWindow("Terrain Material");
         ImGui.setWindowPosition(new Vector2(x, y), SetCondition.FirstUseEver);
         ImGui.setWindowSize(new Vector2(ImGui.width - x -x, 200), SetCondition.FirstUseEver);
         ImGui.setWindowLayout(Window.Layout.Horizontal);
         for (int i = myFirstVisible; i < myFirstVisible + maxMaterials; i++)
         {
            ImGui.beginGroup();
            ImGui.setWindowLayout(Window.Layout.Vertical);
            if (ImGui.button(Terrain.MaterialManager.myMaterialTextureArray, myMaterialPallete[i].side, new Vector2(size)))
            {
               myActiveIndex = i;
            }
            ImGui.label(myMaterialPallete[i].name);

            if (myActiveIndex == i)
            {
               Rect r = new Rect();
               ImGui.currentWindow.canvas.addRect(r, Color4.White);
            }
            ImGui.endGroup();
         }
         ImGui.endWindow();

         String name;
         if (myMaterialPallete[myActiveIndex] != null)
            name = myMaterialPallete[myActiveIndex].name;
         else
            name = "";

         myEditor.context.currentMaterial = name;

         if (ImGui.mouse.buttonClicked[(int)MouseButton.Left] == true && ImGui.hoveredWindow == null)
         {
            assignMaterial();
         }

         if(ImGui.mouse.wheelDelta != 0.0f && ImGui.hoveredWindow == ImGui.findWindow("Terrain Material"))
         {
            myFirstVisible += (int)ImGui.mouse.wheelDelta;
            if (myFirstVisible < 0) myFirstVisible = 0;
            if (myFirstVisible > myMaterialPallete.Count - maxMaterials) myFirstVisible = myMaterialPallete.Count - maxMaterials;
         }
      }


      public void assignMaterial()
      {
         //get block selection
         NodeLocation nl = myEditor.context.currentLocation;
         if (nl != null)
         {
            //send create blocks cmd
            AssignMaterialCommand cmd = new AssignMaterialCommand(nl, myMaterialPallete[myActiveIndex].name);
            myEditor.world.dispatch(cmd);
         }
      }
   }
}