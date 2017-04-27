using System;
using System.Collections.Generic;

using Util;
using Lua;

namespace Sim
{
   public class ParameterDatabase
   {
      LuaState myVm;
      LuaObject myTemplates;

      public ParameterDatabase()
      {
         myVm = new LuaState();
      }

      public bool init(Initializer init)
      {
         //read in templates
         String path = (String)init.findDataOrDefault("SimManager.entityDataDir", "../data/entity");
         
         //read in the scripts to support the entity system
         //may need to move this to a resource
         myVm.doFile(path+"/entitySystem.lua");

         //read in all the entity files
         foreach (String file in System.IO.Directory.GetFiles(path))
         {
            if (file.Contains("entitySystem.lua") == false)
            {
               Debug.print("Parameter Database: parsing file: {0}", file);
               myVm.doFile(file);
            }
         }

         myTemplates = myVm.findObject("entity.templates");
         if (myTemplates == null)
         {
            throw new Exception("Cannot find entity templates");
         }

         return true;
      }

      public LuaObject entityTemplate(String name)
      {
         LuaObject template = myTemplates[name];
         if (template==null)
         {
            Warn.print("Paramter Database: Cannot find entity template {0}", name);
            return null;
         }

         return template;
      }

      public JsonObject weaponTemplate(String name)
      {
         String templateName = "weapon.templates." + name;
         JsonObject template = null;//(JsonObject)ScriptManager.vm[templateName];

         if (template == null)
         {
            Warn.print("Paramter Database: Cannot find weapon template {0}", name);
         }

         return template;
      }

      public JsonObject damageTemplate(String name)
      {
         String templateName = "damage.templates." + name;
         JsonObject template = null;//(JsonObject)ScriptManager.vm[templateName];

         if (template == null)
         {
            Warn.print("Paramter Database: Cannot find damage template {0}", name);
         }

         return template;
      }
   }
}