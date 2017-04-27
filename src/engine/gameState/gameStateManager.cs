using System;
using System.Collections.Generic;

using Engine;

namespace Engine
{
   public class GameStateManagerTask : Task
   {
      GameStateManager myManager;
      public GameStateManagerTask(GameStateManager mgr)
         : base("Game State Manager")
      {
         myManager = mgr;
         frequency = 30;
      }

      protected override void onUpdate(double dt)
      {
         myManager.update(dt);
      }

   }

   public class GameStateManager
   {
      Dictionary<string, GameState> myGameStates = new Dictionary<string, GameState>();
      Stack<GameState> myStateStack=new Stack<GameState>();

      bool myTransition = false;
      string myTransitionName;
      bool myPush = false;
      string myPushName;
      bool myPop = false;

      public GameStateManager()
      {

      }

      public GameState currentState { get { return myStateStack.Peek(); } }

      public void addGameState(GameState gs)
      {
         gs.gameStateManager = this;
         myGameStates.Add(gs.name, gs);
      }

      public void transition(String gsName)
      {
         myTransition = true;
         myTransitionName = gsName;
      }

      public void push(String gsName)
      {
         myPush = true;
         myPushName = gsName;
      }

      public void pop()
      {
         myPop = true;
      }

      void doTransition(String gsName)
      {
         if (myStateStack.Count > 0)
         {
            if (gsName == myStateStack.Peek().name)
               return;

            GameState oldState = myStateStack.Pop();
            if (oldState != null)
               oldState.onExit();
         }

         GameState newState;
         if (myGameStates.TryGetValue(gsName, out newState) == false)
         {
            throw new Exception("Unknown state " + gsName);
         }

         newState.onEnter();
         myStateStack.Push(newState);
      }

      public void doPush(String gsName)
      {
         if (gsName == myStateStack.Peek().name)
            return;

         GameState newState;
         if (myGameStates.TryGetValue(gsName, out newState) == false)
         {
            throw new Exception("Unknown state " + gsName);
         }

         newState.onEnter();
         myStateStack.Push(newState);
      }

      public void doPop()
      {
         if (myStateStack.Count == 0)
         {
            throw new Exception("Cannot pop an empty game state stack");
         }

         GameState gs=myStateStack.Pop();
         gs.onExit();
      }

      public void update(double dt)
      {
         foreach (GameState gs in myStateStack)
         {
            gs.onUpdate(dt);
         }

         if (myTransition == true)
         {
            doTransition(myTransitionName);
            myTransition = false;
            myTransitionName = "";
         }

         if (myPush == true)
         {
            doPush(myPushName);
            myPush = false;
            myPushName = "";
         }

         if (myPop == true)
         {
            doPop();
            myPop = false;
         }
      }
   }
}