using System;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

using Graphics;
using Util;

namespace UI2
{
   public static partial class UI
   {
      public static void openPopup(String label)
      {
         Window win = currentWindow;
         UInt32 popupId = win.getChildId(label);
         if (isPopupOpen(popupId) == false)
         {
            Popup pref = new Popup(popupId, win, win.getChildId("popups"), mouse.pos);
            myOpenedPopupStack.Push(pref);
         }
      }

      public static bool beginPopup(String label, Window.Flags extraFlags)
      {
         Window win = currentWindow;
         UInt32 popupId = win.getChildId(label);
         if (isPopupOpen(popupId) == false)
         {
            return false;
         }

         style.pushStyleVar(StyleVar.WindowRounding, 0.0f);
         style.pushStyleVar(StyleVar.WindowFillAlpha, 1.0f);
         Window.Flags flags = extraFlags | Window.Flags.Popup | Window.Flags.AutoResize;

         String name;
         if(flags.HasFlag(Window.Flags.ChildMenu)==true)
         {
            name = "menu_" + label;
         }
         else
         {
            name = "popup_" + label;
         }

         bool closed = false;
         bool opened = beginWindow(name, ref closed, flags);

         //turn off show borders if the parent window doesn't have them
         if(win.flags.HasFlag(Window.Flags.Borders)==false)
         {
            currentWindow.flags &= ~Window.Flags.Borders;
         }

         if(!opened)
         {
            endPopup();
         }

         return opened;
      }

      public static bool beginPopupModal(String label, ref bool opened, Window.Flags flags)
      {
         return true;
      }

      public static bool beginPopupContextItem(String label, int mouseButton = 1)
      {
         if(hoveredId != 0 && mouse.buttonClicked[mouseButton]==true)
         {
            openPopup(label);
         }
         return beginPopup(label, 0);
      }

      public static bool beginPopupContextWindow(String label, int mouseButton = 1, bool alsoOverItems = true)
      {
         if(hoveredWindow != null && mouse.buttonClicked[mouseButton])
         {
            if(alsoOverItems==true || hoveredId == 0)
            {
               openPopup(label);
            }
         }

         return beginPopup(label, 0);
      }

      public static bool beginPopupContextVoid(String label, int mouseButton = 1)
      {
         if (hoveredWindow == null && mouse.buttonClicked[mouseButton])
         {
            openPopup(label);
         }

         return beginPopup(label, 0);
      }

      public static void endPopup()
      {
         Window win = currentWindow;
         System.Diagnostics.Debug.Assert(win.flags.HasFlag(Window.Flags.Popup));

         endWindow();
         style.popStyleVar(2);
      }

      public static void closeCurrentPopup()
      {
         myOpenedPopupStack.Pop();
      }
   }
}