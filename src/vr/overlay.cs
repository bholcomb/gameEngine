using System;

using Graphics;
using Util;

using OpenTK;
using OpenTK.Graphics.OpenGL;

using Valve.VR;

namespace VR
{
   public class Overlay
   {
      Texture myTexture;
      UInt64 myHandle;
      bool myVisible;

      public Overlay(string key, string name, Texture tex)
      {
         myTexture = tex;
         myVisible = false;
         EVROverlayError error = OpenVR.Overlay.CreateOverlay(key, name, ref myHandle);
         if (error != EVROverlayError.None)
         {
            throw new Exception(string.Format("Error creating VR Overlay {0}", error));
         }

         error = OpenVR.Overlay.SetOverlayInputMethod(myHandle, VROverlayInputMethod.None);
         error = OpenVR.Overlay.SetOverlayWidthInMeters(myHandle, 1.5f);

         setOffsetMatrix(Matrix4.CreateTranslation(new Vector3(0f, 0f, -2.5f)));

         //show the overlay
         visible = true;
      }

      public bool visible
      {
         get { return myVisible; }
         set
         {
            if (myVisible != value)
            {
               myVisible = value;
               if (myVisible == true)
               {
                  EVROverlayError error = OpenVR.Overlay.ShowOverlay(myHandle);
                  if (error != EVROverlayError.None)
                  {
                     throw new Exception(String.Format("Error showing overlay {0}", error));
                  }
               }
               else
               {
                  EVROverlayError error = OpenVR.Overlay.HideOverlay(myHandle);
                  if (error != EVROverlayError.None)
                  {
                     throw new Exception(String.Format("Error hiding overlay {0}", error));
                  }
               }
            }
         }
      }

      public Texture texture
      {
         get { return myTexture; }
         set
         {
            myTexture = value;
         }
      }

      public void setInputMethod()
      {
      }

      public void setOffsetMatrix(Matrix4 mat)
      {
         HmdMatrix34_t m = VR.convertToMatrix34(mat);
         EVROverlayError error = EVROverlayError.None;
         //error = OpenVR.Overlay.SetOverlayTransformTrackedDeviceRelative(myHandle, OpenVR.k_unTrackedDeviceIndex_Hmd, ref m);
         error = OpenVR.Overlay.SetOverlayTransformAbsolute(myHandle, ETrackingUniverseOrigin.TrackingUniverseSeated, ref m);
         if (error != EVROverlayError.None)
         {
            throw new Exception(String.Format("Error setting overlay transfrom {0}", error));
         }
      }

      public void submit()
      {
         Texture_t tex = new Texture_t();
         tex.handle = new IntPtr(myTexture.id());
         tex.eType = ETextureType.OpenGL;
         tex.eColorSpace = EColorSpace.Auto;

         //EVROverlayError error = OpenVR.Overlay.SetOverlayTexture(myHandle, ref tex);
         EVROverlayError error = OpenVR.Overlay.SetOverlayFromFile(myHandle, "C:\\source\\gameEngine\\data\\textures\\uvTest.png");
         if (error != EVROverlayError.None)
         {
            Warn.print("Error submitting overlay to HMD: {0}", error);
         }
      }

      public void release()
      {
         if (myHandle != 0)
         {
            OpenVR.Overlay.DestroyOverlay(myHandle);
            myHandle = 0;
         }
      }
   }
}