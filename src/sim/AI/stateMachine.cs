using System;
using System.Collections.Generic;

using Util;
using Events;
using Engine;

namespace Sim
{
   public abstract class FiniteState
   {
      protected String myName;
      protected FiniteStateMachine myMachine;

      public FiniteState(FiniteStateMachine fsm, String name)
      {
          machine = fsm;
          myName = name;
      }

      public String name { get { return myName; } }
      public FiniteStateMachine machine { get; set; }

      public abstract void onEntry();
      public abstract void onExit();
      public abstract void onUpdate(double dt);
      public abstract EventManager.EventResult onEvent(Event e);
   }

   public class FiniteStateMachine
   {
      FiniteState myCurrentState;
      Dictionary<String, FiniteState> myStates = new Dictionary<string, FiniteState>();

      public FiniteStateMachine()
      {
      }

      public String currentState
      {
         get { return myCurrentState.name; }
      }

      public void addState(FiniteState state)
      {
         state.machine = this;
         myStates[state.name] = state;
      }

      public void transition(String stateName)
      {
         FiniteState state;
         if (myStates.TryGetValue(stateName, out state) == true)
         {
            if (myCurrentState != null)
            {
               myCurrentState.onExit();
            }

            //Debug.print("AI: Transition to : {0}", stateName);
            myCurrentState = state;
            myCurrentState.onEntry();
         }
         else
         {
            Error.print("Cannot transition to state {0}", stateName);
         }
      }

      public void onUpdate(double dt)
      {
         if (myCurrentState != null)
         {
            myCurrentState.onUpdate(dt);
         }
      }

      public EventManager.EventResult onEvent(Event e)
      {
         if (myCurrentState != null)
         {
            return myCurrentState.onEvent(e);
         }

         return EventManager.EventResult.IGNORED;
      }
   }
}