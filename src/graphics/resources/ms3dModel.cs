using System;
using System.IO;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using Util;

namespace Graphics
{
   public class MS3DModelDescriptor : ResourceDescriptor
   {
      public MS3DModelDescriptor(string descriptorFilename)
         : base()
      {
         path = Path.GetDirectoryName(descriptorFilename);
         JsonObject definition = JsonObject.loadFile(descriptorFilename);
         descriptor = definition["ms3d"];
         type = "ms3d";
         name = (string)descriptor["name"];

      }

      public override IResource create(ResourceManager mgr)
      {
         MS3DModelLoader loader = new MS3DModelLoader(mgr);
         SkinnedModel m = loader.loadFromFile(Path.Combine(path, (string)descriptor["file"]));
         if(m == null)
         {
            return null;
         }

         JsonObject initTransform = descriptor["initialTransform"];
         if (initTransform != null)
         {
            Vector3 pos = (Vector3)initTransform["translate"];
            Vector3 rot = (Vector3)initTransform["rotate"];
            Vector3 scale = (Vector3)initTransform["scale"];

            Matrix4 ori = new Matrix4();
            ori = ori.fromHeadingPitchRoll(rot.X, rot.Y, rot.Z);

            m.myInitialTransform = Matrix4.CreateScale(scale) * ori * Matrix4.CreateTranslation(pos);
         }

         JsonObject animations = descriptor["animations"];
         for (int i = 0; i < animations.count(); i++)
         {
            JsonObject ani = animations[i];
            Animation a = new Animation();
            a.name = (string)ani["name"];
            a.fps = loader.fps;
            a.loop = (bool)ani["loop"];
            m.animations.Add(a.name, a);
         }

         return m;
      }
   }

   public class MS3DModelLoader
   {
      ResourceManager myResourceManager;
      #region MS3d Model File Structures

      // These structures store the 'raw' MS3D model data, prior to the 
      // conversion needed to render the model

      struct MilkshapeHeader
      {
         public char[] id;						//neds to be "MS3D000000"
         public int version;              //version 4
      }

      //structure vertex
      struct MilkshapeVertex
      {
         public byte flags;            // SELECTED | SELECTED2 | HIDDEN
         public Vector3 vertex;
         public sbyte boneId;           // -1 = no bone
         public byte referenceCount;
         //public char[] boneIds;
         //public byte[] weights;
         //public uint extra;
      }

      struct MilkshapeTriangle
      {
         public int flags;
         public int[] vertexIndex;
         public Vector3[] vertexNormals;
         //this is actually stored in the file as 
         //float[3] s;
         //float [3] t;
         public Vector2[] uvs;
         public byte smoothingGroup;
         public byte groupIndex;
      }

      struct MilkshapeGroup
      {
         public byte flags;
         //stored as a char[32]
         public string name;
         public int numTriangles;
         public int[] triangleIndexs;
         public sbyte materialIndex;
      }

      struct MilkshapeMaterial
      {
         //stored as a char[32]
         public string name;
         //stored as float[4]
         public Color4 ambient;
         public Color4 diffuse;
         public Color4 specular;
         public Color4 emissive;
         public float shininess;
         public float transparency;
         public byte mode;
         //stored as char[128]
         public string texture;
         public string alphamap;
      }

      struct MilkshapeKeyframe
      {
         public float time;
         public Vector3 param;
      }

      struct MilkshapeJoint
      {
         public byte flags;
         public string name;
         public string parentName;
         public int parentIndex;
         public Vector3 rotation;
         public Vector3 position;
         public int numRotationKeyframes;
         public int numPositionKeyframes;
         public MilkshapeKeyframe[] rotationKeyframes;
         public MilkshapeKeyframe[] translationKeyframes;
         //public Color4 color;

         public Matrix4 local;
         public Matrix4 absolute;
         public Matrix4 final;
      }

      //struct MilkshapeComment
      //{
      //   public int index;
      //   public int commentLength;
      //   public char[] comment;
      //}

      #endregion

      //data structures to store the model
      MilkshapeHeader msHeader;
      int msVertCount;
      MilkshapeVertex[] msVerts;
      int msTriCount;
      MilkshapeTriangle[] msTris;
      int msGroupCount;
      MilkshapeGroup[] msGroups;
      int msMaterialCount;
      MilkshapeMaterial[] msMaterials;
      int msJointCount;
      MilkshapeJoint[] msJoints;

      int msTotalFrames = 0;

      int boneCount { get; set; }
      public float fps { get; set; }

      List<Material> myMaterials = new List<Material>();

      String myFilename;

      public MS3DModelLoader(ResourceManager mgr)
      {
         myResourceManager = mgr;
      }

      public SkinnedModel loadFromFile(string path)
      {
         Info.print("Loading MS3D model {0}", path);

         if (File.Exists(path) == false)
         {
            Warn.print("Cannot find file {0}", path);
            return null;
         }

         FileStream stream = null;
         BinaryReader reader = null;

         try
         {
            myFilename = path;
            // Open the specified file as a stream.
            stream = new FileStream(path, FileMode.Open, FileAccess.Read);

            // Pipe the stream in to a binary reader so we can work at the byte-by-byte level.
            reader = new BinaryReader(stream);

            //load the header
            msHeader = new MilkshapeHeader();
            msHeader.id = reader.ReadChars(10);
            msHeader.version = reader.ReadInt32();

            //check if the header is ok
            //string magic = "MS3D000000".ToString();
            if (msHeader.id.ToString() == "MS3D000000")
            {
               Warn.print("{0} is not a valid Milkshape file", path);
               return null;
            }
            if (msHeader.version != 4)
            {
               Warn.print("{0} is not a valid Milkshape file", path);
               return null;
            }

            #region "read the verts"
            //read the verts
            msVertCount = reader.ReadUInt16();
            msVerts = new MilkshapeVertex[msVertCount];
            for (int i = 0; i < msVertCount; i++)
            {
               msVerts[i].flags = reader.ReadByte();
               msVerts[i].vertex.X = reader.ReadSingle();
               msVerts[i].vertex.Y = reader.ReadSingle();
               msVerts[i].vertex.Z = reader.ReadSingle();
               msVerts[i].boneId = reader.ReadSByte();
               msVerts[i].referenceCount = reader.ReadByte();
            }
            #endregion

            #region "read the tris"
            //read the tris
            msTriCount = reader.ReadUInt16();
            msTris = new MilkshapeTriangle[msTriCount];
            for (int i = 0; i < msTriCount; i++)
            {
               msTris[i].flags = reader.ReadUInt16();
               msTris[i].vertexIndex = new int[3];
               msTris[i].vertexIndex[0] = reader.ReadUInt16();
               msTris[i].vertexIndex[1] = reader.ReadUInt16();
               msTris[i].vertexIndex[2] = reader.ReadUInt16();

               msTris[i].vertexNormals = new Vector3[3];
               msTris[i].vertexNormals[0].X = reader.ReadSingle();
               msTris[i].vertexNormals[0].Y = reader.ReadSingle();
               msTris[i].vertexNormals[0].Z = reader.ReadSingle();

               msTris[i].vertexNormals[1].X = reader.ReadSingle();
               msTris[i].vertexNormals[1].Y = reader.ReadSingle();
               msTris[i].vertexNormals[1].Z = reader.ReadSingle();

               msTris[i].vertexNormals[2].X = reader.ReadSingle();
               msTris[i].vertexNormals[2].Y = reader.ReadSingle();
               msTris[i].vertexNormals[2].Z = reader.ReadSingle();

               //not sure why its stored this way, but it's their format
               msTris[i].uvs = new Vector2[3];
               msTris[i].uvs[0].X = reader.ReadSingle();
               msTris[i].uvs[1].X = reader.ReadSingle();
               msTris[i].uvs[2].X = reader.ReadSingle();
               msTris[i].uvs[0].Y = reader.ReadSingle();
               msTris[i].uvs[1].Y = reader.ReadSingle();
               msTris[i].uvs[2].Y = reader.ReadSingle();
               msTris[i].smoothingGroup = reader.ReadByte();
               msTris[i].groupIndex = reader.ReadByte();
            }
            #endregion

            #region "read the groups"
            //read the meshes or groups
            msGroupCount = reader.ReadUInt16();
            msGroups = new MilkshapeGroup[msGroupCount];
            for (int i = 0; i < msGroupCount; i++)
            {
               msGroups[i].flags = reader.ReadByte();
               byte[] n = reader.ReadBytes(32);
               int idx = Array.FindIndex(n, 0, item => item == '\0');
               msGroups[i].name = System.Text.Encoding.ASCII.GetString(n, 0, idx); ;
               msGroups[i].numTriangles = reader.ReadUInt16();
               msGroups[i].triangleIndexs = new int[msGroups[i].numTriangles];
               for (int j = 0; j < msGroups[i].numTriangles; j++)
               {
                  msGroups[i].triangleIndexs[j] = reader.ReadUInt16();
               }
               msGroups[i].materialIndex = reader.ReadSByte();
            }
            #endregion

            #region "read the materials"
            //read the materials
            msMaterialCount = reader.ReadUInt16();
            msMaterials = new MilkshapeMaterial[msMaterialCount];
            for (int i = 0; i < msMaterialCount; i++)
            {
               byte[] n = reader.ReadBytes(32);
               int idx = Array.FindIndex(n, 0, item => item == '\0');
               String matName = System.Text.Encoding.ASCII.GetString(n, 0, idx);
               System.Console.WriteLine("Found Material: {0}", matName);
               Material m = new Material(myFilename + "/" + matName);
               Color4 amb = new Color4();
               amb.R = reader.ReadSingle();
               amb.G = reader.ReadSingle();
               amb.B = reader.ReadSingle();
               amb.A = reader.ReadSingle();
               m.ambient = amb;

               Color4 dif = new Color4();
               dif.R = reader.ReadSingle();
               dif.G = reader.ReadSingle();
               dif.B = reader.ReadSingle();
               dif.A = reader.ReadSingle();
               m.diffuse = dif;

               Color4 spec = new Color4();
               spec.R = reader.ReadSingle();
               spec.G = reader.ReadSingle();
               spec.B = reader.ReadSingle();
               spec.A = reader.ReadSingle();
               m.spec = spec;

               Color4 emmit = new Color4();
               emmit.R = reader.ReadSingle();
               emmit.G = reader.ReadSingle();
               emmit.B = reader.ReadSingle();
               emmit.A = reader.ReadSingle();
               m.emission = emmit;

               m.shininess = reader.ReadSingle();
               m.alpha = reader.ReadSingle();

               //if the alpha channel is used for something other than alpha
               int mode = reader.ReadByte();

               n = reader.ReadBytes(128);
               idx = Array.FindIndex(n, 0, item => item == '\0');
               String diffuseMapName = System.Text.Encoding.ASCII.GetString(n, 0, idx);
               if (diffuseMapName != "")
               {
                  Texture diffText = findTexture(diffuseMapName);
                  if (diffText != null)
                  {
                     m.addAttribute(new TextureAttribute("diffuseMap", diffText));
                  }
               }

               n = reader.ReadBytes(128);
               idx = Array.FindIndex(n, 0, item => item == '\0');
               String alphaMapName = System.Text.Encoding.ASCII.GetString(n, 0, idx);
               if (alphaMapName != "")
               {
                  Texture alphaText = findTexture(alphaMapName);
                  if (alphaText != null)
                  {
                     m.addAttribute(new TextureAttribute("alphaTexture", alphaText));
                  }
               }

               myMaterials.Add(m);

            }
            #endregion

            #region "read animation data"

            fps = reader.ReadSingle();
            float currentTime = reader.ReadSingle();
            msTotalFrames = reader.ReadInt32();


            #endregion

            #region "read the joints"
            //read the joints
            msJointCount = reader.ReadUInt16();
            boneCount = msJointCount;
            msJoints = new MilkshapeJoint[msJointCount];
            for (int i = 0; i < msJointCount; i++)
            {
               msJoints[i].flags = reader.ReadByte();
               byte[] n = reader.ReadBytes(32);
               int idx = Array.FindIndex(n, 0, item => item == '\0');
               msJoints[i].name = System.Text.Encoding.ASCII.GetString(n, 0, idx); ;
               n = reader.ReadBytes(32);
               idx = Array.FindIndex(n, 0, item => item == '\0');
               msJoints[i].parentName = System.Text.Encoding.ASCII.GetString(n, 0, idx);
               msJoints[i].rotation.X = reader.ReadSingle();
               msJoints[i].rotation.Y = reader.ReadSingle();
               msJoints[i].rotation.Z = reader.ReadSingle();
               msJoints[i].position.X = reader.ReadSingle();
               msJoints[i].position.Y = reader.ReadSingle();
               msJoints[i].position.Z = reader.ReadSingle();

               //read the keyframes
               msJoints[i].numRotationKeyframes = reader.ReadUInt16();
               msJoints[i].numPositionKeyframes = reader.ReadUInt16();
               msJoints[i].rotationKeyframes = new MilkshapeKeyframe[msJoints[i].numRotationKeyframes];
               msJoints[i].translationKeyframes = new MilkshapeKeyframe[msJoints[i].numPositionKeyframes];
               for (int j = 0; j < msJoints[i].numRotationKeyframes; j++)
               {
                  msJoints[i].rotationKeyframes[j].time = reader.ReadSingle();
                  msJoints[i].rotationKeyframes[j].param.X = reader.ReadSingle();
                  msJoints[i].rotationKeyframes[j].param.Y = reader.ReadSingle();
                  msJoints[i].rotationKeyframes[j].param.Z = reader.ReadSingle();
               }
               for (int j = 0; j < msJoints[i].numPositionKeyframes; j++)
               {
                  msJoints[i].translationKeyframes[j].time = reader.ReadSingle();
                  msJoints[i].translationKeyframes[j].param.X = reader.ReadSingle();
                  msJoints[i].translationKeyframes[j].param.Y = reader.ReadSingle();
                  msJoints[i].translationKeyframes[j].param.Z = reader.ReadSingle();
               }
            }

            //fixup parent indexes
            for (int i = 0; i < msJointCount; i++)
            {
               if (msJoints[i].parentName != "")
               {
                  //find the parent name
                  for (int j = 0; j < msJointCount; j++)
                  {
                     if (msJoints[j].name == msJoints[i].parentName)
                     {
                        msJoints[i].parentIndex = j;
                        break;
                     }
                  }
               }
               else
               {
                  msJoints[i].parentIndex = -1;
               }
            }

            #endregion

            #region "setup joints"
            Skeleton skeleton = new Skeleton();
            for (int i = 0; i < msJoints.Length; i++)
            {
               msJoints[i].local = Matrix4.CreateRotationX(msJoints[i].rotation.X) * Matrix4.CreateRotationY(msJoints[i].rotation.Y) * Matrix4.CreateRotationZ(msJoints[i].rotation.Z);
               msJoints[i].local *= Matrix4.CreateTranslation(msJoints[i].position);

               if (msJoints[i].parentIndex == -1)
                  msJoints[i].absolute = msJoints[i].local;
               else
                  msJoints[i].absolute = msJoints[msJoints[i].parentIndex].absolute * msJoints[i].local;


               //initialize the final position of the joint to the default position
               msJoints[i].final = msJoints[i].absolute;

               //create the skeleton
               Bone b = new Bone();
               b.myName = msJoints[i].name;
               b.myParent = msJoints[i].parentIndex;
               b.myWorldPose = msJoints[i].final;
               skeleton.myBones.Add(b);
            }

            #endregion

            #region "modifiy verticies"
            for (int i = 0; i < msVertCount; i++)
            {
               if (msVerts[i].boneId != -1)
               {
                  Matrix4 m = msJoints[msVerts[i].boneId].absolute;
                  Matrix4 inv = Matrix4.Invert(m);
                  msVerts[i].vertex = Vector3.TransformPosition(msVerts[i].vertex, inv);
               }
            }
            #endregion

            //push all the stuff onto the graphics card
            SkinnedModel sm = bufferData();
            sm.skeleton = skeleton;            
            sm.size = (findMax() - findMin()).Length / 2.0f;

            return sm;
         }
         catch (Exception ex)
         {
            throw new Exception("Error while loading Milkshape model from definition file ( " + path + " ).", ex);
         }
         finally
         {
            if (reader != null) { reader.Close(); }
            if (stream != null)
            {
               stream.Close();
               stream.Dispose();
            }
         }
      }

      Texture findTexture(String textureName)
      {
         //get the texture for this group
         String texFilename = Path.GetFileName(textureName);
         String modelPath = Path.GetDirectoryName(myFilename);
         textureName = Path.Combine(modelPath, texFilename);
         TextureDescriptor td = new TextureDescriptor(textureName);
         Texture t = myResourceManager.getResource(td) as Texture;

         return t;
      }

      SkinnedModel bufferData()
      {
         SkinnedModel sm = new SkinnedModel();

         //just the first group at this time
         int vertCount = 0;
         for (int g = 0; g < msGroups.Length; g++)
         {
            vertCount += msGroups[g].numTriangles * 3;
         }

         V3N3T2B4W4[] verts = new V3N3T2B4W4[vertCount];
         ushort[] indexes = new ushort[vertCount];
         Vector4[] boneData = new Vector4[msJointCount * msTotalFrames * 4];  //store an absolute matrix for each bone for each frame

         int next = 0;
         for (int g = 0; g < msGroups.Length; g++)
         {
            Mesh m = new Mesh();
            m.indexBase = next;
            for (int t = 0; t < msGroups[g].numTriangles; t++)
            {
               MilkshapeTriangle tri = msTris[msGroups[g].triangleIndexs[t]];
               for (int v = 0; v < 3; v++)
               {
                  verts[next].Position = msVerts[tri.vertexIndex[v]].vertex;
                  verts[next].Normal = tri.vertexNormals[v];
                  verts[next].TexCoord = tri.uvs[v];

                  //this format only uses 1 bone per vertex, but my vertex format allows for up to 4
                  verts[next].BoneId.X = msVerts[tri.vertexIndex[v]].boneId;
                  verts[next].BoneId.Y = -1;
                  verts[next].BoneId.Z = -1;
                  verts[next].BoneId.W = -1;
                  verts[next].BoneWeight.X = 1.0f; //only 1 bone influences the mesh
                  verts[next].BoneWeight.Y = 0.0f;
                  verts[next].BoneWeight.Z = 0.0f;
                  verts[next].BoneWeight.W = 0.0f;

                  indexes[next] = (ushort)next;
                  next++;
               }
            }
            m.indexCount = msGroups[g].numTriangles * 3;
            m.material = myMaterials[msGroups[g].materialIndex];
            sm.myMeshes.Add(m);
         }

         //prep the bone data 
         next = 0;
         for (int f = 0; f < msTotalFrames; f++)
         {
            //get the joints as an absolute matrix
            Matrix4[] absMat = getJoints(f);

            for (int j = 0; j < msJointCount; j++)
            {
               boneData[next++] = absMat[j].Row0;
               boneData[next++] = absMat[j].Row1;
               boneData[next++] = absMat[j].Row2;
               boneData[next++] = absMat[j].Row3;
            }
         }
        
         sm.myBindings = V3N3T2B4W4.bindings();
         VertexBufferObject vbo = new VertexBufferObject(BufferUsageHint.StaticDraw);
         vbo.setData(verts);
         sm.myVbos.Add(vbo);
         sm.myIbo.setData(indexes);
         sm.myFrames.setData(boneData);

         return sm;
      }

      public Matrix4[] getJoints(int frame)
      {
         Matrix4[] final = new Matrix4[msJointCount];
         float absoluteTime = frame / fps;

         //update the joints
         for (int i = 0; i < msJointCount; i++)
         {
            //if this joint doesn't have any keyframes, then set it to the absolute, and continue
            if (msJoints[i].numPositionKeyframes == 0 && msJoints[i].numRotationKeyframes == 0)
            {
               final[i] = msJoints[i].absolute;
               continue;
            }

            //find the right keyframe for the joint
            int keyFrame = 0;
            while (keyFrame < msJoints[i].numPositionKeyframes && msJoints[i].translationKeyframes[keyFrame].time < absoluteTime)
            {
               keyFrame++;
            }

            //get the two values
            Vector3 posKey;
            Vector3 oriKey;
            if (keyFrame == 0)
            {
               posKey = msJoints[i].translationKeyframes[keyFrame].param;
               oriKey = msJoints[i].rotationKeyframes[keyFrame].param;
            }
            else if (keyFrame == msJoints[i].numPositionKeyframes)
            {
               posKey = msJoints[i].translationKeyframes[keyFrame - 1].param;
               oriKey = msJoints[i].rotationKeyframes[keyFrame - 1].param;
            }
            else
            {
               posKey = msJoints[i].translationKeyframes[keyFrame].param;
               oriKey = msJoints[i].rotationKeyframes[keyFrame].param;
               Vector3 prevPos = msJoints[i].translationKeyframes[keyFrame - 1].param;
               Vector3 prevOri = msJoints[i].rotationKeyframes[keyFrame - 1].param;

               float timeDelta = msJoints[i].translationKeyframes[keyFrame].time - msJoints[i].translationKeyframes[keyFrame - 1].time;
               float interpol = (float)((absoluteTime - msJoints[i].translationKeyframes[keyFrame - 1].time) / timeDelta);

               //interpolate the translation and rotation
               posKey = Vector3.Lerp(prevPos, posKey, interpol);
               oriKey = Vector3.Lerp(prevOri, oriKey, interpol);
            }

            //create the new translation matrix
            Matrix4 trans = Matrix4.CreateRotationX(oriKey.X) * Matrix4.CreateRotationY(oriKey.Y) * Matrix4.CreateRotationZ(oriKey.Z);
            trans *= msJoints[i].local * Matrix4.CreateTranslation(posKey);

            if (msJoints[i].parentIndex == -1)
               final[i] = trans;
            else
               final[i] = trans * final[msJoints[i].parentIndex];
         }

         return final;
      }

      public Vector3 findMin()
      {
         Vector3 min = new Vector3(float.MaxValue);
         foreach (MilkshapeVertex mv in msVerts)
         {
            Vector3 v = mv.vertex;
            if (v.X < min.X) min.X = v.X;
            if (v.Y < min.Y) min.Y = v.Y;
            if (v.Z < min.Z) min.Z = v.Z;
         }

         return min;
      }

      public Vector3 findMax()
      {
         Vector3 max = new Vector3(float.MinValue);
         foreach (MilkshapeVertex mv in msVerts)
         {
            Vector3 v = mv.vertex;
            if (v.X > max.X) max.X = v.X;
            if (v.Y > max.Y) max.Y = v.Y;
            if (v.Z > max.Z) max.Z = v.Z;
         }

         return max;
      }
   }
}