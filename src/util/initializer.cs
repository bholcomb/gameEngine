using System;
using System.IO;
using System.Collections.Generic;

using Lua;

namespace Util
{
    public class InitTable : LuaObject
   {
      public InitTable(LuaState state, int index)
         :base(state, index)
      {
      }

      public InitTable(LuaObject obj)
         :base(obj)
      {
      }

      public bool hasField(string name)
      {
         return contains(name);
      }

      public bool hasField(int index)
      {
         return contains(index);
      }

      #region generic lookup functions for plain data types
      public T findData<T>(String name)
      {
         if (contains(name) == false)
         {
            throw new Exception(String.Format("Cannot find table member {0}", name));
         }

         if (typeof(T) == typeof(InitTable))
         {
            LuaObject obj = get<LuaObject>(name);
            obj.push();
            InitTable tab = new InitTable(obj.state, -1);
            return (T)Convert.ChangeType(tab, typeof(T));
         }
         else
         {
            return get<T>(name);
         }
         
      }
      
      public T findData<T>(int index)
      {
         if (contains(index) == false)
         {
            throw new Exception(String.Format("Cannot find table index {0}", index));
         }

         if (typeof(T) == typeof(InitTable))
         {
            LuaObject obj = get<LuaObject>(index);
            obj.push();
            InitTable tab = new InitTable(obj.state, -1);
            return (T)Convert.ChangeType(tab, typeof(T));
         }
         else
         {
            return get<T>(index);
         }
      }

      public T findDataOr<T>(String name, T def)
      {
         if (contains(name) == false)
         {
            Warn.print("Cannot find field {0} using default {1}", name, def);
            return def;
         }

         return get<T>(name);
      }

      public T findDataOr<T>(int index, T def)
      {
         if (contains(index) == false)
         {
            Warn.print("Cannot find index {0} using default {1}", index, def);
            return def;
         }

         return get<T>(index);
      }

      #endregion

      #region specialization for init tables
      public InitTable findData(string name)
      {
         LuaObject obj = findData<LuaObject>(name);
         InitTable it = new InitTable(obj);
         return it;
      }

      public InitTable findData(int index)
      {
         LuaObject obj = findData<LuaObject>(index);
         InitTable it = new InitTable(obj);
         return it;
      }

      public InitTable findDataOr(String name, InitTable def)
      {
         LuaObject obj = findDataOr<LuaObject>(name, null);
         if (obj == null)
         {
            return def;
         }

         InitTable it = new InitTable(obj);
         return it;
      }

      public InitTable findDataOr(int index, InitTable def)
      {
         LuaObject obj = findDataOr<LuaObject>(index, null);
         if (obj == null)
         {
            return def;
         }

         InitTable it = new InitTable(obj);
         return it;
      }
      #endregion
   }
    
   public class Initializer
   {
      LuaState myVm;
      InitTable myGlobalTable;

      public Initializer(String[] args)
      {
         String configFile = "config.lua";
         myVm = new LuaState();
         if (args.Length >= 1)
         {
            configFile = args[0];
         }

         if (File.Exists(configFile) == false)
         {
            String[] env = Environment.GetCommandLineArgs();
            configFile = env[0];
            if (configFile.Contains("vshost.exe"))
            {
               configFile = configFile.Substring(0, configFile.LastIndexOf(".vshost.exe"));
            }
            else
            {
               configFile = configFile.Substring(0, configFile.LastIndexOf('.'));
            }

            configFile += ".lua";
            if (File.Exists(configFile) == false)
            {
               throw new Exception(String.Format("Cannot find configuration file {0}", configFile));
            }
         }

         FilePrintSink filePrinter = new FilePrintSink(System.IO.Path.ChangeExtension(configFile, "log"));
         Printer.addPrintSink("logFile", filePrinter);

         if (addConfigScript(configFile) == false)
         {
            throw new Exception(String.Format("Failed to load configuration file {0}", configFile));
         }

         createGlobalTable();
      }

      public Initializer(String configFile)
      {
         myVm = new LuaState();

         if (addConfigScript(configFile) == false)
         {
            throw new Exception(String.Format("Failed to load configuration file {0}", configFile));
         }

         createGlobalTable();
      }

      void createGlobalTable()
      {
         LuaDLL.lua_getglobal(myVm.statePtr, "_G");
         myGlobalTable = new InitTable(myVm, -1);
      }

      public bool addConfigScript(String filename)
      {
         if (myVm.doFile(filename) == false)
         {
            Warn.print("Initializer cannot load config {0}", filename);
            return false;
         }
         Info.print("Loaded config file {0}", filename);

         return true;
      }

      public bool hasField(string name)
      {
         return myGlobalTable.contains(name);
      }

      #region generic lookup for basic data types
      public T findData<T>(String name)
      {
         return myGlobalTable.findData<T>(name);
      }

      public T findData<T>(int index)
      {
         return myGlobalTable.findData<T> (index);
      }

      public T findDataOr<T>(String name, T def)
      {
         return myGlobalTable.findDataOr<T>(name, def);
      }

      public T findDataOr<T>(int index, T def)
      {
         return myGlobalTable.findDataOr<T>(index, def);
      }
      #endregion

      #region specialization for init tables
      public InitTable findData(string name)
      {
         LuaObject obj = findData<LuaObject>(name);
         InitTable it = new InitTable(obj);
         return it;
      }

      public InitTable findData(int index)
      {
         LuaObject obj = findData<LuaObject>(index);
         InitTable it = new InitTable(obj);
         return it;
      }

      public InitTable findDataOr(String name, InitTable def)
      {
         LuaObject obj = findDataOr<LuaObject>(name, null);
         if (obj == null)
         {
            return def;
         }

         InitTable it = new InitTable(obj);
         return it;
      }

      public InitTable findDataOr(int index, InitTable def)
      {
         LuaObject obj = findDataOr<LuaObject>(index, null);
         if (obj == null)
         {
            return def;
         }

         InitTable it = new InitTable(obj);
         return it;
      }
      #endregion
   }
}

