using System;

using Util;

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

      static string getTrackedDeviceString(ETrackedDeviceProperty prop)
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
   }
}