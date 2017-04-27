using System;
using System.Collections.Generic;

using OpenTK;

using Terrain;
using Util;
using Noise;

namespace Editor
{
   public class WorldDefinition
   {
      public String name { get; set; }
      public Vector3 min { get; set; }
      public Vector3 max { get; set; }
      //public Dictionary<String, ModuleTree> layers { get; set; }
      public List<Biome> biomes { get; set; }

      public WorldDefinition()
      {
         biomes = new List<Biome>();
//          layers = new Dictionary<string, ModuleTree>();
//          layers["elevation"] = new ModuleTree();
//          layers["moisture"] = new ModuleTree();
//          layers["temperature"] = new ModuleTree();
//          layers["water"] = new ModuleTree();
      }

      public bool loadFromFile(String filename)
      {
         JsonObject initData = JsonObject.loadFile(filename);
         if (initData == null)
         {
            Warn.print("Failed to load world file: {0}", filename);
            return false;
         }

         name = (string)initData["name"];
         min = (Vector3)initData["min"];
         max = (Vector3)initData["max"];

         //load layer data
         JsonObject layerDefs = initData["layers"];
         foreach (JsonObject l in layerDefs.elements)
         {
            //layers[l.name] = ModuleFactory.createTree(l);
         }

         //load biome data
         JsonObject biomesDefs = initData["biomes"];
         foreach (JsonObject b in biomesDefs.elements)
         {
            biomes.Add(new Biome(b));
         }

         return true;
      }

      public bool saveToFile(String filename)
      {
         JsonObject data = new JsonObject(JsonObject.JsonType.OBJECT);

         data["name"] = name;
         data["min"] = min;
         data["max"] = max;

         JsonObject layerDefs = new JsonObject(JsonObject.JsonType.OBJECT);
//          foreach (KeyValuePair<String, ModuleTree> tree in layers)
//          {
//             layerDefs[tree.Key] = tree.Value.getDefinition(); ;
//          }
//          data["layers"] = layerDefs;

         JsonObject biomeDef = new JsonObject(JsonObject.JsonType.OBJECT);
         foreach (Biome b in biomes)
         {
            biomeDef[b.name] = b.getDefinition();
         }
         data["biomes"] = biomeDef;

         using (System.IO.StreamWriter outfile = new System.IO.StreamWriter(filename))
         {
            outfile.Write(data.generateString());
         }

         return true;
      }
   }
}