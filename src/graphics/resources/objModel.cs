using System;
using System.IO;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using Util;

namespace Graphics
{
   public class ObjModelDescriptor : ResourceDescriptor
   {
      public ObjModelDescriptor(string name)
         : base(name)
      {
         type = "ObjModel";
      }

      public override IResource create(ResourceManager mgr)
      {
         ObjModelLoader loader = new ObjModelLoader(mgr);
         StaticModel m = loader.loadFromFile(name);
         return m;
      }
   }

   public class ObjModelLoader
   {
      ResourceManager myResourceManager;

      List<Vector3> myVerts = new List<Vector3>();
      List<Vector2> myUvs = new List<Vector2>();
      List<Vector3> myNorms = new List<Vector3>();

      class Tri
      {
         public int[] i = new int[3];
         public int[] uv = new int[3];
         public int[] n = new int[3];
      }

      MultiMap<Material, Tri> myTris = new MultiMap<Material, Tri>();

      Dictionary<String, Material> myMaterials = new Dictionary<String, Material>();
      Material myCurrentMaterial;
      String myFilename;

      public ObjModelLoader(ResourceManager mgr)
      {
         myResourceManager = mgr;
      }

      public StaticModel loadFromFile(string path)
      {
         Info.print("Loading OBJ model {0}", path);

         if (File.Exists(path) == false)
         {
            Warn.print("Cannot find file {0}", path);
            return null;
         }

         myFilename = path;

         StreamReader file = new StreamReader(path);
         String line;
         while ((line = file.ReadLine()) != null)
         {
            parseLine(line);
         }

         file.Close();

         StaticModel sm = buildMeshes();
         sm.size = (findMax() - findMin()).Length / 2.0f;

         return sm;
      }

      public void parseLine(String line)
      {
         line = line.Trim();
         line = line.ToLowerInvariant();
         String[] tokens = line.Split(' ');
         switch (tokens[0])
         {
            case "#":
               break;
            case "mtllib":
               {
                  string filename = Path.GetDirectoryName(myFilename);
                  filename = Path.Combine(filename, tokens[1]);
                  loadMaterialFromFile(filename);
               }
               break;
            case "v":
               {
                  Vector3 v = new Vector3();
                  v.X = System.Convert.ToSingle(tokens[1]);
                  v.Y = System.Convert.ToSingle(tokens[2]);
                  v.Z = System.Convert.ToSingle(tokens[3]);
                  myVerts.Add(v);
               }
               break;
            case "vt":
               {
                  Vector2 v = new Vector2();
                  v.X = System.Convert.ToSingle(tokens[1]);
                  v.Y = System.Convert.ToSingle(tokens[2]);
                  myUvs.Add(v);
               }
               break;
            case "vn":
               {
                  Vector3 v = new Vector3();
                  v.X = System.Convert.ToSingle(tokens[1]);
                  v.Y = System.Convert.ToSingle(tokens[2]);
                  v.Z = System.Convert.ToSingle(tokens[3]);
                  v.Normalize();
                  myNorms.Add(v);
               }
               break;
            case "usemtl":
               {
                  String matName = tokens[1];
                  if (myMaterials.TryGetValue(matName, out myCurrentMaterial) == false)
                  {
                     throw new Exception(String.Format("Cannot find material named: {0}", matName));
                  }
               }
               break;
            case "f":
               {
                  if (tokens.Length < 4 || tokens.Length > 5)
                  {
                     Warn.print("Only supporting tris and quad in models at this time");
                     break;
                  }

                  Tri t = new Tri();
                  for (int j = 0; j < 3; j++)
                  {
                     String[] ss = tokens[j + 1].Split('/');
                     t.i[j] = System.Convert.ToInt32(ss[0]) - 1;
                     if (ss.Length >= 2 && ss[1] != "") t.uv[j] = System.Convert.ToInt32(ss[1]) - 1; else t.uv[j] = -1;
                     if (ss.Length >= 3 && ss[2] != "") t.n[j] = System.Convert.ToInt32(ss[2]) - 1; else t.n[j] = -1;
                  }
                  myTris.Add(myCurrentMaterial, t);

                  //create a new triangle for the other half
                  if (tokens.Length == 5)
                  {
                     Tri t2 = new Tri();
                     int j = 0;
                     String[] ss = tokens[1].Split('/');
                     t2.i[j] = System.Convert.ToInt32(ss[0]) - 1;
                     if (ss.Length >= 2 && ss[1] != "") t2.uv[j] = System.Convert.ToInt32(ss[1]) - 1; else t2.uv[j] = -1;
                     if (ss.Length >= 3 && ss[2] != "") t2.n[j] = System.Convert.ToInt32(ss[2]) - 1; else t2.n[j] = -1;
                     j = 1;
                     ss = tokens[4].Split('/');
                     t2.i[j] = System.Convert.ToInt32(ss[0]) - 1;
                     if (ss.Length >= 2 && ss[1] != "") t2.uv[j] = System.Convert.ToInt32(ss[1]) - 1; else t2.uv[j] = -1;
                     if (ss.Length >= 3 && ss[2] != "") t2.n[j] = System.Convert.ToInt32(ss[2]) - 1; else t2.n[j] = -1;
                     j = 2;
                     ss = tokens[3].Split('/');
                     t2.i[j] = System.Convert.ToInt32(ss[0]) - 1;
                     if (ss.Length >= 2 && ss[1] != "") t2.uv[j] = System.Convert.ToInt32(ss[1]) - 1; else t2.uv[j] = -1;
                     if (ss.Length >= 3 && ss[2] != "") t2.n[j] = System.Convert.ToInt32(ss[2]) - 1; else t2.n[j] = -1;
                     myTris.Add(myCurrentMaterial, t2);
                  }
               }
               break;
         }
      }

      public bool loadMaterialFromFile(string path)
      {
         string rootPath = Path.GetDirectoryName(path);
         string filename = Path.GetFileNameWithoutExtension(path);
         filename += ".mtl";
         filename = Path.Combine(rootPath, filename);
         if (File.Exists(filename) == false)
         {
            Warn.print("Cannot find file {0}", filename);
            return false;
         }

         StreamReader file = new StreamReader(filename);
         String line;
         while ((line = file.ReadLine()) != null)
         {
            line = line.Trim();
            line = line.ToLowerInvariant();
            String[] tokens = line.Split(' ');
            switch (tokens[0])
            {
               case "#":
                  {
                     continue;
                  }

               case "newmtl":
                  {
                     Material m = new Material(tokens[1]);
							m.myFeatures |= Material.Feature.Lighting;
                     while (line != "")
                     {
                        line = file.ReadLine();
                        line = line.Trim();
                        line = line.ToLowerInvariant();
                        String[] matTokens = line.Split(' ');
                        switch (matTokens[0])
                        {
                           case "ka":
                              {
                                 Color4 c = new Color4();
                                 c.R = System.Convert.ToSingle(matTokens[1]);
                                 c.G = System.Convert.ToSingle(matTokens[2]);
                                 c.B = System.Convert.ToSingle(matTokens[3]);
                                 m.addAttribute(new ColorAttribute("ambientColor", c));
                                 break;
                              }
                           case "kd":
                              {
                                 Color4 c = new Color4();
                                 c.R = System.Convert.ToSingle(matTokens[1]);
                                 c.G = System.Convert.ToSingle(matTokens[2]);
                                 c.B = System.Convert.ToSingle(matTokens[3]);
                                 m.addAttribute(new ColorAttribute("diffuseColor", c));
                                 break;
                              }
                           case "ks":
                              {
                                 Color4 c = new Color4();
                                 c.R = System.Convert.ToSingle(matTokens[1]);
                                 c.G = System.Convert.ToSingle(matTokens[2]);
                                 c.B = System.Convert.ToSingle(matTokens[3]);
                                 m.addAttribute(new ColorAttribute("specularColor", c));
                                 break;
                              }
                           case "ns":
                              {
                                 float val = System.Convert.ToSingle(matTokens[1]);
                                 m.addAttribute(new ValueAttribute("shininess", val));
                                 break;
                              }
                           case "d":
                              {
                                 float val = System.Convert.ToSingle(matTokens[1]);
                                 m.addAttribute(new ValueAttribute("alpha", val));
                                 break;
                              }

                           case "map_ka":
                              {
                                 string textureName = matTokens[1];
                                 TextureDescriptor td = new TextureDescriptor(Path.Combine(rootPath, textureName), true);
                                 Texture t = myResourceManager.getResource(td) as Texture;
                                 if (t != null)
                                    m.addAttribute(new TextureAttribute("ambientMap", t));
                                 break;
                              }

                           case "map_kd":
                              {
                                 string textureName = matTokens[1];
                                 TextureDescriptor td = new TextureDescriptor(Path.Combine(rootPath, textureName), true);
                                 Texture t = myResourceManager.getResource(td) as Texture;
											if (t != null)
											{
												m.myTextures[(int)Material.TextureId.Diffuse] = new TextureAttribute("diffuseMap", t);
												m.myFeatures |= Material.Feature.DiffuseMap;
											}
                                 break;
                              }

                           case "map_ks":
                              {
                                 string textureName = matTokens[1];
                                 TextureDescriptor td = new TextureDescriptor(Path.Combine(rootPath, textureName), true);
                                 Texture t = myResourceManager.getResource(td) as Texture;
                                 if (t != null)
											{
												m.myTextures[(int)Material.TextureId.Specular] = new TextureAttribute("specularMap", t);
												m.myFeatures |= Material.Feature.SpecMap;
											}
											break;
                              }
                           case "map_d":
                              {
                                 string textureName = matTokens[1];
                                 TextureDescriptor td = new TextureDescriptor(Path.Combine(rootPath, textureName), true);
                                 Texture t = myResourceManager.getResource(td) as Texture;
											if (t != null)
												m.addAttribute(new TextureAttribute("alphaMap", t));
                                 break;
                              }
                           case "bump":
                           case "map_bump":
                              {
                                 string textureName = matTokens[1];
                                 TextureDescriptor td = new TextureDescriptor(Path.Combine(rootPath, textureName), true);
                                 Texture t = myResourceManager.getResource(td) as Texture;
											if (t != null)
											{
												m.myTextures[(int)Material.TextureId.Normal] = new TextureAttribute("normalMap", t);
												m.myFeatures |= Material.Feature.NormalMap;
											}
                                 break;
                              }
                           case "disp":
                              {
                                 string textureName = matTokens[1];
                                 TextureDescriptor td = new TextureDescriptor(Path.Combine(rootPath, textureName), true);
                                 Texture t = myResourceManager.getResource(td) as Texture;
											if (t != null)
											{
												m.myTextures[(int)Material.TextureId.Displacement] = new TextureAttribute("displaceMap", t);
												m.myFeatures |= Material.Feature.DisplacementMap;
											}
                                 break;
                              }
                        }
                     }

                     myMaterials.Add(tokens[1], m);
                  }
                  break;
            }
         }

         file.Close();
         return true;
      }

      public StaticModel buildMeshes()
      {
         List<V3N3T2> verts = new List<V3N3T2>();
         List<ushort> index = new List<ushort>();

         StaticModel sm = new StaticModel();

         int indexCount = 0;
         int indexOffset = indexCount;

         foreach (Material mat in myTris.Keys)
         {
            List<Tri> tris;
            myTris.TryGetValue(mat, out tris);

            //build the vertex list
            foreach (Tri t in tris)
            {
               for (int j = 0; j < 3; j++)
               {
                  V3N3T2 v = new V3N3T2();
                  v.Position = myVerts[t.i[j]];
                  if (t.uv[j] != -1) v.TexCoord = myUvs[t.uv[j]];
                  if (t.n[j] != -1) v.Normal = myNorms[t.n[j]];

                  //check if this exact vertex is already in the list
                  if (!verts.Contains(v))
                  {
                     verts.Add(v);
                  }

                  index.Add((ushort)verts.IndexOf(v));
                  indexCount++;
               }
            }

            Mesh m = new Mesh();
				m.primativeType = PrimitiveType.Triangles;
            m.indexBase = indexOffset;
            m.indexCount = indexCount;
            m.material = mat;
            sm.myMeshes.Add(m);

            //update index base for next pass
            indexOffset += indexCount;
            indexCount = 0;
         }

         sm.myVbo.setData(verts);
         sm.myIbo.setData(index);

         return sm;
      }

      public Vector3 findMin()
      {
         Vector3 min = new Vector3(float.MaxValue);
         foreach(Vector3 v in myVerts)
         {
            if (v.X < min.X) min.X = v.X;
            if (v.Y < min.Y) min.Y = v.Y;
            if (v.Z < min.Z) min.Z = v.Z;
         }

         return min;
      }

      public Vector3 findMax()
      {
         Vector3 max = new Vector3(float.MinValue);
         foreach (Vector3 v in myVerts)
         {
            if (v.X > max.X) max.X = v.X;
            if (v.Y > max.Y) max.Y = v.Y;
            if (v.Z > max.Z) max.Z = v.Z;
         }

         return max;
      }
   }
}