using System;
using System.Collections.Generic;

using Graphics;

using OpenTK;
using OpenTK.Graphics;

namespace GUI
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
   };


   public enum StyleColors
	{
		TEXT,
		WINDOW,
		HEADER,
		BORDER,
		BUTTON,
		BUTTON_HOVER,
		BUTTON_ACTIVE,
		TOGGLE,
		TOGGLE_HOVER,
		TOGGLE_CURSOR,
		SELECT,
		SELECT_ACTIVE,
		SLIDER,
		SLIDER_CURSOR,
		SLIDER_CURSOR_HOVER,
		SLIDER_CURSOR_ACTIVE,
		PROPERTY,
		EDIT,
		EDIT_CURSOR,
		COMBO,
		CHART,
		CHART_COLOR,
		CHART_COLOR_HIGHLIGHT,
		SCROLLBAR,
		SCROLLBAR_CURSOR,
		SCROLLBAR_CURSOR_HOVER,
		SCROLLBAR_CURSOR_ACTIVE,
		TAB_HEADER
	};

	public class StyleItem
	{
		public enum Type { COLOR, IMAGE, SPRITE, NINEPATCH};
		public Type type;
		public Color4 color;
		public Texture image;
      public Sprite sprite;
      public NinePatch patch;
	};

	public class StyleText
	{
		public Color4 color;
		public Color4 backgroundColor;
		public Vector2 padding;
	};

	public class StyleButton
	{
		// background 
		public StyleItem normal;
		public StyleItem hover;
		public StyleItem active;
		public Color4 borderColor;

		// text 
		public Color4 textBackground;
		public Color4 textNormal;
		public Color4 textHover;
		public Color4 textActive;
		public Alignment textAlignment;

		// properties 
		public float border;
		public float rounding;
		public Vector2 padding;
		public Vector2 imagePadding;
		public Vector2 touchPadding;
	};

	public class StyleToggle
	{
		// background 
		public StyleItem normal;
		public StyleItem hover;
		public StyleItem active;
		public Color4 borderColor;

		// cursor 
		public StyleItem cursorNormal;
		public StyleItem cursorHover;

		// text 
		public Color4 textBackground;
		public Color4 textNormal;
		public Color4 textHover;
		public Color4 textActive;
		public Alignment textAlignment;

		// properties 
		public float spacing;
		public float border;
		public Vector2 padding;
		public Vector2 touchPadding;
	};

	public class StyleSelectable
	{
		// background (inactive)
		public StyleItem normal;
		public StyleItem hover;
		public StyleItem pressed;

		// background (active)
		public StyleItem normalActive;
		public StyleItem hoverActive;
		public StyleItem pressedActive;

		// text color (inactive) 
		public Color4 textNormal;
		public Color4 textHover;
		public Color4 textPressed;

		// text color (active) 
		public Color4 textNormalActive;
		public Color4 textHoverActive;
		public Color4 textPressedActive;
		public Color4 textBackground;

		// properties 
		public float rounding;
		public Vector2 padding;
		public Vector2 imagePadding;
		public Vector2 touchPadding;
      public Alignment textAlignment;
   };

	public class StyleSlider
	{
		// background 
		public StyleItem normal;
		public StyleItem hover;
		public StyleItem active;
		public Color4 borderColor;

		// background bar 
		public Color4 barNormal;
		public Color4 barHover;
		public Color4 barActive;
		public Color4 barFilled;

		// cursor 
		public StyleItem cursorNormal;
		public StyleItem cursorHover;
		public StyleItem cursorActive;

		// properties 
		public float border;
		public float rounding;
		public float barHeight;
		public Vector2 padding;
		public Vector2 spacing;
		public Vector2 cursorSize;

		//option buttons
		public bool showButtons;
		public StyleButton incButton;
		public StyleButton decButton;
		public int incSymbol;
		public int decSymbol;
	};

	public class StyleProgress
	{
		// background 
		public StyleItem normal;
		public StyleItem hover;
		public StyleItem active;
		public Color4 borderColor;

		// cursor 
		public StyleItem cursorNormal;
		public StyleItem cursorHover;
		public StyleItem cursorActive;
		public Color4 cursorBorderColor;

		// properties 
		public float border;
		public float rounding;
		public float cursorBorder;
		public float cursorRounding;
		public Vector2 padding;
	};

	public class StyleScrollbar
	{
		// background 
		public StyleItem normal;
		public StyleItem hover;
		public StyleItem active;
		public Color4 borderColor;

		// cursor 
		public StyleItem cursorNormal;
		public StyleItem cursorHover;
		public StyleItem cursorActive;
		public Color4 cursorBorderColor;

		// properties 
		public float border;
		public float rounding;
		public float borderCursor;
		public float roundingCursor;
		public Vector2 padding;

		//option buttons
		public bool showButtons;
		public StyleButton incButton;
		public StyleButton decButton;
		public int incSymbol;
		public int decSymbol;
	};

	public class StyleEdit
	{
		// background 
		public StyleItem normal;
		public StyleItem hover;
		public StyleItem active;
		public Color4 borderColor;
		public StyleScrollbar scrollbar;

		//cursor
		public Color4 cursorNormal;
		public Color4 cursorHover;
		public Color4 cursorTextNormal;
		public Color4 cursorTextHover;

		//text (unselected)
		public Color4 textNormal;
		public Color4 textHover;
		public Color4 textActive;

		//text (selected)
		public Color4 selectedNormal;
		public Color4 selectedHover;
		public Color4 selectedTextNormal;
		public Color4 selectedTextHover;

		// properties 
		public float border;
		public float rounding;
		public float cursorSize;
		public float rowPadding;
		public Vector2 scrollbarSize;
		public Vector2 padding;
	};

	public class StyleProperty
	{
		// background 
		public StyleItem normal;
		public StyleItem hover;
		public StyleItem active;
		public Color4 borderColor;

		//text
		public Color4 labelNormal;
		public Color4 labelHover;
		public Color4 labelActive;

		//symbols
		public int symLeft;
		public int symRight;

		// properties 
		public float border;
		public float rounding;
		public Vector2 padding;

		public StyleEdit edit;
		public StyleButton incButton;
		public StyleButton decButton;
	};

	public class StyleChart
	{
		//colors
		public StyleItem background;
		public Color4 borderColor;
		public Color4 selectedColor;
		public Color4 color;

		//properties
		public float border;
		public float rounding;
		public Vector2 padding;
	};

	public class StyleCombo
	{
		// background 
		public StyleItem normal;
		public StyleItem hover;
		public StyleItem active;
		public Color4 borderColor;

		//label
		public Color4 labelNormal;
		public Color4 labelHover;
		public Color4 labelActive;

		//symbol
		public Color4 symbolNormal;
		public Color4 symbolHover;
		public Color4 symbolActive;

		//button
		public StyleButton button;
		public int symNormal;
		public int symHover;
		public int symActive;

		//properties
		public float border;
		public float rounding;
		public Vector2 contentPadding;
		public Vector2 buttonPadding;
		public Vector2 spacing;
	};

	public class StyleTab
	{
		//background
		public StyleItem background;
		public Color4 borderColor;
		public Color4 text;

		//button
		public StyleButton tabMaximizeButton;
		public StyleButton tabMinimizeButton;
		public StyleButton nodeMaximizeButton;
		public StyleButton nodeMinimizeButton;
		public int symMinimize;
		public int symMaximize;

		//properties
		public float border;
		public float rounding;
		public float indent;
		public Vector2 padding;
		public Vector2 spacing;
	};

	public class StyleWindowHeader
	{
		//background
		public StyleItem normal;
		public StyleItem hover;
		public StyleItem active;

		//button
		public StyleButton closeButton;
		public StyleButton minimizeButton;
		public int closeSymbol;
		public int minimizeSymbol;
		public int maximizeSymbol;

		//title
		public Color4 labelNormal;
		public Color4 labelHover;
		public Color4 labelActive;

		//properties
		public HeaderAlign align;
		public Vector2 padding;
		public Vector2 labelPadding;
		public Vector2 spacing;
	};

	public class StyleWindow
	{
		public StyleWindowHeader header= new StyleWindowHeader();
		public StyleItem background;
      public StyleItem border;

      public Color4 backgroundColor;
		public Color4 borderColor;
		public Color4 groupBorderColor;
		public Color4 popupBorderColor;
		public Color4 comboBorderColor;
		public Color4 contextualBorderColor;
		public Color4 menuBorderColor;
		public Color4 tooltipBorderColor;
		public StyleItem scaler;

		public float groupBorder;
		public float popupBorder;
		public float comboBorder;
		public float contextualBorder;
		public float menuBorder;
		public float tooltipBorder;

		public float rounding;
		public Vector2 spacing;
		public Vector2 scrollbarSize;
		public Vector2 minSize;

		public Vector2 padding;
		public Vector2 groupPadding;
		public Vector2 popupPadding;
		public Vector2 comboPadding;
		public Vector2 contextualPadding;
		public Vector2 menuPadding;
		public Vector2 tooltipPadding;
	};

	public class Style
	{
		public Font font;
		public Cursor activeCursor;
		public Cursor lastCursor;
		public bool cursorVisible;

		public StyleText text;
		public StyleButton button;
		public StyleButton contextualButton;
		public StyleButton menuButton;
		public StyleToggle option;
		public StyleToggle checkbox;
		public StyleSelectable selectable;
		public StyleSlider slider;
		public StyleProgress progress;
		public StyleProperty property;
		public StyleEdit edit;
		public StyleChart chart;
		public StyleScrollbar scrollH;
		public StyleScrollbar scrollV;
		public StyleTab tab;
		public StyleCombo combo;
		public StyleWindow window;

		public static Color4[] defaultColors =
		{
			new Color4(175,175,175,255),
			new Color4(45, 45, 45, 255),
			new Color4(40, 40, 40, 255),
			new Color4(65, 65, 65, 255),
			new Color4(50, 50, 50, 255),
			new Color4(40, 40, 40, 255),
			new Color4(35, 35, 35, 255),
			new Color4(100,100,100,255),
			new Color4(120,120,120,255),
			new Color4(45, 45, 45, 255),
			new Color4(45, 45, 45, 255),
			new Color4(35, 35, 35,255) ,
			new Color4(38, 38, 38, 255),
			new Color4(100,100,100,255),
			new Color4(120,120,120,255),
			new Color4(150,150,150,255),
			new Color4(38, 38, 38, 255),
			new Color4(38, 38, 38, 255),
			new Color4(175,175,175,255),
			new Color4(45, 45, 45, 255),
			new Color4(120,120,120,255),
			new Color4(45, 45, 45, 255),
			new Color4(255, 0,  0, 255),
			new Color4(40, 40, 40, 255),
			new Color4(100,100,100,255),
			new Color4(120,120,120,255),
			new Color4(150,150,150,255),
			new Color4(40, 40, 40,255)
		};
	};

	public class StyleStacks
	{
		public Stack<StyleItem> styleItems = new Stack<StyleItem>(8);
		public Stack<float>  floats= new Stack<float>(32);
		public Stack<Vector2> vectors = new Stack<Vector2>(16);
		public Stack<UInt32> flags = new Stack<UInt32>(32);
		public Stack<Color4> colors = new Stack<Color4>(32);
		public Stack<Font> fonts = new Stack<Font>(8);
		public Stack<StyleItem> buttonBehaviors = new Stack<StyleItem>(8);
	};

	public static partial class UI
	{
		#region Style
		public static StyleItem styleItemColor(Color4 color)
		{
			StyleItem i = new StyleItem();
			i.type = StyleItem.Type.COLOR;
			i.color = color;
			return i;
		}

		public static StyleItem styleItemHide()
		{
			StyleItem i = new StyleItem();
			i.type = StyleItem.Type.COLOR;
			i.color = new Color4(0,0,0,0);
			return i;
		}

		public static StyleItem styleItemImage(Texture t)
		{
			StyleItem i = new StyleItem();
			i.type = StyleItem.Type.IMAGE;
			i.image = t;
			return i;
		}

      public static StyleItem styleItemSprite(Sprite s)
      {
         StyleItem i = new StyleItem();
         i.type = StyleItem.Type.SPRITE;
         i.sprite = s;
         return i;
      }

      public static StyleItem styleItemNinePatch(NinePatch p)
      {
         StyleItem i = new StyleItem();
         i.type = StyleItem.Type.NINEPATCH;
         i.patch = p;
         return i;
      }
      public static void setDefaultStyle()
		{
			setStyleFromTable(Style.defaultColors);
		}

      public static void setStyleFromTable(Color4[] table)
      {
         Style style = UI.style;
         style.font = FontManager.findFont("SANS");

         // default text 
         StyleText text = new StyleText();
         text.color = table[(int)StyleColors.TEXT];
         text.padding = Vector2.Zero;
         style.text = text;

         // default button 
         StyleButton button = new StyleButton();
         button.normal = styleItemColor(table[(int)StyleColors.BUTTON]);
         button.hover = styleItemColor(table[(int)StyleColors.BUTTON_HOVER]);
         button.active = styleItemColor(table[(int)StyleColors.BUTTON_ACTIVE]);
         button.borderColor = table[(int)StyleColors.BORDER];
         button.textBackground = table[(int)StyleColors.BUTTON];
         button.textNormal = table[(int)StyleColors.TEXT];
         button.textHover = table[(int)StyleColors.TEXT];
         button.textActive = table[(int)StyleColors.TEXT];
         button.padding = new Vector2(2.0f, 2.0f);
         button.imagePadding = new Vector2(0.0f, 0.0f);
         button.touchPadding = new Vector2(0.0f, 0.0f);
         button.textAlignment = Alignment.Middle;
         button.border = 1.0f;
         button.rounding = 4.0f;
         style.button = button;

         // contextual button 
         button = new StyleButton();
         button.normal = styleItemColor(table[(int)StyleColors.WINDOW]);
         button.hover = styleItemColor(table[(int)StyleColors.BUTTON_HOVER]);
         button.active = styleItemColor(table[(int)StyleColors.BUTTON_ACTIVE]);
         button.borderColor = table[(int)StyleColors.WINDOW];
         button.textBackground = table[(int)StyleColors.WINDOW];
         button.textNormal = table[(int)StyleColors.TEXT];
         button.textHover = table[(int)StyleColors.TEXT];
         button.textActive = table[(int)StyleColors.TEXT];
         button.padding = new Vector2(2.0f, 2.0f);
         button.touchPadding = new Vector2(0.0f, 0.0f);
         button.textAlignment = Alignment.Middle;
         button.border = 0.0f;
         button.rounding = 0.0f;
         style.contextualButton = button;

         // menu button 
         button = new StyleButton();
         button.normal = styleItemColor(table[(int)StyleColors.WINDOW]);
         button.hover = styleItemColor(table[(int)StyleColors.WINDOW]);
         button.active = styleItemColor(table[(int)StyleColors.WINDOW]);
         button.borderColor = table[(int)StyleColors.WINDOW];
         button.textBackground = table[(int)StyleColors.WINDOW];
         button.textNormal = table[(int)StyleColors.TEXT];
         button.textHover = table[(int)StyleColors.TEXT];
         button.textActive = table[(int)StyleColors.TEXT];
         button.padding = new Vector2(2.0f, 2.0f);
         button.touchPadding = new Vector2(0.0f, 0.0f);
         button.textAlignment = Alignment.Middle;
         button.border = 0.0f;
         button.rounding = 1.0f;
         style.menuButton = button;

         // checkbox toggle 
         StyleToggle toggle = new StyleToggle();
         toggle.normal = styleItemColor(table[(int)StyleColors.TOGGLE]);
         toggle.hover = styleItemColor(table[(int)StyleColors.TOGGLE_HOVER]);
         toggle.active = styleItemColor(table[(int)StyleColors.TOGGLE_HOVER]);
         toggle.cursorNormal = styleItemColor(table[(int)StyleColors.TOGGLE_CURSOR]);
         toggle.cursorHover = styleItemColor(table[(int)StyleColors.TOGGLE_CURSOR]);
         toggle.textBackground = table[(int)StyleColors.WINDOW];
         toggle.textNormal = table[(int)StyleColors.TEXT];
         toggle.textHover = table[(int)StyleColors.TEXT];
         toggle.textActive = table[(int)StyleColors.TEXT];
         toggle.padding = new Vector2(2.0f, 2.0f);
         toggle.touchPadding = new Vector2(0, 0);
         toggle.borderColor = new Color4(0, 0, 0, 0);
         toggle.border = 0.0f;
         toggle.spacing = 4;
         style.checkbox = toggle;

         // option toggle 
         toggle = new StyleToggle();
         toggle.normal = styleItemColor(table[(int)StyleColors.TOGGLE]);
         toggle.hover = styleItemColor(table[(int)StyleColors.TOGGLE_HOVER]);
         toggle.active = styleItemColor(table[(int)StyleColors.TOGGLE_HOVER]);
         toggle.cursorNormal = styleItemColor(table[(int)StyleColors.TOGGLE_CURSOR]);
         toggle.cursorHover = styleItemColor(table[(int)StyleColors.TOGGLE_CURSOR]);
         toggle.textBackground = table[(int)StyleColors.WINDOW];
         toggle.textNormal = table[(int)StyleColors.TEXT];
         toggle.textHover = table[(int)StyleColors.TEXT];
         toggle.textActive = table[(int)StyleColors.TEXT];
         toggle.padding = new Vector2(3.0f, 3.0f);
         toggle.touchPadding = new Vector2(0, 0);
         toggle.borderColor = new Color4(0, 0, 0, 0);
         toggle.border = 0.0f;
         toggle.spacing = 4;
         style.option = toggle;

         // selectable 
         StyleSelectable select = new StyleSelectable();
         select.normal = styleItemColor(table[(int)StyleColors.SELECT]);
         select.hover = styleItemColor(table[(int)StyleColors.SELECT]);
         select.pressed = styleItemColor(table[(int)StyleColors.SELECT]);
         select.normalActive = styleItemColor(table[(int)StyleColors.SELECT_ACTIVE]);
         select.hoverActive = styleItemColor(table[(int)StyleColors.SELECT_ACTIVE]);
         select.pressedActive = styleItemColor(table[(int)StyleColors.SELECT_ACTIVE]);
         select.textNormal = table[(int)StyleColors.TEXT];
         select.textHover = table[(int)StyleColors.TEXT];
         select.textPressed = table[(int)StyleColors.TEXT];
         select.textNormalActive = table[(int)StyleColors.TEXT];
         select.textHoverActive = table[(int)StyleColors.TEXT];
         select.textPressedActive = table[(int)StyleColors.TEXT];
         select.padding = new Vector2(4.0f, 4.0f);
         select.touchPadding = new Vector2(0, 0);
         select.rounding = 0.0f;
         select.textAlignment = Alignment.Middle;
         style.selectable = select;

         // slider 
         StyleSlider slider = new StyleSlider();
         slider.normal = styleItemHide();
         slider.hover = styleItemHide();
         slider.active = styleItemHide();
         slider.barNormal = table[(int)StyleColors.SLIDER];
         slider.barHover = table[(int)StyleColors.SLIDER];
         slider.barActive = table[(int)StyleColors.SLIDER];
         slider.barFilled = table[(int)StyleColors.SLIDER_CURSOR];
         slider.cursorNormal = styleItemColor(table[(int)StyleColors.SLIDER_CURSOR]);
         slider.cursorHover = styleItemColor(table[(int)StyleColors.SLIDER_CURSOR_HOVER]);
         slider.cursorActive = styleItemColor(table[(int)StyleColors.SLIDER_CURSOR_ACTIVE]);
         slider.incSymbol = Icons.TRIANGLE_RIGHT;
         slider.decSymbol = Icons.TRIANGLE_LEFT;
         slider.cursorSize = new Vector2(16, 16);
         slider.padding = new Vector2(2, 2);
         slider.spacing = new Vector2(2, 2);
         slider.showButtons = false;
         slider.barHeight = 8;
         slider.rounding = 0;
         style.slider = slider;

         // slider buttons 
         button = new StyleButton();
         button.normal = styleItemColor(new Color4(40, 40, 40, 255));
         button.hover = styleItemColor(new Color4(42, 42, 42, 255));
         button.active = styleItemColor(new Color4(44, 44, 44, 255));
         button.borderColor = new Color4(65, 65, 65, 255);
         button.textBackground = new Color4(40, 40, 40, 255);
         button.textNormal = new Color4(175, 175, 175, 255);
         button.textHover = new Color4(175, 175, 175, 255);
         button.textActive = new Color4(175, 175, 175, 255);
         button.padding = new Vector2(8.0f, 8.0f);
         button.touchPadding = new Vector2(0.0f, 0.0f);
         button.textAlignment = Alignment.Middle;
         button.border = 1.0f;
         button.rounding = 0.0f;
         style.slider.incButton = button;
         style.slider.decButton = button;

         // progressbar 
         StyleProgress prog = new StyleProgress();
         prog.normal = styleItemColor(table[(int)StyleColors.SLIDER]);
         prog.hover = styleItemColor(table[(int)StyleColors.SLIDER]);
         prog.active = styleItemColor(table[(int)StyleColors.SLIDER]);
         prog.cursorNormal = styleItemColor(table[(int)StyleColors.SLIDER_CURSOR]);
         prog.cursorHover = styleItemColor(table[(int)StyleColors.SLIDER_CURSOR_HOVER]);
         prog.cursorActive = styleItemColor(table[(int)StyleColors.SLIDER_CURSOR_ACTIVE]);
         prog.borderColor = new Color4(0, 0, 0, 0);
         prog.cursorBorderColor = new Color4(0, 0, 0, 0);
         prog.padding = new Vector2(4, 4);
         prog.rounding = 0;
         prog.border = 0;
         prog.cursorRounding = 0;
         prog.cursorBorder = 0;
         style.progress = prog;

         // scrollbars 
         StyleScrollbar scroll = new StyleScrollbar();
         scroll.normal = styleItemColor(table[(int)StyleColors.SCROLLBAR]);
         scroll.hover = styleItemColor(table[(int)StyleColors.SCROLLBAR]);
         scroll.active = styleItemColor(table[(int)StyleColors.SCROLLBAR]);
         scroll.cursorNormal = styleItemColor(table[(int)StyleColors.SCROLLBAR_CURSOR]);
         scroll.cursorHover = styleItemColor(table[(int)StyleColors.SCROLLBAR_CURSOR_HOVER]);
         scroll.cursorActive = styleItemColor(table[(int)StyleColors.SCROLLBAR_CURSOR_ACTIVE]);
         scroll.decSymbol = Icons.CIRCLE_SOLID;
         scroll.incSymbol = Icons.CIRCLE_SOLID;
         scroll.borderColor = table[(int)StyleColors.SCROLLBAR];
         scroll.cursorBorderColor = table[(int)StyleColors.SCROLLBAR];
         scroll.padding = new Vector2(0, 0);
         scroll.showButtons = false;
         scroll.border = 0;
         scroll.rounding = 0;
         scroll.borderCursor = 0;
         scroll.roundingCursor = 0;
         style.scrollH = scroll;
         style.scrollV = scroll;

         // scrollbars buttons 
         button = new StyleButton();
         button.normal = styleItemColor(new Color4(40, 40, 40, 255));
         button.hover = styleItemColor(new Color4(42, 42, 42, 255));
         button.active = styleItemColor(new Color4(44, 44, 44, 255));
         button.borderColor = new Color4(65, 65, 65, 255);
         button.textBackground = new Color4(40, 40, 40, 255);
         button.textNormal = new Color4(175, 175, 175, 255);
         button.textHover = new Color4(175, 175, 175, 255);
         button.textActive = new Color4(175, 175, 175, 255);
         button.padding = new Vector2(4.0f, 4.0f);
         button.touchPadding = new Vector2(0.0f, 0.0f);
         button.textAlignment = Alignment.Middle;
         button.border = 1.0f;
         button.rounding = 0.0f;
         style.scrollH.incButton = button;
         style.scrollH.decButton = button;
         style.scrollV.incButton = button;
         style.scrollV.decButton = button;

         // edit 
         StyleEdit edit = new StyleEdit();
         edit.normal = styleItemColor(table[(int)StyleColors.EDIT]);
         edit.hover = styleItemColor(table[(int)StyleColors.EDIT]);
         edit.active = styleItemColor(table[(int)StyleColors.EDIT]);
         edit.cursorNormal = table[(int)StyleColors.TEXT];
         edit.cursorHover = table[(int)StyleColors.TEXT];
         edit.cursorTextNormal = table[(int)StyleColors.EDIT];
         edit.cursorTextHover = table[(int)StyleColors.EDIT];
         edit.borderColor = table[(int)StyleColors.BORDER];
         edit.textNormal = table[(int)StyleColors.TEXT];
         edit.textHover = table[(int)StyleColors.TEXT];
         edit.textActive = table[(int)StyleColors.TEXT];
         edit.selectedNormal = table[(int)StyleColors.TEXT];
         edit.selectedHover = table[(int)StyleColors.TEXT];
         edit.selectedTextNormal = table[(int)StyleColors.EDIT];
         edit.selectedTextHover = table[(int)StyleColors.EDIT];
         edit.scrollbarSize = new Vector2(10, 10);
         edit.scrollbar = style.scrollV;
         edit.padding = new Vector2(4, 4);
         edit.rowPadding = 2;
         edit.cursorSize = 4;
         edit.border = 1;
         edit.rounding = 0;
         style.edit = edit;

         // property 
         StyleProperty property = new StyleProperty();
         property.normal = styleItemColor(table[(int)StyleColors.PROPERTY]);
         property.hover = styleItemColor(table[(int)StyleColors.PROPERTY]);
         property.active = styleItemColor(table[(int)StyleColors.PROPERTY]);
         property.borderColor = table[(int)StyleColors.BORDER];
         property.labelNormal = table[(int)StyleColors.TEXT];
         property.labelHover = table[(int)StyleColors.TEXT];
         property.labelActive = table[(int)StyleColors.TEXT];
         property.symLeft = Icons.TRIANGLE_LEFT;
         property.symRight = Icons.TRIANGLE_RIGHT;
         property.padding = new Vector2(4, 4);
         property.border = 1;
         property.rounding = 10;
         style.property = property;

         // property buttons 
         button = new StyleButton();
         button.normal = styleItemColor(table[(int)StyleColors.PROPERTY]);
         button.hover = styleItemColor(table[(int)StyleColors.PROPERTY]);
         button.active = styleItemColor(table[(int)StyleColors.PROPERTY]);
         button.borderColor = new Color4(0, 0, 0, 0);
         button.textBackground = table[(int)StyleColors.PROPERTY];
         button.textNormal = table[(int)StyleColors.TEXT];
         button.textHover = table[(int)StyleColors.TEXT];
         button.textActive = table[(int)StyleColors.TEXT];
         button.padding = new Vector2(0.0f, 0.0f);
         button.touchPadding = new Vector2(0.0f, 0.0f);
         button.textAlignment = Alignment.Middle;
         button.border = 0.0f;
         button.rounding = 0.0f;
         style.property.decButton = button;
         style.property.incButton = button;

         // property edit 
         edit = new StyleEdit();
         edit.normal = styleItemColor(table[(int)StyleColors.PROPERTY]);
         edit.hover = styleItemColor(table[(int)StyleColors.PROPERTY]);
         edit.active = styleItemColor(table[(int)StyleColors.PROPERTY]);
         edit.borderColor = new Color4(0, 0, 0, 0);
         edit.cursorNormal = table[(int)StyleColors.TEXT];
         edit.cursorHover = table[(int)StyleColors.TEXT];
         edit.cursorTextNormal = table[(int)StyleColors.EDIT];
         edit.cursorTextHover = table[(int)StyleColors.EDIT];
         edit.textNormal = table[(int)StyleColors.TEXT];
         edit.textHover = table[(int)StyleColors.TEXT];
         edit.textActive = table[(int)StyleColors.TEXT];
         edit.selectedNormal = table[(int)StyleColors.TEXT];
         edit.selectedHover = table[(int)StyleColors.TEXT];
         edit.selectedTextNormal = table[(int)StyleColors.EDIT];
         edit.selectedTextHover = table[(int)StyleColors.EDIT];
         edit.padding = new Vector2(0, 0);
         edit.cursorSize = 8;
         edit.border = 0;
         edit.rounding = 0;
         style.property.edit = edit;

         // chart 
         StyleChart chart = new StyleChart();
         chart.background = styleItemColor(table[(int)StyleColors.CHART]);
         chart.borderColor = table[(int)StyleColors.BORDER];
         chart.selectedColor = table[(int)StyleColors.CHART_COLOR_HIGHLIGHT];
         chart.color = table[(int)StyleColors.CHART_COLOR];
         chart.padding = new Vector2(4, 4);
         chart.border = 0;
         chart.rounding = 0;
         style.chart = chart;

         // combo 
         StyleCombo combo = new StyleCombo();
         combo.normal = styleItemColor(table[(int)StyleColors.COMBO]);
         combo.hover = styleItemColor(table[(int)StyleColors.COMBO]);
         combo.active = styleItemColor(table[(int)StyleColors.COMBO]);
         combo.borderColor = table[(int)StyleColors.BORDER];
         combo.labelNormal = table[(int)StyleColors.TEXT];
         combo.labelHover = table[(int)StyleColors.TEXT];
         combo.labelActive = table[(int)StyleColors.TEXT];
         combo.symNormal = Icons.TRIANGLE_DOWN;
         combo.symHover = Icons.TRIANGLE_DOWN;
         combo.symActive = Icons.TRIANGLE_DOWN;
         combo.contentPadding = new Vector2(4, 4);
         combo.buttonPadding = new Vector2(0, 4);
         combo.spacing = new Vector2(4, 0);
         combo.border = 1;
         combo.rounding = 0;
         style.combo = combo;

         // combo button 
         button = new StyleButton();
         button.normal = styleItemColor(table[(int)StyleColors.COMBO]);
         button.hover = styleItemColor(table[(int)StyleColors.COMBO]);
         button.active = styleItemColor(table[(int)StyleColors.COMBO]);
         button.borderColor = new Color4(0, 0, 0, 0);
         button.textBackground = table[(int)StyleColors.COMBO];
         button.textNormal = table[(int)StyleColors.TEXT];
         button.textHover = table[(int)StyleColors.TEXT];
         button.textActive = table[(int)StyleColors.TEXT];
         button.padding = new Vector2(2.0f, 2.0f);
         button.touchPadding = new Vector2(0.0f, 0.0f);
         button.textAlignment = Alignment.Middle;
         button.border = 0.0f;
         button.rounding = 0.0f;
         style.combo.button = button;

         // tab 
         StyleTab tab = new StyleTab();
         tab.background = styleItemColor(table[(int)StyleColors.TAB_HEADER]);
         tab.borderColor = table[(int)StyleColors.BORDER];
         tab.text = table[(int)StyleColors.TEXT];
         tab.symMinimize = Icons.TRIANGLE_RIGHT;
         tab.symMaximize = Icons.TRIANGLE_DOWN;
         tab.padding = new Vector2(4, 4);
         tab.spacing = new Vector2(4, 4);
         tab.indent = 10.0f;
         tab.border = 1;
         tab.rounding = 0;
         style.tab = tab;

         // tab button 
         button = new StyleButton();
         button.normal = styleItemColor(table[(int)StyleColors.TAB_HEADER]);
         button.hover = styleItemColor(table[(int)StyleColors.TAB_HEADER]);
         button.active = styleItemColor(table[(int)StyleColors.TAB_HEADER]);
         button.borderColor = new Color4(0, 0, 0, 0);
         button.textBackground = table[(int)StyleColors.TAB_HEADER];
         button.textNormal = table[(int)StyleColors.TEXT];
         button.textHover = table[(int)StyleColors.TEXT];
         button.textActive = table[(int)StyleColors.TEXT];
         button.padding = new Vector2(2.0f, 2.0f);
         button.touchPadding = new Vector2(0.0f, 0.0f);
         button.textAlignment = Alignment.Middle;
         button.border = 0.0f;
         button.rounding = 0.0f;
         style.tab.tabMinimizeButton = button;
         style.tab.tabMaximizeButton = button;

         // node button 
         button = new StyleButton();
         button.normal = styleItemColor(table[(int)StyleColors.WINDOW]);
         button.hover = styleItemColor(table[(int)StyleColors.WINDOW]);
         button.active = styleItemColor(table[(int)StyleColors.WINDOW]);
         button.borderColor = new Color4(0, 0, 0, 0);
         button.textBackground = table[(int)StyleColors.TAB_HEADER];
         button.textNormal = table[(int)StyleColors.TEXT];
         button.textHover = table[(int)StyleColors.TEXT];
         button.textActive = table[(int)StyleColors.TEXT];
         button.padding = new Vector2(2.0f, 2.0f);
         button.touchPadding = new Vector2(0.0f, 0.0f);
         button.textAlignment = Alignment.Middle;
         button.border = 0.0f;
         button.rounding = 0.0f;
         style.tab.nodeMinimizeButton = button;
         style.tab.nodeMaximizeButton = button;

         // window header 
         StyleWindow win = new StyleWindow();
         win.header.align = HeaderAlign.RIGHT;
         win.header.closeSymbol = Icons.X;
         win.header.minimizeSymbol = Icons.MINUS;
         win.header.maximizeSymbol = Icons.PLUS;
         win.header.normal = styleItemColor(table[(int)StyleColors.HEADER]);
         win.header.hover = styleItemColor(table[(int)StyleColors.HEADER]);
         win.header.active = styleItemColor(table[(int)StyleColors.HEADER]);
         win.header.labelNormal = table[(int)StyleColors.TEXT];
         win.header.labelHover = table[(int)StyleColors.TEXT];
         win.header.labelActive = table[(int)StyleColors.TEXT];
         win.header.labelPadding = new Vector2(4, 4);
         win.header.padding = new Vector2(4, 4);
         win.header.spacing = new Vector2(0, 0);
         style.window = win;

         // window header close button 
         button = new StyleButton();
         button.normal = styleItemColor(table[(int)StyleColors.HEADER]);
         button.hover = styleItemColor(table[(int)StyleColors.HEADER]);
         button.active = styleItemColor(table[(int)StyleColors.HEADER]);
         button.borderColor = new Color4(0, 0, 0, 0);
         button.textBackground = table[(int)StyleColors.HEADER];
         button.textNormal = table[(int)StyleColors.TEXT];
         button.textHover = table[(int)StyleColors.TEXT];
         button.textActive = table[(int)StyleColors.TEXT];
         button.padding = new Vector2(0.0f, 0.0f);
         button.touchPadding = new Vector2(0.0f, 0.0f);
         button.textAlignment = Alignment.Middle;
         button.border = 0.0f;
         button.rounding = 0.0f;
         style.window.header.closeButton = button;

         // window header minimize button 
         button = new StyleButton();
         button.normal = styleItemColor(table[(int)StyleColors.HEADER]);
         button.hover = styleItemColor(table[(int)StyleColors.HEADER]);
         button.active = styleItemColor(table[(int)StyleColors.HEADER]);
         button.borderColor = new Color4(0, 0, 0, 0);
         button.textBackground = table[(int)StyleColors.HEADER];
         button.textNormal = table[(int)StyleColors.TEXT];
         button.textHover = table[(int)StyleColors.TEXT];
         button.textActive = table[(int)StyleColors.TEXT];
         button.padding = new Vector2(0.0f, 0.0f);
         button.touchPadding = new Vector2(0.0f, 0.0f);
         button.textAlignment = Alignment.Middle;
         button.border = 0.0f;
         button.rounding = 0.0f;
         style.window.header.minimizeButton = button;

         // window 
         win.backgroundColor = table[(int)StyleColors.WINDOW];
         win.background = styleItemColor(table[(int)StyleColors.WINDOW]);
         win.borderColor = Color4.Red; // table[(int)StyleColors.BORDER];
         win.border = styleItemColor(table[(int)StyleColors.BORDER]);
         win.popupBorderColor = table[(int)StyleColors.BORDER];
         win.comboBorderColor = table[(int)StyleColors.BORDER];
         win.contextualBorderColor = table[(int)StyleColors.BORDER];
         win.menuBorderColor = table[(int)StyleColors.BORDER];
         win.groupBorderColor = table[(int)StyleColors.BORDER];
         win.tooltipBorderColor = table[(int)StyleColors.BORDER];
         win.scaler = styleItemColor(table[(int)StyleColors.TEXT]);

         win.rounding = 4.0f;
         win.spacing = new Vector2(4, 4);
         win.scrollbarSize = new Vector2(10, 10);
         win.minSize = new Vector2(64, 64);

         win.comboBorder = 2.0f;
         win.contextualBorder = 2.0f;
         win.menuBorder = 2.0f;
         win.groupBorder = 2.0f;
         win.tooltipBorder = 2.0f;
         win.popupBorder = 2.0f;

         win.padding = new Vector2(4, 4);
         win.groupPadding = new Vector2(4, 4);
         win.popupPadding = new Vector2(4, 4);
         win.comboPadding = new Vector2(4, 4);
         win.contextualPadding = new Vector2(4, 4);
         win.menuPadding = new Vector2(4, 4);
         win.tooltipPadding = new Vector2(4, 4);
      }

		#endregion
	}
}