using System;

using Graphics;
using Util;

using OpenTK;
using OpenTK.Graphics.OpenGL;

using Valve.VR;

namespace VR
{
   public class HmdView : Graphics.View
   {
      HMD myHmd;
      Graphics.View myLeftEyeView;
      Graphics.View myRightEyeView;

      public HmdView(string name, HMD hmd) : base(name, null, null)
      {
         myHmd = hmd;
         myLeftEyeView = new Graphics.View("Left Eye", myHmd.myCameras[0], myHmd.myCameras[0].viewport());
         myRightEyeView = new Graphics.View("Right Eye", myHmd.myCameras[1], myHmd.myCameras[1].viewport());
         addSibling(myLeftEyeView);
         addSibling(myRightEyeView);
      }

      public override void addPass(Pass p)
      {
         Pass lp = new Pass(p);
         if (p.renderTarget == null)
            lp.renderTarget = myHmd.myRenderTargets[0];
         myLeftEyeView.addPass(lp);

         Pass rp = new Pass(p);
         if (p.renderTarget == null)
            rp.renderTarget = myHmd.myRenderTargets[1];
         myRightEyeView.addPass(rp);
      }

      public override void removePass(String passName)
      {
         myLeftEyeView.removePass(passName);
         myRightEyeView.removePass(passName);
      }
   }
}