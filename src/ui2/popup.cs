using System;

using OpenTK;

namespace GUI
{
   public class Popup
   {
      public UInt32 myPopUpId;
      public Window myWin;
      public Window myParentWin;
      public UInt32 myParentMenuSet;
      public Vector2 myMousePositionOnOpen;

      public Popup()
      {

      }

      public Popup(UInt32 id, Window parentwin, UInt32 parentMenuSet, Vector2 mousePos)
      {
         myPopUpId = id;
         myWin = null;
         myParentWin = parentwin;
         myParentMenuSet = parentMenuSet;
         myMousePositionOnOpen = mousePos;
      }
   }

}