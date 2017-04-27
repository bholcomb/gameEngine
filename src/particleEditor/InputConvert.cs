using System;
using System.Windows;

namespace ParticleEditor
{
   public static class InputConvert
   {
      public static OpenTK.Input.MouseButton convert(System.Windows.Forms.MouseButtons button)
      {
         OpenTK.Input.MouseButton b = OpenTK.Input.MouseButton.LastButton;
         switch (button)
         {
            case System.Windows.Forms.MouseButtons.Left:
               b = OpenTK.Input.MouseButton.Left;
               break;
            case System.Windows.Forms.MouseButtons.Middle:
               b = OpenTK.Input.MouseButton.Middle;
               break;
            case System.Windows.Forms.MouseButtons.Right:
               b = OpenTK.Input.MouseButton.Right;
               break;
         }

         return b;
      }

      public static OpenTK.Input.Key convert(System.Windows.Forms.Keys key)
      {
         switch (key)
         {
            case System.Windows.Forms.Keys.None: return OpenTK.Input.Key.Unknown;
            case System.Windows.Forms.Keys.A: return OpenTK.Input.Key.A;
            case System.Windows.Forms.Keys.B: return OpenTK.Input.Key.B;
            case System.Windows.Forms.Keys.C: return OpenTK.Input.Key.C;
            case System.Windows.Forms.Keys.D: return OpenTK.Input.Key.D;
            case System.Windows.Forms.Keys.E: return OpenTK.Input.Key.E;
            case System.Windows.Forms.Keys.F: return OpenTK.Input.Key.F;
            case System.Windows.Forms.Keys.G: return OpenTK.Input.Key.G;
            case System.Windows.Forms.Keys.H: return OpenTK.Input.Key.H;
            case System.Windows.Forms.Keys.I: return OpenTK.Input.Key.I;
            case System.Windows.Forms.Keys.J: return OpenTK.Input.Key.J;
            case System.Windows.Forms.Keys.K: return OpenTK.Input.Key.K;
            case System.Windows.Forms.Keys.L: return OpenTK.Input.Key.L;
            case System.Windows.Forms.Keys.M: return OpenTK.Input.Key.M;
            case System.Windows.Forms.Keys.N: return OpenTK.Input.Key.N;
            case System.Windows.Forms.Keys.O: return OpenTK.Input.Key.O;
            case System.Windows.Forms.Keys.P: return OpenTK.Input.Key.P;
            case System.Windows.Forms.Keys.Q: return OpenTK.Input.Key.Q;
            case System.Windows.Forms.Keys.R: return OpenTK.Input.Key.R;
            case System.Windows.Forms.Keys.S: return OpenTK.Input.Key.S;
            case System.Windows.Forms.Keys.T: return OpenTK.Input.Key.T;
            case System.Windows.Forms.Keys.U: return OpenTK.Input.Key.U;
            case System.Windows.Forms.Keys.V: return OpenTK.Input.Key.V;
            case System.Windows.Forms.Keys.W: return OpenTK.Input.Key.W;
            case System.Windows.Forms.Keys.X: return OpenTK.Input.Key.X;
            case System.Windows.Forms.Keys.Y: return OpenTK.Input.Key.Y;
            case System.Windows.Forms.Keys.Z: return OpenTK.Input.Key.Z;
            case System.Windows.Forms.Keys.F1: return OpenTK.Input.Key.F1;
            case System.Windows.Forms.Keys.F2: return OpenTK.Input.Key.F2;
            case System.Windows.Forms.Keys.F3: return OpenTK.Input.Key.F3;
            case System.Windows.Forms.Keys.F4: return OpenTK.Input.Key.F4;
            case System.Windows.Forms.Keys.F5: return OpenTK.Input.Key.F5;
            case System.Windows.Forms.Keys.F6: return OpenTK.Input.Key.F6;
            case System.Windows.Forms.Keys.F7: return OpenTK.Input.Key.F7;
            case System.Windows.Forms.Keys.F8: return OpenTK.Input.Key.F8;
            case System.Windows.Forms.Keys.F9: return OpenTK.Input.Key.F9;
            case System.Windows.Forms.Keys.F10: return OpenTK.Input.Key.F10;
            case System.Windows.Forms.Keys.Space: return OpenTK.Input.Key.Space;
            case System.Windows.Forms.Keys.ShiftKey: return OpenTK.Input.Key.ShiftLeft;
            case System.Windows.Forms.Keys.LShiftKey: return OpenTK.Input.Key.ShiftLeft;
            case System.Windows.Forms.Keys.RShiftKey: return OpenTK.Input.Key.ShiftRight;
            case System.Windows.Forms.Keys.Escape: return OpenTK.Input.Key.Escape;
            case System.Windows.Forms.Keys.Enter: return OpenTK.Input.Key.Enter;
            case System.Windows.Forms.Keys.Left: return OpenTK.Input.Key.Left;
            case System.Windows.Forms.Keys.Right: return OpenTK.Input.Key.Right;
            case System.Windows.Forms.Keys.Up: return OpenTK.Input.Key.Up;
            case System.Windows.Forms.Keys.Down: return OpenTK.Input.Key.Down;
            case System.Windows.Forms.Keys.LControlKey: return OpenTK.Input.Key.ControlLeft;
            case System.Windows.Forms.Keys.RControlKey: return OpenTK.Input.Key.ControlRight;
            case System.Windows.Forms.Keys.Alt: return OpenTK.Input.Key.AltLeft;

            default: return OpenTK.Input.Key.Unknown; 
         }
      }
   }
}
