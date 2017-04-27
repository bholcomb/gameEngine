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
         Vector2 labelSize = style.textSize(txt) + style.framePadding2x;

         //move cursor down for the size of the text accounting for the padding
         Vector2 pos = win.cursorPosition + new Vector2(style.framePadding.X, -(labelSize.Y - style.framePadding.Y));

         win.canvas.addText(pos, style.colors[(int)ElementColor.Text], txt);
         win.addItem(labelSize);
      }

      public static void image(Texture t, Vector2 size)
      {
         Window win = currentWindow;
         if (win.skipItems)
            return;

         size += style.framePadding2x;
         Vector2 pos = win.cursorPosition - new Vector2(0, size.Y - style.framePadding.Y);

         Rect r= Rect.fromPosSize(pos, size);

         win.canvas.addImage(t, r, Vector2.Zero, Vector2.One, Color4.White);
         win.addItem(size);
      }
   }
}