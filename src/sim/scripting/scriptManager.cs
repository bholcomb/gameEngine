using System;
using System.Collections.Generic;
using System.Reflection;

/*
using LuaInterface;
using LuaSharp;

namespace Sim
{
   public class ScriptCommandEvent : Event
   {
      static EventName theName;
      String myCommand;

      public ScriptCommandEvent() : this("", TimeSource.currentTime(), 0.0) { }
      public ScriptCommandEvent(String cmd) : this(cmd, TimeSource.currentTime(), 0.0) { }
      public ScriptCommandEvent(String cmd, double timestamp) : this(cmd, timestamp, 0.0) { }
      public ScriptCommandEvent(String cmd, double timestamp, double delay)
         : base(timestamp, delay)
      {
         myName = theName;
         myCommand = cmd;
      }

      static ScriptCommandEvent()
      {
         theName = new EventName("script.command");
      }

      public String command
      {
         get { return myCommand; }
      }
   };


   public static class ScriptManager
   {
      static Lua myLuaState;
      static LuaDelegate myPrintDelegate;

      public static bool init(Initializer init)
      {
         myLuaState = new Lua();

         myPrintDelegate = new LuaDelegate();
         myPrintDelegate.function = new LuaFunction(ScriptManager.newPrintFunction, myLuaState);
         myLuaState.RegisterFunction("print", null, typeof(ScriptManager).GetMethod("newPrint"));

         if (init.hasField("core.initScripts") == true)
         {
            JsonObject files = init.findData<JsonObject>("core.initScripts");
            foreach(String s in files.Values)
            {
               String path = init.findData<String>("core.scriptPath");
               path=System.IO.Path.Combine(path, s);
               myLuaState.DoFile(path);
            }
         }

         Kernel.eventManager.addListener(handleConsoleCommand, "script.command");

         return true;
      }

      public static void shutdown()
      {
         Kernel.eventManager.removeListener(handleConsoleCommand, "script.command");
      }

      public static void newPrint(String s)
      {
         Warn.print(s);
      }

      public static int newPrintFunction(IntPtr L)
      {
         int n = LuaDLL.lua_gettop(L);  // number of arguments
         int i;
         LuaDLL.lua_getglobal(L, "tostring");
         for (i=1; i<=n; i++) 
         {
            LuaDLL.lua_pushvalue(L, -1);  // function to be called 
            LuaDLL.lua_pushvalue(L, i);   // value to print 
            LuaDLL.lua_call(L, 1, 1);
            String s = LuaNet.lua_tostring(L, -1);  // get result 
            if (s == null)
            {
               LuaDLL.luaL_error(L, "\"tostring\" must return a string to \"print\"");
               return 0;
            }
            if (i>1)
            {
               s.Insert(0, "\t");
            }
            Error.print(s);
            LuaDLL.lua_pop(L, 1);  // pop result
         }

         return 0;
      }

      public static EventManager.EventResult handleConsoleCommand(Event e)
      {
         ScriptCommandEvent ce = e as ScriptCommandEvent;
         if (e != null)
         {
            try
            {
               Object[] objs = myLuaState.DoString(ce.command);
            }
            catch
            {
            }
         }
         return EventManager.EventResult.EATEN;
      }

      public static Lua vm
      {
         get { return myLuaState; }
      }


   }
}

*/