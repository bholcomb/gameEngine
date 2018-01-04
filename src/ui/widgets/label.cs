using System;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

using Graphics;
using UI;
using Util;

namespace UI
{
   public static partial class ImGui
   {
      public static void label(String s, params Object[] objs)
      {
         Window win = currentWindow;
         if (win.skipItems)
            return;

         String txt = String.Format(s, objs);
         Vector2 labelSize = style.textSize(txt) + style.framePadding;

         //add in padding
         Vector2 pos = win.cursorScreenPosition + style.framePadding;

         Rect r = Rect.fromPosSize(pos, labelSize);
         win.canvas.addText(r, style.colors[(int)ElementColor.Text], txt, Alignment.Default);
         win.addItem(labelSize);
      }

      public static void image(Texture t, Vector2 size)
      {
         Window win = currentWindow;
         if (win.skipItems)
            return;

         size += style.framePadding2x;
         Vector2 pos = win.cursorScreenPosition;

         Rect r= Rect.fromPosSize(pos, size);
         win.canvas.addImage(t, r, Vector2.Zero, Vector2.One, Color4.White);
         win.addItem(size);
      }
   }
}