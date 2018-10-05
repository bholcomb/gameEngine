using System;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

using Graphics;
using Util;

namespace GUI
{
   public class Layout
   {
      public enum Direction : int
      {
         Horizontal,
         Vertical
      }

      public class Element
      {
         public float position;
         public Vector2 size;
         public float policy;
      }

      public Layout myParent;
      public Window myWindow;
      public Vector2 myPos;
      public Vector2 mySize;
      public Vector2 myCursorPos;
      public Direction myDirection;

      public Layout(Window win, Direction l, Vector2 pos)
      {
         myWindow = win;
         myDirection = l;
         myPos = pos;
         myCursorPos = Vector2.Zero;
         mySize = Vector2.Zero;
         myParent = myWindow.currentLayout;
      }

      public void addItem(Vector2 itemSize)
      {
         mySize.X = Math.Max(mySize.X, myCursorPos.X + itemSize.X);
         mySize.Y = Math.Max(mySize.Y, myCursorPos.Y + itemSize.Y);

         if (myDirection == Direction.Horizontal)
         {
            myCursorPos.X += itemSize.X;
         }
         else
         {
            myCursorPos.Y += itemSize.Y;
         }
      }

      public void nextLine()
      {
         if(myDirection == Direction.Horizontal)
         {
            myCursorPos.X = 0;
            myCursorPos.Y += mySize.Y;
         }
         else
         {
            myCursorPos.X += mySize.X;
            myCursorPos.Y = 0;
         }
      }
   }
}