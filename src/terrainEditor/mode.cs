using System;
using System.Collections.Generic;

using OpenTK;

using Util;
using GUI;

namespace Editor
{
   public class Mode
   {
      protected Editor myEditor;
      protected string myName;


      public Mode(Editor e, String name)
      {
         myEditor = e;
         myName = name;
      }

      public string name { get { return myName; } }

      public virtual void onGui()
      {

      }
   }
}