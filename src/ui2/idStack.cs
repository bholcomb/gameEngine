using System;
using System.Collections.Generic;

using Util;

namespace UI2
{
   public class IdStack
   {
      Stack<UInt32> mySeeds = new Stack<uint>();

      public IdStack()
      {
         mySeeds.Push(Hash.theInitValue);
      }

      public void push(UInt32 id)
      {
         mySeeds.Push(id);
      }

      public void pop()
      {
         mySeeds.Pop();
      }

      public UInt32 getId(String str)
      {
         UInt32 seed = mySeeds.Peek();
         return Hash.hash(str, seed);
      }
   }

}