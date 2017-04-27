using System;

namespace Graphics
{
   public class AnimationController : Controller
   {
      AnimationState myAnimationState;
      SkinnedModel myModel;

      public AnimationController(SkinnedModelRenderable renderable)
         :base(renderable, "animation")
      {
         myModel = renderable.model as SkinnedModel;
         myAnimationState = myModel.createAnimationState("null");
      }

      public AnimationState animation { get { return myAnimationState; } }

      public void startAnimation(String name)
      {
         if (myAnimationState.animation.name != name)
         {
            myAnimationState = myModel.createAnimationState(name);
         }
      }

      public override bool finished()
      {
         return false;
      }

      public override void update(float dt)
      {
         myAnimationState.update(dt);
      }
   }
}