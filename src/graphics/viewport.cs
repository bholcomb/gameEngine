using System;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Graphics
{
   public class Viewport
   {
      int myX, myY, myWidth, myHeight;
      bool myDirty;

      public Viewport(int x, int y, int width, int height)
      {
         myX = x;
         myY = y;
         myWidth = width;
         myHeight = height;
         myDirty = true;
         apply();
       }

      public Viewport(GameWindow win)
      {
         myX = 0;
         myY = 0;
         myWidth = win.Width;
         myHeight = win.Height;
         apply();

         win.Resize += win_Resize;
      }

      public Viewport()
      {
         int[] view = new int[4];
         GL.GetInteger(GetPName.Viewport, view);
         myX = view[0];
         myY = view[1];
         myWidth = view[2];
         myHeight = view[3];
      }

      void win_Resize(object sender, EventArgs e)
      {
         GameWindow win = sender as GameWindow;
         if(myWidth != win.Width && myHeight != win.Height)
         {
            myWidth = win.Width;
            myHeight = win.Height;
            myDirty = true;
         }

         apply();
      }

      public int x { get { return myX; } set { myX = value; myDirty = true; } }
      public int y { get { return myY; } set { myY = value; myDirty = true; } }
      public int width { get { return myWidth; } set { myWidth = value; myDirty = true; } }
      public int height { get { return myHeight; } set { myHeight = value; myDirty = true; } }
      public Vector2 size { get { return new Vector2(myWidth, myHeight); } }

      public delegate void ViewportNotifier(int x, int y, int w, int h);
      public ViewportNotifier notifier;

      public void apply()
      {
         GL.Viewport(myX, myY, myWidth, myHeight);
         if(myDirty == true && notifier != null)
         {
            notifier(myX, myX, myWidth, myHeight);
         }

         myDirty = false;
      }
   }
}
