/****************************************************************************** 

Copyright (c) 2018 Apexica LLC 
All rights reserved. 

Author: Robert C. Holcomb Jr.

******************************************************************************/

using System;
using System.IO;
using System.Collections.Generic;

using OpenTK;
using Util;
using Lua;
using Engine;

using OpenTK;
using OpenTK.Graphics;

namespace Sim
{
   public class EntityFactory
   {
      Dictionary<Int32, BehaviorCreator> myCreators = new Dictionary<Int32, BehaviorCreator>();
      EntityManager myMgr;
      EntityDatabase myDb;
      
      //0 is reserved for something else
      ulong myNextId = 1;
      ulong myAppId = Application.uniqueId;

      public EntityFactory(EntityManager mgr)
      {
         myMgr = mgr;
         myDb = mgr.db;
      }

      public bool init(Initializer init)
      {
         return true;
      }

      public Entity create(string entityType)
      {
         LuaObject template = myMgr.paramterDatabase.entityTemplate(entityType);

         if (template.type() == DataType.NIL)
         {
            Warn.print("Cannot find template for: {0}", entityType);
            return null;
         }

         using (template)
         {
            Entity e = null;

            //get parents contribution
            String inheritsFrom = template.getOr<string>("inherits", "");
            if (inheritsFrom != "")
            {
               //recursively get parents contributions
               e = create(inheritsFrom);
            }

            //if the entity hasn't been created yet, create it.  Happens in base class if inheritance is used
            if (e == null)
            {
               //high 32 bits for the process ID (rare collisions on this)
               ulong id = Application.procId;
               id = id << 32;

               //add in (or binary or) the id bits
               id += myNextId++;

               e = new Entity(id, myMgr);
               e.type = entityType;
            }

            //setup the entity type
            Attribute<String> typeAttribute = new Attribute<String>(e.state, Attributes.Type, (string)template["type"]);

            LuaObject attributes = template["attributes"];
            LuaObject behaviors = template["behaviors"];

            //create any attributes
            foreach (string attName in attributes.keys)
            {
               LuaObject attr = attributes[attName];
               createAttribute(e, attName, attr, entityType);
            }

            //create the behaviors
            foreach (String behaviorName in behaviors.keys)
            {
               BehaviorCreator bc;
               int id = findBehaviorId(behaviorName);
               if (myCreators.TryGetValue(id, out bc))
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
            String inheritsFrom = template.getOr<string>("inherits", "");
            if (inheritsFrom != "")
            {
               //recursively get parents contributions
               e = createReflected(inheritsFrom, id);
            }

            //if the entity hasn't been created yet, create it.  Happens in base class if inheritance is used
            if (e == null)
            {
               e = new Entity(id, myMgr);
            }

            LuaObject attributes = template["attributes"];
            LuaObject reflected = template["reflected"];
            LuaObject behaviors = template["behaviors"];

            //create any attributes
            foreach (string attName in attributes.keys)
            {
               LuaObject attr = attributes[attName];
               createAttribute(e, attName, attr, entityType);
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
                  int bid = findBehaviorId(behaviorName);
                  if (myCreators.TryGetValue(bid, out bc))
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

            Attribute<bool> reflectedAtt = new Attribute<bool>(e.state, Attributes.Reflected, true);
            return e;
         }
      }

      public int findAttributeId(string name)
      {
         Type type = typeof(Attributes);
         var field = type.GetField(name);
         if(field != null)
         {
            return (int)field.GetValue(null);
         }

         throw new Exception(String.Format("Failed to find attribute type {0}", name));
      }

      public int findBehaviorId(string name)
      {
         Type type = typeof(Behaviors);
         var field = type.GetField(name);
         if (field != null)
         {
            return (int)field.GetValue(null);
         }

         throw new Exception(String.Format("Failed to find behavior type {0}", name));
      }

      protected void createAttribute(Entity e, string attrName, LuaObject attr, String entityType)
      {
         int name = findAttributeId(attrName);
         bool notify = attr.contains("notify") && (bool)attr["notify"];

         switch ((String)attr["type"])
         {
            case "bool":
               {
                  bool val=false;
                  if(attr.contains("value")==true)
                  {
                     val = (bool)attr["value"];
                  }
                  Attribute<bool> temp = new Attribute<bool>(e.state, name, val);
                  if (notify)
                  {
                     temp.onValueChanged += temp.notifyUpdate;
                  }
                  break;
               }
            case "int":
               {
                  int val = 0;
                  if(attr.contains("value")==true)
                  {
                     val = (int)attr["value"];
                  }
                  Attribute<int> temp = new Attribute<int>(e.state, name, val);
                  if (notify)
                  {
                     temp.onValueChanged += temp.notifyUpdate;
                  }
                  break;
               }
            case "uint":
               {
                  uint val = 0;
                  if(attr.contains("value")==true)
                  {
                     val = (uint)attr["value"];
                  }
                  Attribute<uint> temp = new Attribute<uint>(e.state, name, val);
                  if (notify)
                  {
                     temp.onValueChanged += temp.notifyUpdate;
                  }
                  break;
               }
            case "long":
               {
                  long val = 0;
                  if(attr.contains("value")==true)
                  {
                     val = (long)attr["value"];
                  }
                  Attribute<long> temp = new Attribute<long>(e.state, name, val);
                  if (notify)
                  {
                     temp.onValueChanged += temp.notifyUpdate;
                  }
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
                  Attribute<ulong> temp = new Attribute<ulong>(e.state, name, val);
                  if (notify)
                  {
                     temp.onValueChanged += temp.notifyUpdate;
                  }
                  break;
               }
            case "float":
               {
                  float val = 0.0f;
                  if(attr.contains("value")==true)
                  {
                     val = (float)attr["value"];
                  }
                  Attribute<float> temp = new Attribute<float>(e.state, name, val);
                  if (notify)
                  {
                     temp.onValueChanged += temp.notifyUpdate;
                  }
                  break;
               }
            case "double":
               {
                  double val = 0.0;
                  if(attr.contains("value")==true)
                  {
                     val = (double)attr["value"];
                  }
                  Attribute<double> temp = new Attribute<double>(e.state, name, val);
                  if (notify)
                  {
                     temp.onValueChanged += temp.notifyUpdate;
                  }
                  break;
               }
            case "string":
               {
                  string val = "";
                  if(attr.contains("value")==true)
                  {
                     val = (string)attr["value"];
                  }
                  Attribute<string> temp = new Attribute<string>(e.state, name, val);
                  if (notify)
                  {
                     temp.onValueChanged += temp.notifyUpdate;
                  }
                  break;
               }
            case "vector2":
               {
                  Vector2 val=new Vector2();
                  if(attr["value"]!=null)
                  {
                     LuaObject v = attr["value"];
                     val.X = v.get<float>("x");
                     val.Y = v.get<float>("y");
                  }
                  Attribute<Vector2> temp = new Attribute<Vector2>(e.state, name, val);
                  if (notify)
                  {
                     temp.onValueChanged += temp.notifyUpdate;
                  }
                  break;
               }
            case "vector3":
               {
                  Vector3 val = new Vector3();
                  if(attr.contains("value")==true)
                  {
                     LuaObject v = attr["value"];
                     val.X = v.get<float>("x");
                     val.Y = v.get<float>("y");
                     val.Z = v.get<float>("z");
                  } 
                  Attribute<Vector3> temp = new Attribute<Vector3>(e.state, name, val);
                  if (notify)
                  {
                     temp.onValueChanged += temp.notifyUpdate;
                  }
                  break;
               }
            case "vector4":
               {
                  Vector4 val = new Vector4();
                  if(attr.contains("value")==true)
                  {
                     LuaObject v = attr["value"];
                     val.X = v.get<float>("x");
                     val.Y = v.get<float>("y");
                     val.Z = v.get<float>("z");
                     val.W = v.get<float>("w");
                  }
                  Attribute<Vector4> temp = new Attribute<Vector4>(e.state, name, val);
                  if (notify)
                  {
                     temp.onValueChanged += temp.notifyUpdate;
                  }
                  break;
               }
            case "quaternion":
               {
                  Quaternion val = new Quaternion();
                  if(attr.contains("value")==true)
                  {
                     LuaObject v = attr["value"];
                     val.X = v.get<float>("x");
                     val.Y = v.get<float>("y");
                     val.Z = v.get<float>("z");
                     val.W = v.get<float>("w");
                  }
                  Attribute<Quaternion> temp = new Attribute<Quaternion>(e.state, name, val);
                  if (notify)
                  {
                     temp.onValueChanged += temp.notifyUpdate;
                  }
                  break;
               }
            case "color":
               {
                  Color4 val = new Color4();
                  if (attr.contains("value") == true)
                  {
                     LuaObject v = attr["value"];
                     val.R = v.get<float>("r");
                     val.G = v.get<float>("g");
                     val.B = v.get<float>("b");
                     val.A = v.getOr<float>("a", 1.0f);
                  }
                  Attribute<Color4> temp = new Attribute<Color4>(e.state, name, val);
                  if (notify)
                  {
                     temp.onValueChanged += temp.notifyUpdate;
                  }
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

      public void removeCreator(Int32 name)
      {
         myCreators.Remove(name);
      }
   }
}