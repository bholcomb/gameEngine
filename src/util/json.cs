using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using OpenTK.Graphics;

namespace Util
{
   public class JsonObject
   {
      public enum JsonType { NONE, OBJECT, ARRAY, INTEGER, DOUBLE, STRING, BOOL, NULL };
      JsonType myType = JsonType.NONE;
      Object myValue = null;
      String myName = "";
      JsonObject myParent = null;

      public static JsonObject theNullObject;

      public static JsonObject loadFile(String path)
      {
         if (System.IO.File.Exists(path))
         {
            System.IO.StreamReader file = new System.IO.StreamReader(path);
            String data = file.ReadToEnd();
            file.Close();
            return new JsonObject(data);
         }

         Warn.print("Cannot find JSON file {0}", path);
         return null;
      }

      #region constructors
      
      static JsonObject()
      {
         theNullObject = new JsonObject();
      }

      public JsonObject(JsonType t=JsonType.NONE, Object val=null)
      {
         myType = t;
         switch (myType)
         {
            case JsonType.OBJECT: myValue=new Dictionary<String, JsonObject>();break;
            case JsonType.ARRAY: myValue=new List<JsonObject>(); break;
            case JsonType.INTEGER: myValue = Convert.ToInt64(val==null ? 0 : val); break;
            case JsonType.DOUBLE: myValue = Convert.ToDouble(val==null ? 0.0 : val); break;
            case JsonType.STRING: myValue = Convert.ToString(val==null ? "" : val); break;
            case JsonType.BOOL: myValue = Convert.ToBoolean(val==null ? false : val); break;
            case JsonType.NULL: myValue = null; break;
         }
      }

      public JsonObject(String json)
      {
         JsonObject ret = parse(json);
         myType = ret.myType;
         myValue = ret.myValue;
         myName = ret.myName;
         myParent = ret.myParent;
      }
      #endregion

      #region enumerators
      public int count()
      {
         if (myType == JsonType.ARRAY)
         {
            return ((List<JsonObject>)myValue).Count;
         }

         if (myType == JsonType.OBJECT)
         {
            return ((Dictionary<String, JsonObject>)myValue).Count;
         }

         return 0;
      }

      public IEnumerable<JsonObject> elements
      {
         get
         {
            if (myType == JsonType.ARRAY)
            {
               foreach(JsonObject j in (List<JsonObject>)myValue)
                  yield return j;
            }

            if (myType == JsonType.OBJECT)
            {
               foreach (JsonObject j in ((Dictionary<String, JsonObject>)myValue).Values)
                  yield return j;
            }
         }
      }

      public IEnumerable<String> keys
      {
         get
         {
            if (myType == JsonType.OBJECT)
            {
               foreach (String j in ((Dictionary<String, JsonObject>)myValue).Keys)
                  yield return j;
            }
         }
      }

      #endregion

      #region Accessors
      public String name { get { return myName; } set {myName=value;}}

      public JsonType type { get { return myType; } }

      public bool contains(String name)
      {
         if (myType != JsonType.OBJECT)
         {
            return false;
         }

         Dictionary<string, JsonObject> dict = (Dictionary<string, JsonObject>)myValue;
         return dict.ContainsKey(name);
      }
      
      public JsonObject this[int index]
      {
         get
         {
            if (myType != JsonType.ARRAY || index < 0 || index >= ((List<JsonObject>)myValue).Count)
            {
               return theNullObject;
            }

            return ((List<JsonObject>)myValue)[index];
         }
         set
         {
            if (myType != JsonType.ARRAY)
            {
               throw new Exception("Cannot set index in non-array object");
            }

            if (myValue == null)
               myValue = new List<JsonObject>();

            List<JsonObject> jl = (List<JsonObject>)myValue;
            if (jl.Count < index+1)
            {
               jl.Capacity = index+1;
               for (int i = jl.Count - 1; i < index; i++)
               {
                  jl.Add(null);
               }
            }

            jl[index] = value;
            value.myParent = this;
         }
      }

      public JsonObject this[string index]
      {
         get
         {
            if (myType != JsonType.OBJECT)
            {
               return theNullObject;
            }

            Dictionary<string, JsonObject> dict = (Dictionary<string, JsonObject>)myValue;
            JsonObject outObject;
            if (dict.TryGetValue(index, out outObject))
            {
               return outObject;
            }

            return theNullObject;
         }
         set
         {
            if (myType != JsonType.OBJECT)
            {
               throw new Exception("Cannot set index in non-object value");
            }

            if (myValue == null)
               myValue = new Dictionary<String, JsonObject>();

            Dictionary<String, JsonObject> jd = (Dictionary<String, JsonObject>)myValue;

            jd[index] = value;
            value.myName = index;
            value.myParent = this;
         }
      }

      public Object raw { get { return myValue; } }

      #endregion

      #region explicit casting
      public static explicit operator string(JsonObject j)
      {
         switch (j.myType)
         {
            case JsonType.STRING:
               {
                  return System.Convert.ToString(j.myValue);
               }
            default:
               {
                  return "";
               }
         }
      }

      public static explicit operator double(JsonObject j)
      {
         switch (j.myType)
         {
            case JsonType.INTEGER:
               {
                  return System.Convert.ToDouble(j.myValue);
               }
            case JsonType.DOUBLE:
               {
                  return System.Convert.ToDouble(j.myValue);
               }
            default:
               {
                  return double.NaN;
               }
         }
      }

      public static explicit operator float(JsonObject j)
      {
         switch (j.myType)
         {
            case JsonType.INTEGER:
               {
                  return System.Convert.ToSingle(j.myValue);
               }
            case JsonType.DOUBLE:
               {
                  return System.Convert.ToSingle(j.myValue);
               }
            default:
               {
                  return float.NaN;
               }
         }
      }

      public static explicit operator long(JsonObject j)
      {
         switch (j.myType)
         {
            case JsonType.INTEGER:
               {
                  return System.Convert.ToInt64(j.myValue);
               }
            case JsonType.DOUBLE:
               {
                  return System.Convert.ToInt64(j.myValue);
               }
            default:
               {
                  return long.MaxValue;
               }
         }
      }

      public static explicit operator int(JsonObject j)
      {
         switch (j.myType)
         {
            case JsonType.INTEGER:
               {
                  return System.Convert.ToInt32(j.myValue);
               }
            case JsonType.DOUBLE:
               {
                  return System.Convert.ToInt32(j.myValue);
               }
            default:
               {
                  return int.MaxValue;
               }
         }
      }

      public static explicit operator bool(JsonObject j)
      {
         switch (j.myType)
         {
            case JsonType.BOOL:
               {
                  return System.Convert.ToBoolean(j.myValue);
               }
            case JsonType.INTEGER:
               {
                  return System.Convert.ToInt64(j.myValue) != 0;
               }
            case JsonType.DOUBLE:
               {
                  return System.Convert.ToDouble(j.myValue) != 0;
               }
            default:
               {
                  return false;
               }
         }
      }

      public static explicit operator Vector3(JsonObject j)
      {
         Vector3 v = Vector3.Zero;
         if (j.myType == JsonType.OBJECT)
         {
            v.X = (float)j["x"];
            v.Y = (float)j["y"];
            v.Z = (float)j["z"];
         }
         else if (j.myType == JsonType.ARRAY)
         {
            v.X = (float)j[0];
            v.Y = (float)j[1];
            v.Z = (float)j[2];
         }

         return v;
      }

      public static explicit operator Color4(JsonObject j)
      {
         Color4 v = Color4.White;
         if (j.myType == JsonType.OBJECT)
         {
            v.R = (float)j["r"];
            v.G = (float)j["g"];
            v.B = (float)j["b"];
            v.A = (float)j["a"];
         }
         else if (j.myType == JsonType.ARRAY)
         {
            v.R = (float)j[0];
            v.G = (float)j[1];
            v.B = (float)j[2];
            v.A = (float)j[3];
         }

         return v;
      }

      public static implicit operator JsonObject(double d)
      {
         return new JsonObject(JsonType.DOUBLE, d);
      }

      public static implicit operator JsonObject(float d)
      {
         return new JsonObject(JsonType.DOUBLE, d);
      }

      public static implicit operator JsonObject(long d)
      {
         return new JsonObject(JsonType.INTEGER, d);
      }

      public static implicit operator JsonObject(int d)
      {
         return new JsonObject(JsonType.INTEGER, d);
      }

      public static implicit operator JsonObject(String d)
      {
         return new JsonObject(JsonType.STRING, d);
      }

      public static implicit operator JsonObject(bool d)
      {
         return new JsonObject(JsonType.BOOL, d);
      }

      public static implicit operator JsonObject(Vector3 d)
      {
         JsonObject obj = new JsonObject(JsonType.OBJECT);
         obj["x"] = d.X;
         obj["y"] = d.Y;
         obj["z"] = d.Z;
         return obj;
      }

      public static implicit operator JsonObject(Color4 d)
      {
         JsonObject obj = new JsonObject(JsonType.OBJECT);
         obj["r"] = d.R;
         obj["g"] = d.G;
         obj["b"] = d.B;
         obj["a"] = d.A;
         return obj;
      }
      #endregion

      #region Parser
      bool isDigit(Char c)
      {
         return c >= '0' && c <= '9';
      }

      int asNumber(Char c)
      {
         return c - '0';
      }

      void append(JsonObject a, JsonObject b)
      {
         if (a.myType != JsonType.OBJECT && a.myType != JsonType.ARRAY)
            throw new Exception("Cannot append object to non-object or non-list");

         b.myParent = a;

         if (a.myType==JsonType.OBJECT)
         {
            a[b.myName] = b;
         }

         if (a.myType == JsonType.ARRAY)
         {
            List<JsonObject> jl = (List<JsonObject>)a.myValue;
            jl.Add(b);
         }
      }

      JsonObject parse(String json)
      {
         JsonObject root = null;
         JsonObject top = null;
         String name=null;

         Char[] source = json.ToCharArray();
         int i=0;
         while(i < source.Length)
         {
            Char b = source[i];
            switch (b)
            {
               case '{':
                  {
                     JsonObject obj=new JsonObject(JsonType.OBJECT);
                     obj.myName=name;
                     name=null;
                     ++i;
                     if(top!=null)
                     {
                        append(top, obj);
                     }
                     else if(root==null)
                     {
                        root=obj;
                     }
                     else
                     {
                        throw new Exception("Second Root.  Only 1 root object allowed");
                     }

                     top=obj;
                  }
                  break;
               case '[':
                  {
                     JsonObject obj=new JsonObject(JsonType.ARRAY);
                     obj.myName=name;
                     name=null;
                     ++i;
                     if(top!=null)
                     {
                        append(top, obj);
                     }
                     top=obj;
                  }
                  break;
               case '}':
                  {
                     if(top==null || top.myType!=JsonType.OBJECT)
                        throw new Exception("Mismatch of closing brace");

                     i++;
                     top=top.myParent;

                  }
                  break;
               case ']':
                  {
                     if(top==null || top.myType!=JsonType.ARRAY)
                     throw new Exception("Mismatch of closing bracket");

                     i++;
                     top=top.myParent;
                  }
                  break;
               case ':':
                  {
                     if(top==null || top.myType!=JsonType.OBJECT)
                        throw new Exception(String.Format("Unexpected character {0} at character {1}", b, i));

                     i++;
                  }
                  break;
               case ',':
                  {
                     if (top == null)
                        throw new Exception(String.Format("Unexpected character {0} at character {1}", b, i));
                     i++;
                  }
                  break;
               case '"':
                  {
                     if (top == null)
                        throw new Exception(String.Format("Unexpected character {0} at character {1}", b, i));
                     i++;

                     String str="";

                     while(i < source.Length)
                     {
                        if(source[i] < '\x20')
                           throw new Exception("Control characters not allowed in strings");

                        if(source[i]=='\\')
                        {
                           switch(source[i+1])
                           {
                              case '"':
							            str+="\"";
							            break;
						            case '\\':
							            str += '\\';
							            break;
						            case '/':
							            str += '/';
							            break;
						            case 'b':
							            str += '\b';
							            break;
						            case 'f':
							            str += '\f';
							            break;
						            case 'n':
							            str += '\n';
							            break;
						            case 'r':
							            str += '\r';
							            break;
						            case 't':
							            str += '\t';
							            break;
						            case 'u':
							            {
                                    throw new Exception("Unicode not supported yet");
                                    i+=4;
								         }
                                 break;
                              default:
                                 throw new Exception("Unrecognized escape sequence");
							      }
                        }
                        else if(source[i]=='"')
                        {
                           i++;
                           break;
                        }
                        else
                        {
                           str+=source[i];
                           i++;
                        }
                     }

                     if(name==null && top.myType==JsonType.OBJECT)
                     {
                        name=str;
                     }
                     else
                     {
                        JsonObject obj=new JsonObject(JsonType.STRING);
                        obj.myName=name;
                        name=null;
                        obj.myValue=str;
                        append(top, obj);
                     }
                  }
                  break;
               case 'n':
               case 't':
               case 'f':
                  {
                    if (top == null)
                       throw new Exception(String.Format("Unexpected character {0} at character {1}", b, i));
                        
                     JsonObject obj=new JsonObject();
                     obj.myName=name;
                     name=null;

                     if(source[i]=='n' && source[i+1]=='u' && source[i+2]=='l' && source[i+3]=='l')
                     {
                        obj.myType=JsonType.NULL;
                        i+=4;
                     }
                     else if(source[i]=='t' && source[i+1]=='r' && source[i+2]=='u' && source[i+3]=='e')
                     {
                        obj.myType=JsonType.BOOL;
                        obj.myValue=true;
                        i+=4;
                     }
                     else if(source[i]=='f' && source[i+1]=='a' && source[i+2]=='l' && source[i+3]=='s' && source[i+4]=='e')
                     {
                        obj.myType=JsonType.BOOL;
                        obj.myValue=false;
                        i+=5;
                     }
                     else
                     {
                        throw new Exception("Unknown identifier");
                     }

                     append(top, obj);
                  }
                  break;
               case '-':
               case '0':
               case '1':
               case '2':
               case '3':
               case '4':
               case '5':
               case '6':
               case '7':
               case '8':
               case '9':
                  {
                     if (top == null)
                        throw new Exception(String.Format("Unexpected character {0} at character {1}", b, i));
                     
                     JsonObject obj = new JsonObject();
                     obj.myName=name;
                     name=null;
                     obj.myType=JsonType.INTEGER;

                     int first=i;
                     while(source[i]!='\x20' && source[i]!='\x9' && source[i]!='\xD' && source[i]!='\xA' && source[i]!=',' && source[i]!=']' && source[i]!='}')
                     {
                        if(source[i]=='.' || source[i]=='e' || source[i]=='E')
                        {
                           obj.myType=JsonType.DOUBLE;
                        }
                        i++;
                     }
                     if(obj.myType==JsonType.INTEGER)
                     {
                        string val="";
                        for(int j=first; j<i; j++)
                        {
                           val+=source[j];
                        }
                        obj.myValue=Convert.ToInt64(val);
                     }
                     if(obj.myType==JsonType.DOUBLE)
                     {
                        string val="";
                        for(int j=first; j<i; j++)
                        {
                           val+=source[j];
                        }
                        obj.myValue=Convert.ToDouble(val);
                     }

                     append(top, obj);
                  }
                  break;
               default:
                  throw new Exception(String.Format("Unexpected character {0} at character {1}", b, i));
            }

            while(i<source.Length && 
                  (source[i]=='\x20' ||
                  source[i]=='\x9' ||
                  source[i]=='\xD' ||
                  source[i] == '\xA'))
            {
               ++i;
            }
         }

         if (top!=null)
         {
            throw new Exception("Not all objects/arrays have been properly closed");
         }

         return root;
      }
      #endregion

      #region String Generator
      public String generateString()
      {
         String ret = "";
         generateNode(ref ret);
         return ret;
      }

      static int indentLevel = -1;
      void indent(ref String ret)
      {
         for(int i=0; i< indentLevel; i++)
         {
            ret +="\t";
         }
      }

      void generateNode(ref String ret)
      {
         indentLevel++;
         switch (myType)
         {
            case JsonType.OBJECT:
               {
                  ret += '{';
                  ret += "\n";
                  indent(ref ret);
                  Dictionary<String, JsonObject> dict=(Dictionary<String, JsonObject>)myValue;
                  int counter = dict.Count;
                  foreach(KeyValuePair<String, JsonObject> entry in dict)
                  {
                     ret += "\"";
                     ret += entry.Key;
                     ret += "\": ";
                     entry.Value.generateNode(ref ret);
                     
                     counter--;
                     if (counter >= 1)
                     {
                        ret += ", ";
                     }
                     ret += "\n";
                     indent(ref ret);
                  }
                  ret += "}";
               }
               break;
            case JsonType.ARRAY:
               {
                  ret += "[";
                  List<JsonObject> list = (List<JsonObject>)myValue;
                  int counter = list.Count;
                  foreach (JsonObject j in list)
                  {
                     j.generateNode(ref ret);
                     counter--;
                     if (counter >= 1)
                     {
                        ret += ", ";
                     }
                  }

                  ret += "]";
               }
               break;
            case JsonType.NULL:
               {
                  ret += "null";
               }
               break;
            case JsonType.BOOL:
               {
                  bool val = Convert.ToBoolean(myValue);
                  if (val == true)
                     ret += "true";
                  else
                     ret += "false";
               }
               break;
            case JsonType.INTEGER:
               {
                  int val = Convert.ToInt32(myValue);
                  ret += Convert.ToString(val);
               }
               break;
            case JsonType.DOUBLE:
                {
                  double val = Convert.ToDouble(myValue);
                  ret += Convert.ToString(val);
               }
               break;
            case JsonType.STRING:
               {
                  string val = Convert.ToString(myValue);
                  ret += "\"";
                  ret += val;
                  ret += "\"";
               }
               break;
            case JsonType.NONE:
               throw new Exception("Can't write a none object-shouldn't get here");
               break;
         }

         indentLevel--;
      }
      #endregion
   }
}
