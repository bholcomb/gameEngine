using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using Util;
using Engine;

namespace Sim
{
   public abstract class BaseAttribute 
   {
      protected String myName;
      protected bool myIsDirty;
      protected Entity myEntity;

      public BaseAttribute(String name, Entity e)
      {
         myName = name;
         myEntity = e;
         
         //auto register the attribute when it's created
         myEntity.registerAttribute(this);
      }

      public String name
      {
         get { return myName; }
      }

      public abstract void update();
   }

   public class Attribute<T> : BaseAttribute
   {
      T myCurrentValue;
      T myNextValue;

      public delegate void AttriubteChanged(T newValue);
      public event AttriubteChanged onValueChanged;

      public Attribute(Entity e, String name, T initalValue) : base(name, e)
      {
         if (initalValue != null)
         {
            myCurrentValue = initalValue;
            myNextValue = initalValue;
         }
      }

      public override void update()
      {
         if (myIsDirty)
         {
            myCurrentValue = myNextValue;
            myIsDirty = false;

            if (onValueChanged != null)
            {
               onValueChanged(myCurrentValue);
            }
         }
      }

      public static bool operator ==(Attribute<T> a, T b)
      {
         // Return true if the fields match:
         return System.Object.Equals(a.myCurrentValue, b);
      }

      public static bool operator !=(Attribute<T> a, T b)
      {
         return !(a == b);
      }

      public override bool Equals(object obj)
      {
         if (obj == null)
            return false;

         return System.Object.Equals(myCurrentValue, obj);
      }

      public T value()
      {
         return myCurrentValue;
      }

      public void setValue(T newVal)
      {
         myNextValue = newVal;

         //if it's really a change of the value
         if (Object.Equals(newVal, myCurrentValue)==false)
         {
            myIsDirty = true;
            //if its not a reflected entity, send a change message
            if (myEntity.hasAttribute("reflected") == false || myEntity.attribute<bool>("reflected") == false)
            {
               //send message here that attribute changed 
               myEntity.dispatchAttributeChangeEvent("entity.attribute." + myName, newVal);
            }
         }
      }

      //this is not thread safe
      public void initValue(T newValue)
      {
         setValue(newValue);
         update();
         
      }

      //this is not thread safe, but needed for child/parent relationships during updates
      //during different buckets, this should be good to use since the next value is guaranteed
      //not to change within a parent when being checked by a child
      public T peekValue()
      {
         return myNextValue;
      }
   }
}