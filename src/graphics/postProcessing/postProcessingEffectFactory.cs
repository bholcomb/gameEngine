using System;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using Util;

namespace Graphics
{
   public class PostProcessingFactory
   {
      public delegate PostEffect EffectCreator();
      public static Dictionary<String, EffectCreator> EffectFactory = new Dictionary<string, EffectCreator>();

      static PostProcessingFactory()
      {
         addCreator<UnderwaterEffect>("underwater");
         addCreator<BlurEffect>("blur");
         addCreator<FogEffect>("fog");
         addCreator<FishEyeEffect>("fisheye");
      }

      public static void addCreator<T>(String name) where T : PostEffect, new()
      {
         EffectCreator creator = delegate () { return new T(); };
         EffectFactory[name] = creator;
      }

      public static PostEffect create(String name)
      {
         EffectCreator creator;
         if (EffectFactory.TryGetValue(name, out creator) == true)
         {
            PostEffect ret = creator();
            return ret;
         }

         return null;
      }
   }
}