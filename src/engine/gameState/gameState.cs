using System;

using Util;

namespace Engine
{
   public class GameState
   {
      String myName;
      protected GameStateManager myGameStateManager;

      public GameState(string n)
      {
         myName = n;
      }

      public string name { get { return myName; } }
      public GameStateManager gameStateManager { 
         get { return myGameStateManager; } 
         set { myGameStateManager = value; } 
      }

      public virtual void onEnter() { }
      public virtual void onExit() { }
      public virtual void onUpdate(double dt) { }
   }
}