using System;
using System.IO;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using Util;

namespace Graphics
{
   public class Animation
   {
      //animation parameters
      public String name { get; set; }
      public int startFrame { get; set; }
      public int endFrame { get; set; }
      public float fps { get; set; }
      public bool loop { get; set; }
      public List<AnimationEvent> events = new List<AnimationEvent>();
      public List<List<Matrix4>> poses = new List<List<Matrix4>>();

      public Animation()
      {
      }

      public Animation(string animName, int start, int end, float framesPerSecond, bool isLooping)
      {
         name = animName;
         startFrame = start;
         endFrame = end;
         fps = framesPerSecond;
         loop = isLooping;
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
         currentFrame = animation.startFrame;
         nextFrame = animation.startFrame + 1;
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
            if (nextFrame > animation.endFrame)
            {
               if (animation.loop == true)
               {
                  nextFrame = animation.startFrame;
               }
               else
               {
                  nextFrame = animation.endFrame;
                  isDone = true;
               }
            }
         }
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

         //insert the null animation
         animations["null"] = new Animation("null", 0, 0, 0, false);
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
   }
}