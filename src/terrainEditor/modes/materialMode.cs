using System;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;

using GUI;
using Util;
using Engine;
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
         int size = (int)UI.displaySize.X / 10;
         int x = (int)UI.displaySize.X / 10;
         int y = (int)UI.displaySize.Y  - 250;
         UI.beginWindow("Terrain Material");
         UI.setWindowPosition(new Vector2(x, y), SetCondition.FirstUseEver);
         UI.setWindowSize(new Vector2(UI.displaySize.X - x -x, size + 75), SetCondition.FirstUseEver); //near the bottom of the screen
         UI.beginLayout(Layout.Direction.Horizontal);
         GUI.Window win = UI.currentWindow;

         for (int i = myFirstVisible; i < myFirstVisible + maxMaterials; i++)
         {
            UI.beginLayout(Layout.Direction.Vertical);
            if (UI.button(Terrain.MaterialManager.myMaterialTextureArray, myMaterialPallete[i].side, new Vector2(size)))
            //if (UI.button(myMaterialPallete[i].name, new Vector2(size)))
            {
               myActiveIndex = i;
            }

            if (myActiveIndex == i)
            {
               //Color4 backup = UI.style.getColor(ElementColor.Text);
               //UI.style.colors[(int)ElementColor.Text] = Color4.Red;
               //UI.label(myMaterialPallete[i].name);
               //UI.style.colors[(int)ElementColor.Text] = backup;
            }
            else
            {   
               UI.label(myMaterialPallete[i].name);
            }
            UI.endLayout();
         }
         UI.endWindow();

         String name;
         if (myMaterialPallete[myActiveIndex] != null)
            name = myMaterialPallete[myActiveIndex].name;
         else
            name = "";

         myEditor.context.currentMaterial = name;

         if (UI.mouse.isButtonClicked(MouseButton.Left) == true && UI.hoveredWindow == null)
         {
            assignMaterial();
         }

         if(UI.mouse.wheelDelta != 0.0f && UI.hoveredWindow == UI.findWindow("Terrain Material"))
         {
            myFirstVisible += (int)UI.mouse.wheelDelta;
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