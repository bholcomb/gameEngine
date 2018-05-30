using System;
using System.Collections.Generic;

using Graphics;

using OpenTK;
using OpenTK.Graphics;

using Util;

namespace GUI
{
	public enum Heading { UP, RIGHT, DOWN, LEFT};
	public enum ButtonBehavior { DEFAULT, REPEAT};
	public enum Modify { FIXED, MODIFIABLE};
	public enum Orientation { VERTICAL, HORIZONTAL};
	public enum CollapseStates { MINIMZED, MAXIMIZED};
	public enum ShowStates { HIDDEN, SHOWN};
	public enum ChartType { LINES, COLUMN};
	[Flags] public enum ChartEvent { HOVERING = 0x01, CLICKED = 0x02};
	public enum ColorFormat { RGB, RGBA};
	public enum PopupType { STATIC, DYNAMIC};
	public enum LayoutFormat { DYNAMIC, STATIC};
	public enum TreeType { NODE, TAB};
	public enum HeaderAlign  { LEFT, RIGHT	};

   public class Context
   {

   }
}