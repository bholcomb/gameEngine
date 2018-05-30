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
   public class Group
   {
      public enum Layout
      {
         Vertical,
         Horizontal
      }

      public enum Policy
      {
         Static,
         Dynamic
      }

      public class Element
      {
         public float position;
         public float size;
         public Policy policy;
      }

      public Vector2 myPos;
      public Vector2 mySize;
      public Vector2 myParentSize;
      public Vector2 myCursorPos;
      public Layout myLayout;
      public List<Element> myElements = new List<Element>();
      public int myCurrentIndex = 0;
      Window myWindow;

      public Group(Window win, Layout l, Vector2 pos, float[] s = null)
      {
         myWindow = win;
         myLayout = l;
         myPos = pos;
         myCursorPos = Vector2.Zero;
         mySize = Vector2.Zero;
         myParentSize = myWindow.currentGroup() != null ? myWindow.currentGroup().mySize : myWindow.size; //get parent size

         if (s != null)
         {
            foreach (float size in s)
            {
               Element e = new Element();
               e.size = size;
               myElements.Add(e);
            }
         }
         else
         {
            myElements.Add(new Element() { policy = Policy.Dynamic, position = 0.0f, size = 0.0f });
         }

         recalcElements();
      }

      public void addItem(Vector2 itemSize)
      {
         Element e = myElements[myCurrentIndex];

         //update the size
         if (e.policy == Policy.Dynamic)
         {
            e.size = myLayout == Layout.Horizontal ? itemSize.X : itemSize.Y;
            recalcElements();
         }

         myCurrentIndex++;
         bool newline = false;
         if(myCurrentIndex == myElements.Count)
         {
            newline = true;
            myCurrentIndex = 0;
         }

         if (myLayout == Layout.Horizontal)
         {
            mySize.X = Math.Max(mySize.X, myCursorPos.X + e.size);
            mySize.Y = Math.Max(mySize.Y, myCursorPos.Y + itemSize.Y);

            if(newline)
            {
               myCursorPos.X  = mySize.X;
               myCursorPos.Y  = 0;
            }
            else
            {
               myCursorPos.X += e.size;
            }
         }
         else
         {
            mySize.X = Math.Max(mySize.X, myCursorPos.X + itemSize.X);
            mySize.Y = Math.Max(mySize.Y, myCursorPos.Y + e.size);

            if(newline)
            {
               myCursorPos.X = 0;
               myCursorPos.Y  = mySize.Y;
            }
            else
            {
               myCursorPos.Y += e.size;
            }
         }
      }

      public void nextLine()
      {
         myCursorPos.X = UI.style.window.groupPadding.X;
         myCursorPos.Y = mySize.Y;
      }

      public void recalcElements()
      {
         int sizeIndex = myLayout == Layout.Horizontal ? 0 : 1;

         if (myElements.Count == 1)
         {
            return;
         }

         //calc size of statics and find dyanmics
         float totalSize = 0.0f;
         int dynamicCount = 0;
         float remainingSize = myParentSize[sizeIndex];
         foreach (Element e in myElements)
         {
            if (e.size == 0)
            {
               dynamicCount++;
               e.policy = Policy.Dynamic;
            }
            else if (e.size < 1.0f)
            {
               e.size = e.size * mySize[sizeIndex];  //percent of parent
               e.policy = Policy.Static;
            }
            else
            {
               //fixed pixel size
               e.policy = Policy.Static;
            }

            totalSize += e.size;
            remainingSize -= e.size;
         }

         if (dynamicCount == 0)
            return;

         //keep negative numbers out of the size field.
         if (remainingSize < 0.0)
         {
            remainingSize = 0;
         }

         //setup sizes and positions of elements
         float dynamicSize = 0.0f;
         totalSize = 0.0f;
         dynamicSize = remainingSize / (float)dynamicCount;

         foreach (Element e in myElements)
         {
            if (e.size == 0.0f)
            {
               e.size = dynamicSize;
            }

            e.position = totalSize;
            totalSize += e.size;
         }
      }
   }
}