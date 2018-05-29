using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using Graphics;
using Util;

using OpenTK;
using OpenTK.Graphics.OpenGL;
using Valve.VR;

namespace VR
{
   public class TrackedCamera
   {
      Texture myTexture;
      bool myIsStreaming = false;
      UInt64 myHandle = 0;
      UInt32 myFrameWidth = 0;
      UInt32 myFrameHeight = 0;
      UInt32 myFrameBufferSize = 0;
      UInt64 myLastFrameSequence = 0;
      double myVideoSignalTime = 0.0;
      byte[] myFrameBuffer;
      byte[] myFrameFlipBuffer;

      Matrix4 myProjection;
      Matrix4 myView;
      Matrix4 myHeadToCameraMatrix;

      EVRTrackedCameraFrameType myFrameType = EVRTrackedCameraFrameType.MaximumUndistorted;

      public TrackedCamera()
      {
         Info.print("Tracked Camera Firmware: {0}", VR.getTrackedDeviceString(ETrackedDeviceProperty.Prop_CameraFirmwareDescription_String));

         if (OpenVR.TrackedCamera.GetCameraFrameSize(OpenVR.k_unTrackedDeviceIndex_Hmd, myFrameType, ref myFrameWidth, ref myFrameHeight, ref myFrameBufferSize) != EVRTrackedCameraError.None)
         {
            Warn.print("GetCameraFrameSize error");
         }

         myFrameBuffer = new byte[myFrameBufferSize];
         myFrameFlipBuffer = new byte[myFrameBufferSize];
         myTexture = new Texture((int)myFrameWidth, (int)myFrameHeight);

         myTexture.setWrapping(TextureWrapMode.ClampToBorder, TextureWrapMode.ClampToBorder);
         myTexture.setMinMagFilters(TextureMinFilter.Linear, TextureMagFilter.Linear);

         var err = ETrackedPropertyError.TrackedProp_Success;
         HmdMatrix34_t mat = VR.vrSystem.GetMatrix34TrackedDeviceProperty(OpenVR.k_unTrackedDeviceIndex_Hmd, ETrackedDeviceProperty.Prop_CameraToHeadTransform_Matrix34, ref err);
         myHeadToCameraMatrix = VR.convertToMatrix4(mat);
      }
      public static bool hasTrackedCamera()
      {
         bool ret = false;
         EVRTrackedCameraError error = OpenVR.TrackedCamera.HasCamera(OpenVR.k_unTrackedDeviceIndex_Hmd, ref ret);
         if (error != EVRTrackedCameraError.None || !ret)
         {
            Warn.print("No tracked camera available on HMD: {0}", OpenVR.TrackedCamera.GetCameraErrorNameFromEnum(error));
            return false;
         }

         return true;
      }

      public Texture texture { get { return myTexture; } }
      public Matrix4 pose { get { return myView; } }
      public Matrix4 projection { get { return myProjection; } }
      public Matrix4 headToCamera { get { return myHeadToCameraMatrix; } }

      public bool startStream()
      {
         myVideoSignalTime = TimeSource.now();
         OpenVR.TrackedCamera.AcquireVideoStreamingService(OpenVR.k_unTrackedDeviceIndex_Hmd, ref myHandle);
         if (myHandle == 0)
         {
            Warn.print("AcquireVideoStreamingService() failed");
            return false;
         }

         HmdMatrix44_t proj = new HmdMatrix44_t();
         EVRTrackedCameraError error = OpenVR.TrackedCamera.GetCameraProjection(OpenVR.k_unTrackedDeviceIndex_Hmd, myFrameType, 0.01f, 10.0f, ref proj);
         if(error != EVRTrackedCameraError.None)
         {
            Warn.print("Error getting camera projection");
         }
         myProjection = VR.convertToMatrix4(proj);

         Info.print("Started VR Camera stream");
         return true;
      }

      public void stopStream()
      {
         OpenVR.TrackedCamera.ReleaseVideoStreamingService(myHandle);
         myHandle = 0;
         Info.print("Stopped VR Camera stream");
      }

      public void toggleStreaming()
      {
         if (myIsStreaming == true)
         {
            stopStream();
            myIsStreaming = false;
         }
         else
         {
            myIsStreaming = startStream();
         }
      }

      public unsafe void tick()
      {
         if (myHandle == 0)
         {
            return;
         }

         if (TimeSource.now() > myVideoSignalTime + 2.0)
         {
            Warn.print("No video frames arriving");
            //stopStream();
         }

         CameraVideoStreamFrameHeader_t frameHeader = new CameraVideoStreamFrameHeader_t();
         EVRTrackedCameraError error = OpenVR.TrackedCamera.GetVideoStreamFrameBuffer(myHandle, myFrameType, IntPtr.Zero, 0, ref frameHeader, (uint)Marshal.SizeOf(frameHeader));
         if (error != EVRTrackedCameraError.None)
         {
            Warn.print("Failed to get frame header");
            return;
         }

         if (frameHeader.nFrameSequence == myLastFrameSequence)
         {
            //frame hasn't changed yet
            return;
         }

         myVideoSignalTime = TimeSource.now();

         // Frame has changed, do the more expensive frame buffer copy
         fixed (byte* ptr = myFrameBuffer)
         {
            error = OpenVR.TrackedCamera.GetVideoStreamFrameBuffer(myHandle, myFrameType, (IntPtr)ptr, myFrameBufferSize, ref frameHeader, (uint)Marshal.SizeOf(frameHeader));
            if (error != EVRTrackedCameraError.None)
            {
               Warn.print("Failed to get frame buffer");
               return;
            }
         }

         if (frameHeader.standingTrackedDevicePose.bPoseIsValid == true)
         {
            Matrix4 standingView = VR.convertToMatrix4(frameHeader.standingTrackedDevicePose.mDeviceToAbsoluteTracking);
            Matrix4 seated2Standing = VR.convertToMatrix4(VR.vrSystem.GetSeatedZeroPoseToStandingAbsoluteTrackingPose());

            myView = standingView * seated2Standing.Inverted();
         }

         invertBuffer();

         //invert buffer from first pixel being top left to bottom left
         myTexture.paste(myFrameFlipBuffer, Vector2.Zero, new Vector2(myFrameWidth, myFrameHeight), PixelFormat.Rgba);
      }

      void invertBuffer()
      {
         UInt32 rowInBytes = myFrameWidth * 4;
         UInt32 sourceOffset = myFrameBufferSize - rowInBytes;
         UInt32 destOffset = 0;
         for(int i=0; i< myFrameHeight; i++)
         {
            Array.Copy(myFrameBuffer, sourceOffset, myFrameFlipBuffer, destOffset, rowInBytes);
            sourceOffset -= rowInBytes;
            destOffset += rowInBytes;
         }
      }
   }
}