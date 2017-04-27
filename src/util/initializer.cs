using System;
using System.IO;
using System.Collections.Generic;

using Lua;

namespace Util
{
   /*  old version using json files instead of lua
   public class Initializer
   {
      JsonObject myData = null;
      String myConfigFilename;

      public Initializer(String[] args)
      {
         if (args.Length >= 1)
         {
            myConfigFilename = args[0];
         }
         else
         {
            myConfigFilename = "../data/config.json";
         }

         if (System.IO.File.Exists(myConfigFilename) == false)
         {
            myConfigFilename = Environment.GetCommandLineArgs()[0];
            myConfigFilename = System.IO.Path.ChangeExtension(myConfigFilename, "json");
            if (System.IO.File.Exists(myConfigFilename) == false)
            {
               //throw new InvalidOperationException("Cannot find config file " + myConfigFilename);
               Warn.print("Cannot find config file {0}", myConfigFilename);
            }
         }

         loadConfigScript();

         FilePrintSink filePrinter = new FilePrintSink(System.IO.Path.ChangeExtension(myConfigFilename, "log"));
         Printer.addPrintSink("logFile", filePrinter);
      }

      public bool hasField(String name)
      {
         if (myData == null)
         {
            return false;
         }

         String[] tokens = name.Split('.');
         JsonObject temp=myData;

         foreach (string s in tokens)
         {
            if (temp.contains(s) == false)
            {
               return false;
            }
            temp = temp[s];
         }

         return true;
      }

      public T findData<T>(String name)
      {
         if (hasField(name))
         {
            String[] tokens = name.Split('.');
            JsonObject temp = myData;
            foreach (String s in tokens)
            {
               temp = temp[s];
            }

            return (T)Convert.ChangeType(temp.raw, typeof(T));
         }

         return default(T);
      }

      public JsonObject findData(String name)
      {
         return findData<JsonObject>(name);
      }

      public T findDataOrDefault<T>(String name, T def)
      {
         if (hasField(name))
         {
            return findData<T>(name);
         }

         return def;
      }

      protected bool loadConfigScript()
      {
         myData = JsonObject.loadFile(myConfigFilename);
         return true;
      }
   }

   */

   public class InitTable : LuaObject
   {
      public InitTable(LuaState state, int index)
         :base(state, index)
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

      public T findDataOrDefault<T>(String name, T def)
      {
         if (contains(name) == false)
         {
            Warn.print("Cannot find field {0} using default {1}", name, def);
            return def;
         }

         return get<T>(name);
      }

      public T findDataOrDefault<T>(int index, T def)
      {
         if (contains(index) == false)
         {
            Warn.print("Cannot find index {0} using default {1}", index, def);
            return def;
         }

         return get<T>(index);
      }
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

      public T findData<T>(String name)
      {
         return myGlobalTable.findData<T>(name);
      }

      public T findData<T>(int index)
      {
         return myGlobalTable.findData<T> (index);
      }

      public T findDataOrDefault<T>(String name, T def)
      {
         return myGlobalTable.findDataOrDefault<T>(name, def);
      }

      public T findDataOrDefault<T>(int index, T def)
      {
         return myGlobalTable.findDataOrDefault<T>(index, def);
      }

      void createGlobalTable()
      {
         LuaDLL.lua_getglobal(myVm.statePtr, "_G");
         myGlobalTable = new InitTable(myVm, -1);
      }
   }
}

