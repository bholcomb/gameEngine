using System;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Graphics
{
   public class MaterialAttribute
   {
      public String name { get; set; }
      public Object myValue { get; set; }

      public MaterialAttribute(string matName)
      {
         name = matName;
      }

      public Object value()
      {
         return myValue;
      }
   }

   public class ColorAttribute : MaterialAttribute
   {
      public ColorAttribute(string name, Color4 c)
         : base(name)
      {
         myValue = c;
      }

      public new Color4 value()
      {
         return (Color4)myValue;
      }
   }

   public class TextureAttribute : MaterialAttribute
   {
      public TextureAttribute(string name, Texture t)
         : base(name)
      {
         myValue = t;
      }

      public new Texture value()
      {
         return (Texture)myValue;
      }

      public bool hasTransparency()
      {
         return ((Texture)myValue).hasAlpha;
      }
   }

   public class ValueAttribute : MaterialAttribute
   {
      public ValueAttribute(string name, float t)
         : base(name)
      {
         myValue = t;
      }

      public new float value()
      {
         return (float)myValue;
      }
   }

   public class Vec3Attribute : MaterialAttribute
   {
      public Vec3Attribute(string name, Vector3 t)
         : base(name)
      {
         myValue = t;
      }

      public new Vector3 value()
      {
         return (Vector3)myValue;
      }
   }

   public class Material
   {
		[Flags]
		public enum Feature : UInt32
		{
			Color					= 0x0001,
			Lighting				= 0x0002,
			DiffuseMap			= 0x0004,
			SpecMap				= 0x0008,
			NormalMap			= 0x0010,
         ParallaxMap       = 0x0020,
			DetailMap			= 0x0040,
			DisplacementMap	= 0x0080,
			EmmissiveMap		= 0x0100,
         AlphaMap          = 0x0200,
			ReflectionMap		= 0x0400,
			Skybox				= 0x0800,
		}

		public enum TextureId
		{
			Diffuse,
			Specular,
			Normal,
			Detail,
			Displacement,
			Emissive,
         Alpha,
			Reflection,
         Skybox
		}

		public String name { get; protected set; }
		public Feature myFeatures;
		public UInt32 effectType { get { return (UInt32)myFeatures; } }
      Dictionary<string, MaterialAttribute> myAttributes = new Dictionary<string, MaterialAttribute>();

		public TextureAttribute[] myTextures = new TextureAttribute[10];
 
      float myAlpha;
      public Color4 ambient { get; set; }
      public Color4 diffuse { get; set; }
      public Color4 spec { get; set; }
      public Color4 emission { get; set; }
      public float shininess { get; set; }
      public float alpha { get { return myAlpha; } set { if (value < 1.0f) hasTransparency = true; myAlpha = value;  } }

      public float alphaTest { get; set; }
      public bool hasTransparency { get; set; }

      public Material(String n ="")
      {
         name = n;

         //default material properties
         ambient = new Color4(0.1f, 0.1f, 0.1f, 1.0f);
         diffuse = new Color4(0.8f, 0.8f, 0.8f, 1.0f);
         spec = new Color4(0.1f, 0.1f, 0.1f, 1.0f);
         emission = new Color4(0.0f, 0.0f, 0.0f, 1.0f);
         shininess = 128.0f;
         alpha = 1.0f;
         alphaTest = 0.0f;
         hasTransparency = false;
      }
      
      public bool hasAttribute(string name)
      {
         MaterialAttribute ret;
         return myAttributes.TryGetValue(name, out ret);
      }

      public MaterialAttribute findAttribute(string name)
      {
         MaterialAttribute ret;
         if (!myAttributes.TryGetValue(name, out ret))
         {
            return null;
         }

         return ret;
      }

      public void addAttribute(MaterialAttribute obj)
      {
         myAttributes[obj.name] = obj;

         if(obj is TextureAttribute)
         {
            TextureAttribute tex = obj as TextureAttribute;
            switch(tex.name)
            {
               case "diffuseMap":
                  myTextures[(int)TextureId.Diffuse] = tex;
                  //hasTransparency = tex.hasTransparency();
                  alphaTest = tex.hasTransparency() ? 0.1f : 0.0f;
                  break;
               case "specularMap":
                  myTextures[(int)TextureId.Specular] = tex;
                  break;
               case "normalMap":
                  myTextures[(int)TextureId.Normal] = tex;
                  break;
               case "detailMap":
                  myTextures[(int)TextureId.Detail] = tex;
                  break;
               case "displaceMap":
                  myTextures[(int)TextureId.Displacement] = tex;
                  break;
               case "emissiveMap":
                  myTextures[(int)TextureId.Emissive] = tex;
                  break;
               case "reflectionMap":
                  myTextures[(int)TextureId.Reflection] = tex;
                  break;
               case "skybox":
                  myTextures[(int)TextureId.Skybox] = tex;
                  break;
            }
         }
      }
   }
}