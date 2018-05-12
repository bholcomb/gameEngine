using System;

using Util;

using OpenTK;
using Valve.VR;

namespace VR
{
   public class VR
   {
      public static CVRSystem vrSystem;

      public static bool vrAvailable()
      {
         return OpenVR.IsRuntimeInstalled();
      }

      public static bool hmdAttached()
      {
         return OpenVR.IsHmdPresent();
      }

      public static bool init()
      {
         EVRInitError error = EVRInitError.None;
         vrSystem = OpenVR.Init(ref error, EVRApplicationType.VRApplication_Scene);

         if (error != EVRInitError.None)
         {
            Warn.print("Error initializing OpenVR: {0}", error);
            return false;
         }

         if (OpenVR.Compositor == null)
         {
            Warn.print("Failed to initialize OpenVR compositor");
            return false;
         }

         //setup a seated environment
         OpenVR.Compositor.SetTrackingSpace(ETrackingUniverseOrigin.TrackingUniverseSeated);

         Info.print("OpenVR Manufacturer: {0}", getTrackedDeviceString(ETrackedDeviceProperty.Prop_ManufacturerName_String));
         Info.print("OpenVR Model Number: {0}", getTrackedDeviceString(ETrackedDeviceProperty.Prop_ModelNumber_String));
         Info.print("OpenVR Tracking System: {0}", getTrackedDeviceString(ETrackedDeviceProperty.Prop_TrackingSystemName_String));
         Info.print("OpenVR Driver Version: {0}", getTrackedDeviceString(ETrackedDeviceProperty.Prop_DriverVersion_String));

         return true;
      }

      public static bool shutdown()
      {
         OpenVR.Shutdown();
         return true;
      }

      public static string getTrackedDeviceString(ETrackedDeviceProperty prop)
      {
         var error = ETrackedPropertyError.TrackedProp_Success;
         uint bufferLength = vrSystem.GetStringTrackedDeviceProperty(OpenVR.k_unTrackedDeviceIndex_Hmd, prop, null, 0, ref error);
         if (bufferLength > 1)
         {
            var stringBuilder = new System.Text.StringBuilder((int)bufferLength);
            vrSystem.GetStringTrackedDeviceProperty(OpenVR.k_unTrackedDeviceIndex_Hmd, prop, stringBuilder, bufferLength, ref error);

            return stringBuilder.ToString();
         }

         if (error != ETrackedPropertyError.TrackedProp_Success)
         {
            return error.ToString();
         }

         return "unkown";
      }

      public static Matrix4 convertToMatrix4(HmdMatrix34_t m)
      {
         Matrix4 mat = new Matrix4(
            m.m0, m.m4, m.m8, 0.0f,
            m.m1, m.m5, m.m9, 0.0f,
            m.m2, m.m6, m.m10, 0.0f,
            m.m3, m.m7, m.m11, 1.0f
            );

         return mat;
      }

      public static Matrix4 convertToMatrix4(HmdMatrix44_t m)
      {
         Matrix4 mat = new Matrix4(
            m.m0, m.m4, m.m8, m.m12,
            m.m1, m.m5, m.m9, m.m13,
            m.m2, m.m6, m.m10, m.m14,
            m.m3, m.m7, m.m11, m.m15
            );

         return mat;
      }


      public static HmdMatrix34_t convertToMatrix34(Matrix4 m)
      {
         HmdMatrix34_t mat;
         mat.m0 = m.M11;
         mat.m1 = m.M21;
         mat.m2 = m.M31;
         mat.m3 = m.M41;
         mat.m4 = m.M12;
         mat.m5 = m.M22;
         mat.m6 = m.M32;
         mat.m7 = m.M42;
         mat.m8 = m.M13;
         mat.m9 = m.M23;
         mat.m10 = m.M33;
         mat.m11 = m.M43;
         return mat;
      }
   }
}