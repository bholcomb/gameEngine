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
	public static partial class UI
	{
		static bool debug_viewMouseState = true;
		static bool debug_viewActiveItemState = true;

		public static void debug()
		{
			bool closed = false;
			if (beginWindow("Debug", ref closed, Window.Flags.DefaultWindow))
			{
				setWindowPosition(new Vector2(400, 100), SetCondition.FirstUseEver);
				setWindowSize(new Vector2(300, 400), SetCondition.FirstUseEver);
				if (beginMenuBar())
				{
					if (beginMenu("View"))
					{
						menuItem("Mouse State", ref debug_viewMouseState);
						menuItem("Active State", ref debug_viewActiveItemState);
						endMenu();
					}
               if (beginMenu("Eggy"))
               {
                  menuItem("Empty 1");
                  menuItem("Really Empty 2");
                  endMenu();
               }
               endMenuBar();
				}

				if (debug_viewMouseState)
				{
					label("mouse X: {0}", mouse.pos.X);
					label("mouse Y: {0}", mouse.pos.Y);
					separator();
				}

				if (debug_viewActiveItemState)
				{
					label("Hover ID: {0}", hoveredId);
					label("Active ID: {0}", activeId);
					separator();
				}


				label("Hovered Window: {0}", UI.hoveredWindow == null ? "" : UI.hoveredWindow.name);
				label("Focused Window: {0}", UI.focusedWindow == null ? "" : UI.focusedWindow.name);

				endWindow();
			}
		}
	}
}