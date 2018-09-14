using System;
using OpenTK.Input;

namespace Util
{
   public class KeyModifiers
   {
      int myModifiers = 0;
      public enum Modifiers
      {
         RSHIFT = 1 << 0,
         RCTRL = 1 << 1,
         RALT = 1 << 2,
         LSHIFT = 1 << 3,
         LCTRL = 1 << 4,
         LALT = 1 << 5,
         COMMMAND = 1 << 6,
         NUMLOCK = 1 << 7,
         CAPSLOCK = 1 << 8,
         LWIN = 1 << 9,
         RWIN = 1 << 10
      }

      public KeyModifiers()
      {
      }

      public void getModifiers(KeyboardState ks)
      {
         if (ks[Key.RShift]) myModifiers |= (int)Modifiers.RSHIFT;
         if (ks[Key.RControl]) myModifiers |= (int)Modifiers.RCTRL;
         if (ks[Key.RAlt]) myModifiers |= (int)Modifiers.RALT;
         if (ks[Key.LShift]) myModifiers |= (int)Modifiers.LSHIFT;
         if (ks[Key.LControl]) myModifiers |= (int)Modifiers.LCTRL;
         if (ks[Key.LAlt]) myModifiers |= (int)Modifiers.LALT;
         //if (ks[Key.Command]) myModifiers |= (int)Modifiers.COMMMAND;
         if (ks[Key.NumLock]) myModifiers |= (int)Modifiers.NUMLOCK;
         if (ks[Key.CapsLock]) myModifiers |= (int)Modifiers.CAPSLOCK;
         if (ks[Key.LWin]) myModifiers |= (int)Modifiers.LWIN;
         if (ks[Key.RWin]) myModifiers |= (int)Modifiers.RWIN;
      }

      public int modifiers
      {
         get { return myModifiers; }
         set { myModifiers = value; }
      }

      public bool shiftPressed()
      {
         return (myModifiers & (int)Modifiers.LSHIFT) != 0 ||
                (myModifiers & (int)Modifiers.RSHIFT) != 0;
      }

      public bool capitalize()
      {
         return shiftPressed() || (myModifiers & (int)Modifiers.CAPSLOCK) != 0;
      }

      public bool altPressed()
      {
         return (myModifiers & (int)Modifiers.LALT) != 0 ||
                (myModifiers & (int)Modifiers.RALT) != 0;
      }

      public bool controlPressed()
      {
         return (myModifiers & (int)Modifiers.LCTRL) != 0 ||
                (myModifiers & (int)Modifiers.RCTRL) != 0;
      }

      public bool empty()
      {
         return myModifiers == 0;
      }

      public Char unicodeFromKey(Key k)
      {
         Char ret = (char)0;
         switch (k)
         {
            case Key.Space: ret = ' '; break;
            case Key.A: ret = capitalize() == true ? 'A' : 'a'; break;
            case Key.B: ret = capitalize() == true ? 'B' : 'b'; break;
            case Key.C: ret = capitalize() == true ? 'C' : 'c'; break;
            case Key.D: ret = capitalize() == true ? 'D' : 'd'; break;
            case Key.E: ret = capitalize() == true ? 'E' : 'e'; break;
            case Key.F: ret = capitalize() == true ? 'F' : 'f'; break;
            case Key.G: ret = capitalize() == true ? 'G' : 'g'; break;
            case Key.H: ret = capitalize() == true ? 'H' : 'h'; break;
            case Key.I: ret = capitalize() == true ? 'I' : 'i'; break;
            case Key.J: ret = capitalize() == true ? 'J' : 'j'; break;
            case Key.K: ret = capitalize() == true ? 'K' : 'k'; break;
            case Key.L: ret = capitalize() == true ? 'L' : 'l'; break;
            case Key.M: ret = capitalize() == true ? 'M' : 'm'; break;
            case Key.N: ret = capitalize() == true ? 'N' : 'n'; break;
            case Key.O: ret = capitalize() == true ? 'O' : 'o'; break;
            case Key.P: ret = capitalize() == true ? 'P' : 'p'; break;
            case Key.Q: ret = capitalize() == true ? 'Q' : 'q'; break;
            case Key.R: ret = capitalize() == true ? 'R' : 'r'; break;
            case Key.S: ret = capitalize() == true ? 'S' : 's'; break;
            case Key.T: ret = capitalize() == true ? 'T' : 't'; break;
            case Key.U: ret = capitalize() == true ? 'U' : 'u'; break;
            case Key.V: ret = capitalize() == true ? 'V' : 'v'; break;
            case Key.W: ret = capitalize() == true ? 'W' : 'w'; break;
            case Key.X: ret = capitalize() == true ? 'X' : 'x'; break;
            case Key.Y: ret = capitalize() == true ? 'Y' : 'y'; break;
            case Key.Z: ret = capitalize() == true ? 'Z' : 'z'; break;
            case Key.Number1: ret = shiftPressed() == true ? '!' : '1'; break;
            case Key.Number2: ret = shiftPressed() == true ? '@' : '2'; break;
            case Key.Number3: ret = shiftPressed() == true ? '#' : '3'; break;
            case Key.Number4: ret = shiftPressed() == true ? '$' : '4'; break;
            case Key.Number5: ret = shiftPressed() == true ? '%' : '5'; break;
            case Key.Number6: ret = shiftPressed() == true ? '^' : '6'; break;
            case Key.Number7: ret = shiftPressed() == true ? '&' : '7'; break;
            case Key.Number8: ret = shiftPressed() == true ? '*' : '8'; break;
            case Key.Number9: ret = shiftPressed() == true ? '(' : '9'; break;
            case Key.Number0: ret = shiftPressed() == true ? ')' : '0'; break;
            case Key.Minus: ret = shiftPressed() == true ? '_' : '-'; break;
            case Key.Plus: ret = shiftPressed() == true ? '+' : '='; break;
            case Key.LBracket: ret = shiftPressed() == true ? '{' : '['; break;
            case Key.RBracket: ret = shiftPressed() == true ? '}' : ']'; break;
            case Key.Semicolon: ret = shiftPressed() == true ? ':' : ';'; break;
            case Key.Quote: ret = shiftPressed() == true ? '"' : '\''; break;
            case Key.Tilde: ret = shiftPressed() == true ? '~' : '`'; break;
            case Key.Comma: ret = shiftPressed() == true ? '<' : ','; break;
            case Key.Period: ret = shiftPressed() == true ? '>' : '.'; break;
            case Key.Slash: ret = shiftPressed() == true ? '?' : '/'; break;
            case Key.BackSlash: ret = shiftPressed() == true ? '|' : '\\'; break;
            case Key.Keypad0: ret = '0'; break;
            case Key.Keypad1: ret = '1'; break;
            case Key.Keypad2: ret = '2'; break;
            case Key.Keypad3: ret = '3'; break;
            case Key.Keypad4: ret = '4'; break;
            case Key.Keypad5: ret = '5'; break;
            case Key.Keypad6: ret = '6'; break;
            case Key.Keypad7: ret = '7'; break;
            case Key.Keypad8: ret = '8'; break;
            case Key.Keypad9: ret = '9'; break;
            case Key.KeypadAdd: ret = '+'; break;
            case Key.KeypadDecimal: ret = '.'; break;
            case Key.KeypadDivide: ret = '/'; break;
            case Key.KeypadMinus: ret = '-'; break;
            case Key.KeypadMultiply: ret = '*'; break;
            default: ret = (char)0; break;
         }

         return ret;
      }

      public static Key KeyfromUnicode(String uni)
      {
         switch (uni)
         {
            case "space": return Key.Space; 
            case "leftShift": return Key.ShiftLeft; 
            case "rightShift": return Key.ShiftRight; 
            case "leftCtrl": return Key.ControlLeft; 
            case "rightCtrl" : return Key.ControlRight; 
            case "leftAlt": return Key.AltLeft; 
            case "rightAlt" :return Key.AltRight; 
            case "escape": return Key.Escape; 
            case "backspace": return Key.BackSpace; 
            case "up": return Key.Up; 
            case "down": return Key.Down; 
            case "left": return Key.Left;
            case "right": return Key.Right;
            case "pageUp": return Key.PageUp; 
            case "pageDown": return Key.PageDown;
            case "home": return Key.Home;
            case "end": return Key.End;
            case "insert": return Key.Insert;
            case "delete": return Key.Delete;
            case "tab": return Key.Tab;

            case "a": return Key.A; 
            case "b": return Key.B; 
            case "c": return Key.C; 
            case "d": return Key.D; 
            case "e": return Key.E; 
            case "f": return Key.F; 
            case "g": return Key.G; 
            case "h": return Key.H; 
            case "i": return Key.I; 
            case "j": return Key.J; 
            case "k": return Key.K; 
            case "l": return Key.L; 
            case "m": return Key.M; 
            case "n": return Key.N; 
            case "o": return Key.O; 
            case "p": return Key.P; 
            case "q": return Key.Q; 
            case "r": return Key.R; 
            case "s": return Key.S; 
            case "t": return Key.T; 
            case "u": return Key.U; 
            case "v": return Key.V; 
            case "w": return Key.W; 
            case "x": return Key.X; 
            case "y": return Key.Y; 
            case "z": return Key.Z; 

            case "A": return Key.A;    
            case "B": return Key.B; 
            case "C": return Key.C; 
            case "D": return Key.D; 
            case "E": return Key.E; 
            case "F": return Key.F; 
            case "G": return Key.G; 
            case "H": return Key.H; 
            case "I": return Key.I; 
            case "J": return Key.J; 
            case "K": return Key.K; 
            case "L": return Key.L; 
            case "M": return Key.M; 
            case "N": return Key.N; 
            case "O": return Key.O; 
            case "P": return Key.P; 
            case "Q": return Key.Q; 
            case "R": return Key.R; 
            case "S": return Key.S; 
            case "T": return Key.T; 
            case "U": return Key.U; 
            case "V": return Key.V; 
            case "W": return Key.W; 
            case "X": return Key.X; 
            case "Y": return Key.Y; 
            case "Z": return Key.Z; 

            case "0": return Key.Number0; 
            case "1": return Key.Number1; 
            case "2": return Key.Number2; 
            case "3": return Key.Number3; 
            case "4": return Key.Number4; 
            case "5": return Key.Number5; 
            case "6": return Key.Number6; 
            case "7": return Key.Number7; 
            case "8": return Key.Number8; 
            case "9": return Key.Number9; 

            case "-": return Key.Minus; 
            case "+": return Key.Plus; 
            case "[": return Key.LBracket; 
            case "]": return Key.RBracket; 
            case ";": return Key.Semicolon; 
            case "'": return Key.Quote; 
            case "`": return Key.Tilde; 
            case ",": return Key.Comma; 
            case ".": return Key.Period; 
            case "/": return Key.Slash; 
            case "\\": return Key.BackSlash;

            case "F1": return Key.F1;
            case "F2": return Key.F2;
            case "F3": return Key.F3;
            case "F4": return Key.F4;
            case "F5": return Key.F5;
            case "F6": return Key.F6;
            case "F7": return Key.F7;
            case "F8": return Key.F8;
            case "F9": return Key.F9;
            case "F10": return Key.F10;
            case "F11": return Key.F11;
            case "F12": return Key.F12;
         }

         return Key.Unknown;
      }
   }

}