using System;
using System.IO;
using System.Collections.Generic;

using OpenTK;
using Util;
using Lua;
using Engine;

namespace Sim
{
   public class EntityFactory
   {
      Dictionary<String, BehaviorCreator> myCreators = new Dictionary<string, BehaviorCreator>();
      EntityManager myMgr;
      EntityDatabase myDb;
      
      //0 is reserved for something else
      ulong myNextId = 1;
      ulong myAppId = Kernel.uniqueId;

      public EntityFactory(EntityManager mgr)
      {
         myMgr = mgr;
         myDb = mgr.db;
      }

      public bool init(Initializer init)
      {
         //register components
//          addCreator(new RenderBehaviorCreator());
//          addCreator(new AnimationBehaviorCreator());
//          addCreator(new ScriptBehaviorCreator());
//          addCreator(new CollisionBehaviorCreator());
//          addCreator(new AudioBehaviorCreator());
//          addCreator(new EmbarkationBehaviorCreator());
//          addCreator(new SenseBehaviorCreator());
//          addCreator(new DamageBehaviorCreator());
//          addCreator(new UserInputBehaviorCreator());
//          addCreator(new CameraBehaviorCreator());
//          addCreator(new WeaponBehaviorCreator());
//          addCreator(new ListenerBehaviorCreator());

         return true;
      }

      public Entity create(string entityType)
      {
         LuaObject template = myMgr.paramterDatabase.entityTemplate(entityType);

         if (template == null)
         {
            Warn.print("Cannot find template for: {0}", entityType);
            return null;
         }

         using (template)
         {
            Entity e = null;

            //get parents contribution
            String inheritsFrom = (String)template["inherits"];
            if (inheritsFrom != null)
            {
               //recursively get parents contributions
               e = create(inheritsFrom);
            }

            //if the entity hasn't been created yet, create it.  Happens in base class if inheritance is used
            if (e == null)
            {
               //high 32 bits for the process ID (rare collisions on this)
               ulong id = Kernel.procId;
               id = id << 32;

               //add in (or binary or) the id bits
               id += myNextId++;

               e = new Entity(id, myDb);
            }

            //setup the entity type
            Attribute<String> typeAttribute=new Attribute<String>(e, "type", (string)template["type"]);

            LuaObject attributes = template["attributes"];
            LuaObject behaviors = template["behaviors"];

            //create any attributes
            foreach (String attName in attributes.keys)
            {
               using (LuaObject att = attributes[attName])
               {
                  createAttribute(ref e, attName, att, entityType);
               }
            }

            //create the behaviors
            foreach (String behaviorName in behaviors.keys)
            {
               BehaviorCreator bc;
               if (myCreators.TryGetValue(behaviorName, out bc))
               {
                  //create with the initialization data from the template
                  using (LuaObject bhv = behaviors[behaviorName])
                  {
                     bc.create(e, bhv);
                  }
               }
               else
               {
                  Error.print("Unable to find creator for {0} in template {1}", behaviorName, entityType);
               }
            }

            attributes.Dispose();
            behaviors.Dispose();

            //called to allow behaviors to find one another
            e.postInit();

            return e;
         }
      }

      public Entity createReflected(string entityType, ulong id)
      {
         LuaObject template = myMgr.paramterDatabase.entityTemplate(entityType);

         if (template == null)
         {
            Warn.print("Cannot find template for: {0}", entityType);
            return null;
         }

         using (template)
         {
            Entity e = null;

            //get parents contribution
            String inheritsFrom = (String)template["inherits"];
            if (inheritsFrom != null)
            {
               //recursively get parents contributions
               e = createReflected(inheritsFrom, id);
            }

            //if the entity hasn't been created yet, create it.  Happens in base class if inheritance is used
            if (e == null)
            {
               e = new Entity(id, myDb);
            }

            LuaObject attributes = template["attributes"];
            LuaObject reflected = template["reflected"];
            LuaObject behaviors = template["behaviors"];

            //create any attributes
            foreach (String attName in attributes.keys)
            {
               using (LuaObject att = attributes[attName])
               {
                  createAttribute(ref e, attName, att, entityType);
               }
            }

            //create the behaviors
            foreach (String behaviorName in behaviors.keys)
            {
               bool isReflected = false;
               foreach (LuaObject rb in reflected)
               {
                  if (behaviorName == (string)rb)
                  {
                     isReflected = true;
                  }
               }

               if (isReflected == true)
               {
                  BehaviorCreator bc;
                  if (myCreators.TryGetValue(behaviorName, out bc))
                  {
                     //create with the initialization data from the template
                     using (LuaObject bhv = behaviors[behaviorName])
                     {
                        bc.create(e, bhv);
                     }
                  }
                  else
                  {
                     Error.print("Unable to find creator for {0} in template {1}", behaviorName, entityType);
                  }
               }
            }

            attributes.Dispose();
            behaviors.Dispose();
            reflected.Dispose();

            //called to allow behaviors to find one another
            e.postInit();

            Attribute<bool> reflectedAtt=new Attribute<bool>(e, "reflected", true);
            return e;
         }
      }

      protected void createAttribute(ref Entity e, String name, LuaObject attr, String entityType)
      {
         switch ((String)attr["type"])
         {
            case "bool":
               {
                  bool val=false;
                  if(attr.contains("value")==true)
                  {
                     val = (bool)attr["value"];
                  }
                  Attribute<bool> temp = new Attribute<bool>(e, name, val);
                  break;
               }
            case "int":
               {
                  int val = 0;
                  if(attr.contains("value")==true)
                  {
                     val = (int)attr["value"];
                  }
                  Attribute<int> temp = new Attribute<int>(e, name, val);
                  break;
               }
            case "uint":
               {
                  uint val = 0;
                  if(attr.contains("value")==true)
                  {
                     val = (uint)attr["value"];
                  }
                  Attribute<uint> temp = new Attribute<uint>(e, name, val);
                  break;
               }
            case "long":
               {
                  long val = 0;
                  if(attr.contains("value")==true)
                  {
                     val = (long)attr["value"];
                  }
                  Attribute<long> temp = new Attribute<long>(e, name, val);
                  break;
               }
            case "ulong":
               {
                  ulong val = 0;
                  if(attr.contains("value")==true)
                  {
                     double t=(double)attr["value"];
                     val = (ulong)t;
                  }
                  Attribute<ulong> temp = new Attribute<ulong>(e, name, val);
                  break;
               }
            case "float":
               {
                  float val = 0.0f;
                  if(attr.contains("value")==true)
                  {
                     val = (float)attr["value"];
                  }
                  Attribute<float> temp = new Attribute<float>(e, name, val);
                  break;
               }
            case "double":
               {
                  double val = 0.0;
                  if(attr.contains("value")==true)
                  {
                     val = (double)attr["value"];
                  }
                  Attribute<double> temp = new Attribute<double>(e, name, val);
                  break;
               }
            case "string":
               {
                  string val = "";
                  if(attr.contains("value")==true)
                  {
                     val = (string)attr["value"];
                  }
                  Attribute<string> temp = new Attribute<string>(e, name, val);
                  break;
               }
            case "vector2":
               {
                  Vector2 val=new Vector2();
                  if(attr["value"]!=null)
                  {
                     LuaObject v=attr["value"];
                     val.X = (float)v["x"];
                     val.Y = (float)v["y"];
                  }
                  Attribute<Vector2> temp = new Attribute<Vector2>(e, name, val);
                  break;
               }
            case "vector3":
               {
                  Vector3 val = new Vector3();
                  if(attr.contains("value")==true)
                  {
                     LuaObject v = attr["value"];
                     val.X = (float)(double)v["x"];
                     val.Y = (float)(double)v["y"];
                     val.Z = (float)(double)v["z"];
                  }
                  Attribute<Vector3> temp = new Attribute<Vector3>(e, name, val);
                  break;
               }
            case "vector4":
               {
                  Vector4 val = new Vector4();
                  if(attr.contains("value")==true)
                  {
                     LuaObject v = attr["value"];
                     val.X = (float)v["x"];
                     val.Y = (float)v["y"];
                     val.Z = (float)v["z"];
                     val.W = (float)v["w"];
                  }
                  Attribute<Vector4> temp = new Attribute<Vector4>(e, name, val);
                  break;
               }
            case "quaternion":
               {
                  Quaternion val = new Quaternion();
                  if(attr.contains("value")==true)
                  {
                     LuaObject v = attr["value"];
                     val.X = (float)v["x"];
                     val.Y = (float)v["y"];
                     val.Z = (float)v["z"];
                     val.W = (float)v["w"];
                  }
                  Attribute<Quaternion> temp = new Attribute<Quaternion>(e, name, val);
                  break;
               }
            default:
               Error.print("Unknown type of attribute {0} in entity type {1}", (String)attr["type"], entityType);
               break;
         }
      }

      public void addCreator(BehaviorCreator creator)
      {
         myCreators.Add(creator.name, creator);
      }

      public void removeCreator(String name)
      {
         myCreators.Remove(name);
      }
   }
}