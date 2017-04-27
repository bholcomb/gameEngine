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
		static bool debug_viewMouseState = true;
		static bool debug_viewActiveItemState = true;

		public static void debug()
		{
			bool closed = false;
			if (ImGui.beginWindow("Debug", ref closed, Window.Flags.MenuBar | Window.Flags.ShowBorders))
			{
				ImGui.setWindowPosition(new Vector2(400, 100), SetCondition.FirstUseEver);
				ImGui.setWindowSize(new Vector2(300, 400), SetCondition.FirstUseEver);
				if (ImGui.beginMenuBar())
				{
					if (ImGui.beginMenu("View"))
					{
						ImGui.menuItem("Mouse State", ref debug_viewMouseState);
						ImGui.menuItem("Active State", ref debug_viewActiveItemState);
						ImGui.endMenu();
					}
					ImGui.endMenuBar();
				}

				if (debug_viewMouseState)
				{
					ImGui.label("mouse X: {0}", mouse.pos.X);
					ImGui.label("mouse Y: {0}", mouse.pos.Y);
					ImGui.separator();
				}

				if (debug_viewActiveItemState)
				{
					ImGui.label("Hover ID: {0}", hoveredId);
					ImGui.label("Active ID: {0}", activeId);
					ImGui.separator();
				}


				ImGui.label("Hovered Window: {0}", ImGui.hoveredWindow == null ? "" : ImGui.hoveredWindow.name);
				ImGui.label("Focused Window: {0}", ImGui.focusedWindow == null ? "" : ImGui.focusedWindow.name);

				ImGui.endWindow();
			}
		}
	}
}