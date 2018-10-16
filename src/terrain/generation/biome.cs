using System;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;
using Util;

namespace Terrain
{
   public class Biome
   {
      public class FeatureProbability
      {
         public float min;
         public float max;
         public String name;
         public uint id;
      }

      public List<FeatureProbability> myTopProbabilities = new List<FeatureProbability>();
      public List<FeatureProbability> mySoilProbabilities = new List<FeatureProbability>();
      public List<FeatureProbability> myVegitationProbabilities = new List<FeatureProbability>();

      public string name { get; set; }
      public FeatureProbability temperatureRange { get; set; }
      public FeatureProbability moistureRange { get; set; }

      public Biome(JsonObject initData)
      {
         temperatureRange = new FeatureProbability();
         moistureRange = new FeatureProbability();

         name = initData.name;
         JsonObject tempData = initData["TemperatureRange"];
         temperatureRange.name = "temperature";
         temperatureRange.min = (float)tempData["min"];
         temperatureRange.max = (float)tempData["max"];

         JsonObject moistData = initData["MoistureRange"];
         moistureRange.name = "moisture";
         moistureRange.min = (float)moistData["min"];
         moistureRange.max = (float)moistData["max"];

         foreach (JsonObject feature in initData["SoilProbability"].elements)
         {
            FeatureProbability prob=new FeatureProbability();
            prob.min = (float)feature["min"];
            prob.max = (float)feature["max"];
            prob.name = feature.name;
            prob.id = Hash.hash(feature.name);
            mySoilProbabilities.Add(prob);
         }

         foreach (JsonObject feature in initData["TopProbability"].elements)
         {
            FeatureProbability prob = new FeatureProbability();
            prob.min = (float)feature["min"];
            prob.max = (float)feature["max"];
            prob.name = feature.name;
            prob.id = Hash.hash(feature.name);
            myTopProbabilities.Add(prob);
         }

         foreach (JsonObject feature in initData["VegitationProbability"].elements)
         {
            FeatureProbability prob = new FeatureProbability();
            prob.min = (float)feature["min"];
            prob.max = (float)feature["max"];
            prob.name = feature.name;
            myVegitationProbabilities.Add(prob);
         }
      }

      JsonObject featureDefinition(FeatureProbability feature)
      {
         JsonObject data = new JsonObject(String.Format("{\"min\": {0}, \"max\":{1}}", feature.min, feature.max));
         data.name = name;
         return data;
      }

      public JsonObject getDefinition()
      {
         JsonObject data = new JsonObject(JsonObject.JsonType.OBJECT);
         data.name = name;
         data["TemperatureRange"] = featureDefinition(temperatureRange);
         data["MoistureRange"] = featureDefinition(moistureRange);

         JsonObject soils = new JsonObject(JsonObject.JsonType.OBJECT);
         foreach (FeatureProbability fp in mySoilProbabilities)
         {
            soils[fp.name] = featureDefinition(fp);
         }
         data["SoilProbability"] = soils;

         JsonObject tops = new JsonObject(JsonObject.JsonType.OBJECT);
         foreach (FeatureProbability fp in myTopProbabilities)
         {
            tops[fp.name] = featureDefinition(fp);
         }
         data["TopProbability"] = tops;

         JsonObject vegs = new JsonObject(JsonObject.JsonType.OBJECT);
         foreach (FeatureProbability fp in myVegitationProbabilities)
         {
            vegs[fp.name] = featureDefinition(fp);
         }
         data["VegitationProbability"] = vegs;

         return data;
      }


      uint topProbability(float prob)
      {
         foreach (FeatureProbability sp in myTopProbabilities)
         {
            if (prob > sp.min && prob < sp.max)
               return sp.id;
         }

         return 0;
      }

      uint soilProbability(float prob)
      {
         foreach (FeatureProbability sp in mySoilProbabilities)
         {
            if (prob > sp.min && prob < sp.max)
               return sp.id;
         }

         return 0;
      }

      String vegitationProbability(float prob)
      {
         foreach (FeatureProbability sp in myVegitationProbabilities)
         {
            if (prob > sp.min && prob < sp.max)
               return sp.name;
         }

         return "none";
      }

      Color4 vegitationColor(int x, int z)
      {
         return new Color4(10, 255, 100, 255);
      }
   }
}