using System;
using System.IO;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using Util;

namespace Graphics
{
   public struct JointPose
   {
      public Vector3 position;
      public Quaternion rotation;
   }

   public class AnimationFrame : List<JointPose>
   {
      public AnimationFrame() : base()
      {
      }
   }


   public class Animation
   {
      //animation parameters
      public String name { get; set; }
      public Skeleton skeleton { get; set; }
      public int numFrames { get { return poses.Count; } }
      public float fps { get; set; }
      public bool loop { get; set; }

      public List<AnimationEvent> events = new List<AnimationEvent>();
      public List<AnimationFrame> poses = new List<AnimationFrame>();

      public Animation()
      {
      }

      public List<Matrix4> buildAnimationFrame(int currentFrame, int nextFrame, float interpolation)
      {
         List<Matrix4> frame = new List<Matrix4>(skeleton.boneCount);

         for (int i = 0; i < skeleton.boneCount; i++)
         {
            JointPose jp1 = poses[currentFrame][i];
            JointPose jp2 = poses[nextFrame][i];

            Vector3 pos = Vector3.Lerp(jp1.position, jp2.position, interpolation); // Vector3.Zero; //
            Quaternion ori = Quaternion.Slerp(jp1.rotation, jp2.rotation, interpolation); //Quaternion.Identity; //

            Matrix4 rel =  Matrix4.CreateFromQuaternion(ori) * Matrix4.CreateTranslation(pos);
            Matrix4 final = Matrix4.Identity;
            Bone b = skeleton.myBones[i];
            if(b.myParent == -1)
            {
               final = rel;
            }
            else
            {
               final =  rel * frame[b.myParent];
            }

            frame.Add(final);
         }

         for (int i = 0; i < skeleton.boneCount; i++)
         {
            frame[i] = skeleton.myBones[i].myInvWorldMatrix * frame[i];
         }

         return frame;
      }

      public void debugDraw(List<Matrix4> pose, Vector3 pos, Quaternion ori)
      {
         List<Matrix4> untxBones = new List<Matrix4>();
         for(int i = 0; i< skeleton.boneCount; i++)
         {
            Bone b = skeleton.myBones[i];
            untxBones.Add( pose[i] * b.myWorldMatrix);
            
            Vector3 p = pos + Vector3.Transform(untxBones[i].ExtractTranslation(), ori);
            DebugRenderer.addSphere(p, 0.05f, Color4.Green, Fill.TRANSPARENT, false, 0.0f);

            if (b.myParent != -1)
            {
               Vector3 p1 = pos + Vector3.Transform(untxBones[i].ExtractTranslation(), ori);
               Vector3 p2 = pos + Vector3.Transform(untxBones[b.myParent].ExtractTranslation(), ori);
               DebugRenderer.addLine(p1, p2, Color4.Blue, Fill.WIREFRAME, false, 0.0f);
            }
         }
      }
   }

   public class AnimationState
   {
      //animation parameters
      public Animation animation { get; set; }

      //between frame animation data
      public float interpolation { get; set; }
      public int currentFrame { get; set; }
      public int nextFrame { get; set; }
      public bool isDone { get; set; }

      public AnimationState(Animation ani)
      {
         animation = ani;

         //ready to start this animation
         reset();
      }

      public void reset()
      {
         currentFrame = 0;
         nextFrame = 1;
         interpolation = 0;
         isDone = false;
      }

      public void update(float dt)
      {
         float increment = dt * animation.fps;
         interpolation += increment;

         while (interpolation >= 1.0)
         {
            interpolation -= 1.0f;
            currentFrame = nextFrame;
            nextFrame += 1;
            if (nextFrame == animation.numFrames)
            {
               if (animation.loop == true)
               {
                  nextFrame = 0;
               }
               else
               {
                  nextFrame = animation.numFrames - 1;
                  isDone = true;
               }
            }
         }
      }

      public List<Matrix4> skinningMatrix()
      {
         return animation.buildAnimationFrame(currentFrame, nextFrame, interpolation);
      }
   }

   public class SkinnedModel : Model
   {
      public ShaderStorageBufferObject myFrames;
      public Dictionary<String, Animation> animations { get; set; }
      public Skeleton skeleton { get; set; }

      public SkinnedModel()
      {
         myFrames = new ShaderStorageBufferObject(BufferUsageHint.StaticDraw);
         animations = new Dictionary<String, Animation>();
         skeleton = new Skeleton();
      }

      public new void Dispose()
      {
         base.Dispose();
         myFrames.Dispose();
      }

      public AnimationState createAnimationState(String animName)
      {
         Animation ani;
         if (animations.TryGetValue(animName, out ani) == false)
         {
            Warn.print("Failed to find animation {0}", animName);
            animations.TryGetValue("null", out ani);
         }

         return new AnimationState(ani);
      }

      public void createNullAnimation()
      {
         //insert the null animation
         Animation nullAni = new Animation();
         nullAni.name = "null";
         nullAni.fps = 0;
         nullAni.loop = false;
         nullAni.skeleton = skeleton;

         AnimationFrame frame = new AnimationFrame();
         for(int i=0; i< skeleton.boneCount; i++)
         {
            JointPose jp = new JointPose();
            jp.position = Vector3.Zero;
            jp.rotation = Quaternion.Identity;
            frame.Add(jp);
         }

         nullAni.poses.Add(frame);
         animations["null"] = nullAni;
      }
   }
}