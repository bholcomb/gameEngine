using System;

using Graphics;
using Util;

using OpenTK;

namespace GUI
{
   public enum CursorType
   {
      NONE,
      ARROW,
      TEXT,
      MOVE,
      RESIZE_VERTICAL,
      RESIZE_HORIZONTAL,
      RESIZE_TOP_LEFT_DOWN_RIGHT,
      RESIZE_TOP_RIGHT_DOWN_LEFT
   };

   public class Cursor
   {
      public CursorType image;
      public Vector2 size;
      public Vector2 offset;
   };

}