using System;
using System.Collections.Generic;
using System.Windows.Forms;

using OpenTK;
using OpenTK.Input;

using Util;
using GUI;

namespace WorldEditor
{
   public class MainWindow
   {
      LeftBar myLeftBar = new LeftBar();
      MainView myMainView = new MainView();
      GameWindow myGameWindow;

      public MainWindow(GameWindow window)
      {
         myGameWindow = window;
      }

      public void onGui()
      {
         if(UI.beginMenuBar()==true)
         {
            if (UI.beginMenu("File") == true)
            {
               if (UI.menuItem("Load") == true)
               {
               }

               if (UI.menuItem("Save") == true)
               {
               }

               if (UI.menuItem("Exit") == true)
               {
                  DialogResult res = MessageBox.Show("Are you sure", "Quit", MessageBoxButtons.OKCancel);
                  if (res == DialogResult.OK)
                  {
                     myGameWindow.Exit();
                  }
               }

               UI.endMenu();
            }

            if (UI.beginMenu("Edit") == true)
            {
               UI.endMenu();
            }

            UI.endMenuBar();
         }

         myLeftBar.onGui();
         myMainView.onGui();
      }
   }
}
