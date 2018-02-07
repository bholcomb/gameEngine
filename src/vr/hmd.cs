using System;
using System.Collections.Generic;

using Graphics;
using Util;

using OpenTK;
using OpenTK.Graphics.OpenGL;
using Valve.VR;

namespace VR
{
   public class HMD
   {
      public Vector3 position = Vector3.Zero;
      public Quaternion orientation = Quaternion.Identity;

      public RenderTarget[] myRenderTargets = new RenderTarget[2];
      public Camera[] myCameras = new Camera[2];
      Matrix4[] myEyeTransform = new Matrix4[2];

      TrackedDevicePose_t[] renderPoseArray = new TrackedDevicePose_t[OpenVR.k_unMaxTrackedDeviceCount];
      TrackedDevicePose_t[] gamePoseArray = new TrackedDevicePose_t[OpenVR.k_unMaxTrackedDeviceCount];

      public HMD()
      {
         initEyes();
      }

      public void resetPose()
      {
         VR.vrSystem.ResetSeatedZeroPose();
      }

      void initEyes()
      {
         List<RenderTargetDescriptor> rtdesc = new List<RenderTargetDescriptor>();
         rtdesc.Add(new RenderTargetDescriptor() { attach = FramebufferAttachment.ColorAttachment0, format = SizedInternalFormat.Rgba8 }); //creates a texture internally
         rtdesc.Add(new RenderTargetDescriptor() { attach = FramebufferAttachment.DepthAttachment, bpp = 32 });

         uint w = 0;
         uint h = 0;
         VR.vrSystem.GetRecommendedRenderTargetSize(ref w, ref h);

         for (int i = 0; i < 2; i++)
         {
            myRenderTargets[i] = new RenderTarget((int)w, (int)h, rtdesc);
            myCameras[i] = new Camera(new Viewport(0, 0, (int)w, (int)h));

            float leftHalfTan = 0.0f;
            float rightHalfTan = 0.0f;
            float topHalfTan = 0.0f;
            float bottomHalfTan = 0.0f;
            //NOTE: top and bottom are still backwards
            VR.vrSystem.GetProjectionRaw((EVREye)i, ref leftHalfTan, ref rightHalfTan, ref bottomHalfTan, ref topHalfTan);

            //convert to frustum edges to create projection matrix
            float zNear = 0.01f;
            float zFar = 1000.0f;
            float left = leftHalfTan * zNear;
            float right = rightHalfTan * zNear;
            float bottom = bottomHalfTan * zNear;
            float top = topHalfTan * zNear;

            myCameras[i].setProjection(Matrix4.CreatePerspectiveOffCenter(left, right, bottom, top, zNear, zFar));

            //openVR uses a right-back-up system, just like our convention, so no conversion necessary
            myEyeTransform[i] = VR.convertToMatrix4(VR.vrSystem.GetEyeToHeadTransform((EVREye)i));
         }
      }

      public bool update()
      {
         var error = EVRCompositorError.None;
         error = OpenVR.Compositor.WaitGetPoses(renderPoseArray, gamePoseArray);

         if (error != EVRCompositorError.None)
         {
            Warn.print("Error in WaitGetPoses: {0}", error);
            return false;
         }

         //get head position
         TrackedDevicePose_t pose = renderPoseArray[OpenVR.k_unTrackedDeviceIndex_Hmd];
         Matrix4 headPose = Matrix4.Identity;
         if (pose.bDeviceIsConnected == true && pose.bPoseIsValid == true)
         {
            headPose = VR.convertToMatrix4(pose.mDeviceToAbsoluteTracking);
         }
         else
         {
            Warn.print("Error getting pose information for the HMD");
            return false;
         }

         //update camera matrix for each eye
         for (int i = 0; i < 2; i++)
         {
            //TODO:  this doesn't look right, but seems to work.  Work through the math.
            Matrix4 view = Matrix4.CreateTranslation(-position) * Matrix4.CreateFromQuaternion(orientation) * myEyeTransform[i].Inverted() * headPose.Inverted();
            myCameras[i].setView(view);
         }

         return true;
      }

      public void present()
      {
         var error = EVRCompositorError.None;
         VRTextureBounds_t bounds = new VRTextureBounds_t();
         bounds.uMin = 0.0f;
         bounds.uMax = 1.0f;
         bounds.vMin = 0.0f;
         bounds.vMax = 1.0f;

         for (int i = 0; i < 2; i++)
         {
            Texture_t tex = new Texture_t();
            tex.handle = (IntPtr)(myRenderTargets[i].buffers[FramebufferAttachment.ColorAttachment0].id());
            tex.eColorSpace = EColorSpace.Gamma;
            tex.eType = ETextureType.OpenGL;

            error = OpenVR.Compositor.Submit((EVREye)i, ref tex, ref bounds, EVRSubmitFlags.Submit_Default);

            if (error != EVRCompositorError.None)
            {
               Warn.print("Error submtting texture to OpenVR: {0}", error);
            }
         }
      }
   }

}