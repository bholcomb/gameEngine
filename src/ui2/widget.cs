using System;

namespace GUI
{
   [Flags]
   public enum WidgetState
   {
      MODIFIED = 1 << 1,
      INACTIVE = 1 << 2,
      ENTERED = 1 << 3,
      HOVER = 1 << 4,
      ACTIVED = 1 << 5,
      LEFT = 1 << 6,
      HOVERED = HOVER | MODIFIED,
      ACTIVE = ACTIVED | MODIFIED
   };
}