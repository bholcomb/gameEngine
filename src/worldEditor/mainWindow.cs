using System;
using System.Collections.Generic;
using System.Windows.Forms;

using OpenTK;
using OpenTK.Input;

using Util;
using UI;

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
         if(ImGui.beginMenuBar()==true)
         {
            if (ImGui.beginMenu("File") == true)
            {
               if (ImGui.menuItem("Load") == true)
               {
               }

               if (ImGui.menuItem("Save") == true)
               {
               }

               if (ImGui.menuItem("Exit") == true)
               {
                  DialogResult res = MessageBox.Show("Are you sure", "Quit", MessageBoxButtons.OKCancel);
                  if (res == DialogResult.OK)
                  {
                     myGameWindow.Exit();
                  }
               }

               ImGui.endMenu();
            }

            if (ImGui.beginMenu("Edit") == true)
            {
               ImGui.endMenu();
            }

            ImGui.endMenuBar();
         }

         myLeftBar.onGui();
         myMainView.onGui();
      }
   }
}
