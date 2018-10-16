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
      public float time;
      public Vector3 position;
      public Quaternion rotation;
   }

   public class AnimationChannel
   {
      public List<JointPose> poses = new List<JointPose>();

      public AnimationChannel()
      {

      }

      public JointPose jointAt(float time)
      {
         //check for single pose
         if(poses.Count == 1)
         {
            return poses[0];
         }

         int idx = 0;
         while (idx < poses.Count - 1 && time > poses[idx].time)
         {
            idx++;
         }

         //check for time past the last pose
         if (time > poses[idx].time)
         {
            return poses[idx];
         }

         float interpolation = (time - poses[idx - 1].time) / (poses[idx].time - poses[idx-1].time);

         JointPose ret = new JointPose();
         ret.position = Vector3.Lerp(poses[idx - 1].position, poses[idx].position, interpolation);
         ret.rotation = Quaternion.Slerp(poses[idx -1].rotation, poses[idx].rotation, interpolation);

         return ret;
      }
   }

   public class Animation
   {
      //animation parameters
      public String name { get; set; }
      public Skeleton skeleton { get; set; }
      public float fps { get; set; }
      public bool loop { get; set; }
      public float duration { get; set; }

      public List<AnimationEvent> events = new List<AnimationEvent>();
      public List<AnimationChannel> channels = new List<AnimationChannel>();

      public Animation()
      {
      }

      public List<Matrix4> buildAnimationFrame(float time)
      {
         List<Matrix4> frame = new List<Matrix4>(skeleton.boneCount);

         for (int i = 0; i < skeleton.boneCount; i++)
         {
            JointPose jp = channels[i].jointAt(time);

            Matrix4 rel =  Matrix4.CreateFromQuaternion(jp.rotation) * Matrix4.CreateTranslation(jp.position);
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
            frame[i] = skeleton.myBones[i].myInvWorldBindMatrix * frame[i];
         }

         return frame;
      }

      public void debugDraw(List<Matrix4> pose, Vector3 pos, Quaternion ori)
      {
         List<Matrix4> untxBones = new List<Matrix4>();
         for(int i = 0; i< skeleton.boneCount; i++)
         {
            Bone b = skeleton.myBones[i];
            untxBones.Add( b.myWorldBindMatrix * pose[i]);
            
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
      public float time { get; set; }
      public bool isDone { get; set; }

      public AnimationState(Animation ani)
      {
         animation = ani;

         //ready to start this animation
         reset();
      }

      public void reset()
      {
         time = 0.0f;
         isDone = false;
      }

      public void update(float dt)
      {
         time += dt;
         if(time > animation.duration)
         {
            if(animation.loop)
            {
               time -= animation.duration;
            }
            else
            {
               time = animation.duration;
               isDone = true;
            }
         }
      }

      public List<Matrix4> skinningMatrix()
      {
         return animation.buildAnimationFrame(time);
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

         for(int i=0; i< skeleton.boneCount; i++)
         {
            AnimationChannel boneChannel = new AnimationChannel();
            JointPose jp = new JointPose();
            jp.position = Vector3.Zero;
            jp.rotation = Quaternion.Identity;
            boneChannel.poses.Add(jp);
            nullAni.channels.Add(boneChannel);
         }

         animations["null"] = nullAni;
      }
   }
}