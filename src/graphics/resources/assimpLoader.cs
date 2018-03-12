using System;
using System.IO;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using Assimp;
using Assimp.Configs;

using Util;

namespace Graphics
{
    public class AssimpLoader
   {
      protected static AssimpContext theAssimpContext;
      protected ResourceManager myResourceManager;
      protected string myRootPath;
      protected Scene myScene;

      static AssimpLoader()
      {
         theAssimpContext = new AssimpContext();
         theAssimpContext.SetConfig(new NormalSmoothingAngleConfig(66.0f));
      }

      public AssimpLoader(ResourceManager mgr)
      {
         myResourceManager = mgr;
      }

      protected Graphics.Material createMaterial(Assimp.Material am)
      {
         Graphics.Material mat = new Material(am.Name);
         mat.myFeatures |= Material.Feature.Lighting;

         if (am.HasColorAmbient)
         {
            mat.ambient = toColor(am.ColorAmbient);
         }

         if (am.HasColorDiffuse)
         {
            mat.diffuse = toColor(am.ColorDiffuse);
         }

         if (am.HasColorSpecular)
         {
            mat.spec = toColor(am.ColorSpecular);
         }

         if (am.HasShininess)
         {
            mat.shininess = am.Shininess;
         }

         if(am.HasOpacity)
         {
            mat.alpha = am.Opacity;
         }

         if(am.HasTextureAmbient)
         {
            Texture t = getTexture(am.TextureAmbient.FilePath);
            if (t != null)
            {
               mat.addAttribute(new TextureAttribute("ambientMap", t));
            }
         }

         if (am.HasTextureDiffuse)
         {
            Texture t = getTexture(am.TextureDiffuse.FilePath);
            if (t != null)
            {
               mat.addAttribute(new TextureAttribute("diffuseMap", t));
               mat.myFeatures |= Material.Feature.DiffuseMap;
               mat.hasTransparency = t.hasAlpha;
            }
         }

         if (am.HasTextureSpecular)
         {
            Texture t = getTexture(am.TextureSpecular.FilePath);
            if (t != null)
            {
               mat.addAttribute(new TextureAttribute("specularMap", t));
               mat.myFeatures |= Material.Feature.SpecMap;
            }
         }

         if (am.HasTextureNormal)
         {
            Texture t = getTexture(am.TextureNormal.FilePath);
            if (t != null)
            {
               mat.addAttribute(new TextureAttribute("normalMap", t));
               mat.myFeatures |= Material.Feature.NormalMap;
            }
         }

         if (am.HasTextureDisplacement)
         {
            Texture t = getTexture(am.TextureDisplacement.FilePath);
            if (t != null)
            {
               mat.addAttribute(new TextureAttribute("displaceMap", t));
               mat.myFeatures |= Material.Feature.DisplacementMap;
            }
         }

         if(am.HasTextureEmissive)
         {
            Texture t = getTexture(am.TextureEmissive.FilePath);
            if (t != null)
            {
               mat.addAttribute(new TextureAttribute("emissiveMap", t));
               mat.myFeatures |= Material.Feature.EmmissiveMap;
            }
         }

         if(am.HasTextureOpacity)
         {
            Texture t = getTexture(am.TextureOpacity.FilePath);
            if (t != null)
            {
               mat.addAttribute(new TextureAttribute("alphaMap", t));
               mat.myFeatures |= Material.Feature.AlphaMap;
            }
         }

         return mat;
      }

      protected Texture getTexture(string filepath)
      {
         Texture t = null;

         if(filepath[0] == '*') //this is an embedded texture
         {
            string textureIndexStr = filepath.TrimStart('*');
            int index = Convert.ToInt32(textureIndexStr);
            if(index >= myScene.TextureCount)
            {
               Warn.print("texture index({0}) is out of range({1})", index, myScene.TextureCount);
               return null;
            }

            EmbeddedTexture texData = myScene.Textures[index];
            if(texData != null)
            {
               if(texData.IsCompressed)
               {
                  byte[] bytes = texData.CompressedData;
                  switch(texData.CompressedFormatHint)
                  {
                     case "png": //fallthrough
                     case "jpg":
                        t = new Texture();
                        Stream stream = new MemoryStream(texData.CompressedData, false);
                        t.loadFromStream(stream);
                        break;
                     case "dds":
                        Warn.print("DDS files not supported yet");
                        break;
                     default:
                        Warn.print("Unkown compressed file format {0}", texData.CompressedFormatHint);
                        break;
                  }
               }
               else
               {
                  byte[] bytes = new byte[texData.Width * texData.Height * 4];
                  for(int i = 0; i < texData.Height; i++)
                     for(int j = 0; i< texData.Width; i++)
                     {
                        bytes[j + (i * texData.Width) + 0] = texData.NonCompressedData[j + (i * texData.Width)].R;
                        bytes[j + (i * texData.Width) + 1] = texData.NonCompressedData[j + (i * texData.Width)].G;
                        bytes[j + (i * texData.Width) + 2] = texData.NonCompressedData[j + (i * texData.Width)].B;
                        bytes[j + (i * texData.Width) + 3] = texData.NonCompressedData[j + (i * texData.Width)].A;
                     }

                  Texture.PixelData pData = new Texture.PixelData();
                  pData.data = bytes;
                  pData.dataType = PixelType.Byte;
                  pData.pixelFormat = PixelFormat.Rgba;
                  t = new Texture(texData.Width, texData.Height, PixelInternalFormat.Rgba8, pData, true);
               }
            }
         }
         else //just a path name
         {
            string textureName = filepath.TrimStart('/');
            TextureDescriptor td = new TextureDescriptor(Path.Combine(myRootPath, textureName), true);
            t = myResourceManager.getResource(td) as Texture;

            if(t == null)
            {
               Warn.print("Failed to load texture {0}", filepath);
            }
         }

         return t;
      }

      #region helper conversion functions

      protected Vector2 toVector2D(Vector3D vec)
      {
         Vector2 v;
         v.X = vec.X;
         v.Y = vec.Y;
         return v;
      }

      protected Vector3 toVector(Vector3D vec)
      {
         Vector3 v;
         v.X = vec.X;
         v.Y = vec.Y;
         v.Z = vec.Z;
         return v;
      }

      protected Color4 toColor(Color4D color)
      {
         Color4 c;
         c.R = color.R;
         c.G = color.G;
         c.B = color.B;
         c.A = color.A;
         return c;
      }

      protected Matrix4 toMatrix(Matrix4x4 mat)
      {
         Matrix4 m = new Matrix4();
         m.M11 = mat.A1;
         m.M12 = mat.A2;
         m.M13 = mat.A3;
         m.M14 = mat.A4;
         m.M21 = mat.B1;
         m.M22 = mat.B2;
         m.M23 = mat.B3;
         m.M24 = mat.B4;
         m.M31 = mat.C1;
         m.M32 = mat.C2;
         m.M33 = mat.C3;
         m.M34 = mat.C4;
         m.M41 = mat.D1;
         m.M42 = mat.D2;
         m.M43 = mat.D3;
         m.M44 = mat.D4;
         return m;
      }
      #endregion
   }
}