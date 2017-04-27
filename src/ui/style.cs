using System;
using System.Collections.Generic;

using Graphics;

using OpenTK;
using OpenTK.Graphics;

namespace UI
{
   [Flags]
   public enum Alignment
   {
      Left = 1 << 0,
      HCenter = 1 << 1,
      Right = 1 << 2,
      Top = 1 << 3,
      VCenter = 1 << 4,
      Default = Left | Top,
      Middle = HCenter | VCenter
   }

   public enum ElementColor
   {
      Text,
      TextDisabled,
      WindowBg,
      ChildWindowBg,
      Border,
      BorderShadow,
      FrameBg,               // Background of checkbox, radio button, plot, slider, text input
      FrameBgHovered,
      FrameBgActive,
      TitleBg,
      TitleBgCollapsed,
      TitleBgActive,
      MenuBarBg,
      ScrollbarBg,
      ScrollbarGrab,
      ScrollbarGrabHovered,
      ScrollbarGrabActive,
      ComboBg,
      CheckMark,
      SliderGrab,
      SliderGrabActive,
      Button,
      ButtonHovered,
      ButtonActive,
      Header,
      HeaderHovered,
      HeaderActive,
      Column,
      ColumnHovered,
      ColumnActive,
      ResizeGrip,
      ResizeGripHovered,
      ResizeGripActive,
      CloseButton,
      CloseButtonHovered,
      CloseButtonActive,
      PlotLines,
      PlotLinesHovered,
      PlotHistogram,
      PlotHistogramHovered,
      TextSelectedBg,
      TooltipBg,
      ModalWindowDarkening,  // darken entire screen when a modal window is active
      
      Max_Elements
   }

   public enum StyleVar
   {
      Alpha,               // float
      WindowFillAlpha,     // float
      WindowPadding,       // Vec2
      WindowRounding,      // float
      WindowMinSize,       // Vec2
      ChildWindowRounding, // float
      FramePadding,        // Vec2
      FrameRounding,       // float
      ItemSpacing,         // Vec2
      ItemInnerSpacing,    // Vec2
      IndentSpacing,       // float
      GrabMinSize          // float
   };

   public class Style
   {
      public float alpha;
      public Vector2 windowPadding;
      public Vector2 windowMinSize;
      public Alignment windowTitleAlign;
      public float windowRounding;
      public float childWindowRounding;
      public Vector2 framePadding;
      public Vector2 framePadding2x;
      public float frameRounding;
      public Vector2 itemSpacing;
      public Vector2 itemInnerSpacing;
      public Vector2 touchExtraPadding;
      public float windowFillAlphaDefault;
      public float indentSpacing;
      public float columnsMinSpacing;
      public float scrollbarSize;
      public float scrollbarRounding;
      public float grabMinSize;
      public float grabRounding;
      public Vector2 displayWindowPadding;
      public Vector2 displaySafeAreaPadding;
      public bool antiAliasedLines;
      public bool antiAliasedShapes;
      public float curveTesselationTolerance;
      public Color4[] colors = new Color4[(int)ElementColor.Max_Elements];

      struct ColorMod
      {
         public ElementColor color;
         public Color4 previousColor;
      }
      Stack<ColorMod> colorStack;

      struct StyleMod
      {
         public StyleVar style;
         public Vector2 prevousValue;
      }
      Stack<StyleMod> styleStack = new Stack<StyleMod>();

      Stack<float> fontSizeStack = new Stack<float>();
      public float currentFontSize { get { return fontSizeStack.Peek(); } }

      public Style()
      {
         fontSizeStack.Push(15.0f);

         alpha = 1.0f;             // Global alpha applies to everything in ImGui
         windowPadding = new Vector2(8, 8);      // Padding within a window
         windowMinSize = new Vector2(32, 32);    // Minimum window size
         windowRounding = 9.0f;             // Radius of window corners rounding. Set to 0.0f to have rectangular windows
         windowTitleAlign = Alignment.Left;  // Alignment for title bar text
         childWindowRounding = 0.0f;             // Radius of child window corners rounding. Set to 0.0f to have rectangular windows
         framePadding = new Vector2(4, 3);      // Padding within a framed rectangle (used by most widgets)
         framePadding2x = framePadding * 2.0f;      // Padding within a framed rectangle (used by most widgets)
         frameRounding = 0.0f;             // Radius of frame corners rounding. Set to 0.0f to have rectangular frames (used by most widgets).
         itemSpacing = new Vector2(8, 4);      // Horizontal and vertical spacing between widgets/lines
         itemInnerSpacing = new Vector2(4, 4);      // Horizontal and vertical spacing between within elements of a composed widget (e.g. a slider and its label)
         touchExtraPadding = new Vector2(0, 0);      // Expand reactive bounding box for touch-based system where touch position is not accurate enough. Unfortunately we don't sort widgets so priority on overlap will always be given to the first widget. So don't grow this too much!
         windowFillAlphaDefault = 0.70f;            // Default alpha of window background, if not specified in ImGui::Begin()
         indentSpacing = 22.0f;            // Horizontal spacing when e.g. entering a tree node
         columnsMinSpacing = 6.0f;             // Minimum horizontal spacing between two columns
         scrollbarSize = 16.0f;            // Width of the vertical scrollbar, Height of the horizontal scrollbar
         scrollbarRounding = 9.0f;             // Radius of grab corners rounding for scrollbar
         grabMinSize = 10.0f;            // Minimum width/height of a grab box for slider/scrollbar
         grabRounding = 0.0f;             // Radius of grabs corners rounding. Set to 0.0f to have rectangular slider grabs.
         displayWindowPadding = new Vector2(22, 22);    // Window positions are clamped to be visible within the display area by at least this amount. Only covers regular windows.
         displaySafeAreaPadding = new Vector2(4, 4);      // If you cannot see the edge of your screen (e.g. on a TV) increase the safe area padding. Covers popups/tooltips as well regular windows.
         antiAliasedLines = true;             // Enable anti-aliasing on lines/borders. Disable if you are really short on CPU/GPU.
         antiAliasedShapes = true;             // Enable anti-aliasing on filled shapes (rounded rectangles, circles, etc.)
         curveTesselationTolerance = 1.25f;            // Tessellation tolerance. Decrease for highly tessellated curves (higher quality, more polygons), increase to reduce quality.

         colors[(int)ElementColor.Text] = new Color4(0.90f, 0.90f, 0.90f, 1.00f);
         colors[(int)ElementColor.TextDisabled] = new Color4(0.60f, 0.60f, 0.60f, 1.00f);
         colors[(int)ElementColor.WindowBg] = new Color4(0.00f, 0.00f, 0.00f, 1.00f);
         colors[(int)ElementColor.ChildWindowBg] = new Color4(0.00f, 0.00f, 0.00f, 0.00f);
         colors[(int)ElementColor.Border] = new Color4(0.70f, 0.70f, 0.70f, 0.65f);
         colors[(int)ElementColor.BorderShadow] = new Color4(0.00f, 0.00f, 0.00f, 0.00f);
         colors[(int)ElementColor.FrameBg] = new Color4(0.80f, 0.80f, 0.80f, 0.30f);   // Background of checkbox, radio button, plot, slider, text input
         colors[(int)ElementColor.FrameBgHovered] = new Color4(0.90f, 0.80f, 0.80f, 0.40f);
         colors[(int)ElementColor.FrameBgActive] = new Color4(0.90f, 0.65f, 0.65f, 0.45f);
         colors[(int)ElementColor.TitleBg] = new Color4(0.50f, 0.50f, 1.00f, 0.45f);
         colors[(int)ElementColor.TitleBgCollapsed] = new Color4(0.40f, 0.40f, 0.80f, 0.20f);
         colors[(int)ElementColor.TitleBgActive] = new Color4(0.50f, 0.50f, 1.00f, 0.55f);
         colors[(int)ElementColor.MenuBarBg] = new Color4(0.40f, 0.40f, 0.55f, 0.80f);
         colors[(int)ElementColor.ScrollbarBg] = new Color4(0.20f, 0.25f, 0.30f, 0.60f);
         colors[(int)ElementColor.ScrollbarGrab] = new Color4(0.40f, 0.40f, 0.80f, 0.30f);
         colors[(int)ElementColor.ScrollbarGrabHovered] = new Color4(0.40f, 0.40f, 0.80f, 0.40f);
         colors[(int)ElementColor.ScrollbarGrabActive] = new Color4(0.80f, 0.50f, 0.50f, 0.40f);
         colors[(int)ElementColor.ComboBg] = new Color4(0.20f, 0.20f, 0.20f, 0.99f);
         colors[(int)ElementColor.CheckMark] = new Color4(0.90f, 0.90f, 0.90f, 0.50f);
         colors[(int)ElementColor.SliderGrab] = new Color4(1.00f, 1.00f, 1.00f, 0.30f);
         colors[(int)ElementColor.SliderGrabActive] = new Color4(0.80f, 0.50f, 0.50f, 1.00f);
         colors[(int)ElementColor.Button] = new Color4(0.67f, 0.40f, 0.40f, 0.60f);
         colors[(int)ElementColor.ButtonHovered] = new Color4(0.67f, 0.40f, 0.40f, 1.00f);
         colors[(int)ElementColor.ButtonActive] = new Color4(0.80f, 0.50f, 0.50f, 1.00f);
         colors[(int)ElementColor.Header] = new Color4(0.40f, 0.40f, 0.90f, 0.45f);
         colors[(int)ElementColor.HeaderHovered] = new Color4(0.45f, 0.45f, 0.90f, 0.80f);
         colors[(int)ElementColor.HeaderActive] = new Color4(0.53f, 0.53f, 0.87f, 0.80f);
         colors[(int)ElementColor.Column] = new Color4(0.50f, 0.50f, 0.50f, 1.00f);
         colors[(int)ElementColor.ColumnHovered] = new Color4(0.70f, 0.60f, 0.60f, 1.00f);
         colors[(int)ElementColor.ColumnActive] = new Color4(0.90f, 0.70f, 0.70f, 1.00f);
         colors[(int)ElementColor.ResizeGrip] = new Color4(1.00f, 1.00f, 1.00f, 0.30f);
         colors[(int)ElementColor.ResizeGripHovered] = new Color4(1.00f, 1.00f, 1.00f, 0.60f);
         colors[(int)ElementColor.ResizeGripActive] = new Color4(1.00f, 1.00f, 1.00f, 0.90f);
         colors[(int)ElementColor.CloseButton] = new Color4(0.50f, 0.50f, 0.90f, 0.50f);
         colors[(int)ElementColor.CloseButtonHovered] = new Color4(0.70f, 0.70f, 0.90f, 0.60f);
         colors[(int)ElementColor.CloseButtonActive] = new Color4(0.70f, 0.70f, 0.70f, 1.00f);
         colors[(int)ElementColor.PlotLines] = new Color4(1.00f, 1.00f, 1.00f, 1.00f);
         colors[(int)ElementColor.PlotLinesHovered] = new Color4(0.90f, 0.70f, 0.00f, 1.00f);
         colors[(int)ElementColor.PlotHistogram] = new Color4(0.90f, 0.70f, 0.00f, 1.00f);
         colors[(int)ElementColor.PlotHistogramHovered] = new Color4(1.00f, 0.60f, 0.00f, 1.00f);
         colors[(int)ElementColor.TextSelectedBg] = new Color4(0.00f, 0.00f, 1.00f, 0.35f);
         colors[(int)ElementColor.TooltipBg] = new Color4(0.05f, 0.05f, 0.10f, 0.90f);
         colors[(int)ElementColor.ModalWindowDarkening] = new Color4(0.20f, 0.20f, 0.20f, 0.35f);
      }

      public Color4 getColor(ElementColor elementType)
      {
         return colors[(int)elementType];
      }

      public Color4 getColor(ElementColor elementType, float alpha)
      {
         Color4 col = colors[(int)elementType];
         col.A = alpha;
         return col;
      }

      public void pushStyleVar(StyleVar var, float value)
      {
         StyleMod mod = new StyleMod();
         mod.style = var;
         switch (var)
         {
            case StyleVar.Alpha: mod.prevousValue.X = alpha; alpha = value; break;
            case StyleVar.WindowFillAlpha: mod.prevousValue.X = windowFillAlphaDefault; windowFillAlphaDefault = value; break;
            case StyleVar.WindowRounding: mod.prevousValue.X = windowRounding; windowRounding = value; break;
            case StyleVar.ChildWindowRounding: mod.prevousValue.X = childWindowRounding; childWindowRounding = value; break;
            case StyleVar.FrameRounding: mod.prevousValue.X = frameRounding; frameRounding = value; break;
            case StyleVar.IndentSpacing: mod.prevousValue.X = indentSpacing; indentSpacing = value; break;
            case StyleVar.GrabMinSize: mod.prevousValue.X = grabMinSize; grabMinSize = value; break;
            default: throw new Exception(String.Format("Can't find float var {0}", var));
         }

         styleStack.Push(mod);
      }

      public void pushStyleVar(StyleVar var, Vector2 value)
      {
         StyleMod mod = new StyleMod();
         mod.style = var;
         switch (var)
         {
            case StyleVar.WindowPadding: mod.prevousValue = windowPadding; windowPadding = value; break;
            case StyleVar.WindowMinSize: mod.prevousValue = windowMinSize; windowMinSize = value; break;
            case StyleVar.FramePadding: mod.prevousValue = framePadding; framePadding = value; break;
            case StyleVar.ItemSpacing: mod.prevousValue = itemSpacing; itemSpacing = value; break;
            case StyleVar.ItemInnerSpacing: mod.prevousValue = itemInnerSpacing; itemInnerSpacing = value; break;
            default: throw new Exception(String.Format("Can't find Vector2 var {0}", var));
         }

         styleStack.Push(mod);
      }

      public void popStyleVar(int count)
      {
         for(int i=0; i < count; i++)
         {
            StyleMod mod = styleStack.Pop();
            switch(mod.style)
            {
               case StyleVar.Alpha: alpha = mod.prevousValue.X; break;
               case StyleVar.WindowFillAlpha: windowFillAlphaDefault = mod.prevousValue.X; break;
               case StyleVar.WindowRounding: windowRounding = mod.prevousValue.X; break;
               case StyleVar.ChildWindowRounding: childWindowRounding = mod.prevousValue.X; break;
               case StyleVar.FrameRounding: frameRounding = mod.prevousValue.X; break;
               case StyleVar.IndentSpacing: indentSpacing = mod.prevousValue.X; break;
               case StyleVar.GrabMinSize: grabMinSize = mod.prevousValue.X; break;
               case StyleVar.WindowPadding: windowPadding = mod.prevousValue; break;
               case StyleVar.WindowMinSize: windowMinSize = mod.prevousValue; break;
               case StyleVar.FramePadding: framePadding = mod.prevousValue; break;
               case StyleVar.ItemSpacing: itemSpacing = mod.prevousValue; break;
               case StyleVar.ItemInnerSpacing: itemInnerSpacing = mod.prevousValue; break;
            }
         }
      }

      public Vector2 textSize(String text)
      {
         float fontSize = currentFontSize;
         //0.6 is since letter spacing is only 0.6 of the fontsize
         return new Vector2(fontSize * text.Length * 0.6f, fontSize);
      }
   }

   public class StyleStack
   {
      Stack<Style> myStyleStack=new Stack<Style>();
      Style myDefaultStyle=new Style();

      public StyleStack()
      {
      }
   }
}