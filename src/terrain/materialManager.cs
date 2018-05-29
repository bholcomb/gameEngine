using System;
using System.Collections.Generic;

using OpenTK;

using Graphics;
using Util;

namespace Terrain
{ 
   public static class MaterialManager
   {
      public static Dictionary<UInt32, Material> myMaterials = new Dictionary<UInt32, Material>();
      static Material myDefaultMaterial = new Material(0, "air", -1, -1, -1, 1.0f, Material.Property.AIR);
      static Material myCompositeSolidMaterial = new Material(0, "compositeSolid", -1, -1, -1, 1.0f, Material.Property.SOLID);
      static Material myCompositeTransparentMaterial = new Material(0, "compositeTransparent", -1, -1, -1, 1.0f, Material.Property.TRANSPARENT);
      static bool myGenerateTextures = false;
      static List<string> myTextureFilenames = new List<string>();
      static int myNextIndex = 0;
      public static ArrayTexture myMaterialTextureArray;

		public static Graphics.Material visualMaterial = new Graphics.Material("terrain");

      static MaterialManager()
      {
         //default material
         myMaterials.Add(0, myDefaultMaterial);
         myMaterials.Add(1, myCompositeSolidMaterial);
         myMaterials.Add(2, myCompositeTransparentMaterial);
      }

      public static void init(bool generateTextures=false)
      {
         myGenerateTextures = generateTextures;
         reset();
      }

      public static void reset()
      {
         myMaterials.Clear();
         myTextureFilenames.Clear();
         loadMaterials();
         createTextureArray();
      }

      public static Material getMaterial(UInt32 matId)
      {
         Material mat;
         if (myMaterials.TryGetValue(matId, out mat) == true)
         {
            return mat;
         }

         //return the default material
         return myDefaultMaterial;
      }

      public static Material getMaterial(String name)
      {
         foreach (Material m in myMaterials.Values)
         {
            if (m.name == name)
               return m;
         }

         return myDefaultMaterial;
      }

      public static UInt32 getMaterialIndex(String name)
      {
         UInt32 ret = 0;
         foreach (KeyValuePair<UInt32, Material> m in myMaterials)
         {
            if (m.Value.name == name)
            {
               return m.Key;
            }
         }

         return ret;
      }

      public static Material defaultMaterial { get { return myDefaultMaterial; } }
      public static Material compositeSolidMaterial { get { return myCompositeSolidMaterial; } }
      public static Material compositeTransparentMaterial { get { return myCompositeTransparentMaterial; } }

      public static void loadMaterial(JsonObject data)
      {
         string name = (string)data["name"];
         string property = (string)data["property"];
         string tf = (string)data["topFilename"];
         string sf = (string)data["sideFilename"];
         string bf = (string)data["bottomFilename"];

         UInt32 index = Hash.hash(name);      

         float scale = 1.0f;
         if (data.contains("scale") == true)
         {
            scale = (float)data["scale"];
         }

         Material.Property prop;
         switch (property)
         {
            case "animated": prop = Material.Property.ANIMATED; break;
            case "clip": prop = Material.Property.CLIP; break;
            case "water": prop = Material.Property.WATER; break;
            case "lava": prop = Material.Property.DEATH; break;
            case "transparent": prop = Material.Property.TRANSPARENT; break;
            default: prop = Material.Property.SOLID; break;
         }

         int t, s, b;
         t = s = b = -1;
         if (tf != "")
         {
            if (tf != "null")
            {
               myTextureFilenames.Add(tf);
               t = myTextureFilenames.Count - 1;
            }
            else
            {
               t = 0x3ff;
            }
         }

         if (sf != "")
         {
            if (sf != "null")
            {
               myTextureFilenames.Add(sf);
               s = myTextureFilenames.Count - 1;
            }
            else
            {
               s = 0x3ff;
            }
         }

         if (bf != "")
         {
            if (bf != "null")
            {
               myTextureFilenames.Add(bf);
               b = myTextureFilenames.Count - 1;
            }
            else
            {
               b = 0x3ff;
            }
         }

         if ((s != -1) && (b != -1))
            myMaterials.Add(index, new Material(index, name, t, s, b, scale / WorldParameters.theChunkSize, prop));
         else
            myMaterials.Add(index, new Material(index, name, t, t, t, scale / WorldParameters.theChunkSize, prop));
      }

      public static void loadMaterials()
      {
         string [] files=System.IO.Directory.GetFiles("../data/materials");
         foreach(string s in files)
         {
            if (System.IO.Path.GetExtension(s) == ".json")
            {
               System.IO.StreamReader file = new System.IO.StreamReader(s);
               string textData= file.ReadToEnd();
               file.Close();

               JsonObject mats = new JsonObject(textData);
               mats=mats["Materials"];
               for(int i=0; i<mats.count(); i++)
               {
                  JsonObject mat=mats[i];
                  loadMaterial(mat);
               }
            }
         }
      }

      public static void createTextureArray()
      {
         if (myGenerateTextures)
         {
            ArrayTextureDescriptor td = new ArrayTextureDescriptor(myTextureFilenames.ToArray(), true);
            myMaterialTextureArray = Renderer.resourceManager.getResource(td) as ArrayTexture;
				visualMaterial.addAttribute(new TextureAttribute("texArray", myMaterialTextureArray));
            visualMaterial.upload();
         }
      }
   }
}