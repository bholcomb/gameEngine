using System;

namespace Graphics
{
   public abstract class Controller
   {
      protected Renderable myInstance;
      String myName;

      public Controller(Renderable instance, String name)
      {
         myInstance = instance;
         myName = name;
      }

      public String name { get { return myName; } }

      public abstract bool finished();
      public abstract void update(float dt);
   }
}