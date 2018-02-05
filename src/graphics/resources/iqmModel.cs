using System;
using System.IO;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using Util;

namespace Graphics
{
   public class IQModelDescriptor : ResourceDescriptor
   {
      public IQModelDescriptor(string descriptorFilename)
         : base()
      {
         path = Path.GetDirectoryName(descriptorFilename);
         JsonObject definition = JsonObject.loadFile(descriptorFilename);
         descriptor = definition["iqm"];
         type = "iqm";
         name = (string)descriptor["name"];
      }

      public override IResource create(ResourceManager mgr)
      {
         IQModelLoader loader = new IQModelLoader(mgr);
         SkinnedModel m = loader.loadFromFile(Path.Combine(path, (string)descriptor["file"]));
         if(m== null)
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

         return m;
      }
   }

   public class IQModelLoader
   {
      ResourceManager myResourceManager;
      #region "IQM structs"
      public struct IQMHeader
      {
         public char[] magic; // the string "INTERQUAKEMODEL\0", 0 terminated
         public uint version; // must be version 2
         public uint filesize;
         public uint flags;
         public uint num_text, ofs_text;
         public uint num_meshes, ofs_meshes;
         public uint num_vertexarrays, num_vertexes, ofs_vertexarrays;
         public uint num_triangles, ofs_triangles, ofs_adjacency;
         public uint num_joints, ofs_joints;
         public uint num_poses, ofs_poses;
         public uint num_anims, ofs_anims;
         public uint num_frames, num_framechannels, ofs_frames, ofs_bounds;
         public uint num_comment, ofs_comment;
         public uint num_extensions, ofs_extensions; // these are stored as a linked list, not as a contiguous array
      };

      public struct iqmmesh
      {
         public string name;     // unique name for the mesh, if desired
         public string material; // set to a name of a non-unique material or texture
         public uint first_vertex, num_vertexes;
         public uint first_triangle, num_triangles;
      };

      public enum VertexArrayType // vertex array type
      {
         IQM_POSITION = 0,  // float, 3
         IQM_TEXCOORD = 1,  // float, 2
         IQM_NORMAL = 2,  // float, 3
         IQM_TANGENT = 3,  // float, 4
         IQM_BLENDINDEXES = 4,  // ubyte, 4
         IQM_BLENDWEIGHTS = 5,  // ubyte, 4
         IQM_COLOR = 6,  // ubyte, 4

         // all values up to IQM_CUSTOM are reserved for future use
         // any value >= IQM_CUSTOM is interpreted as CUSTOM type
         // the value then defines an offset into the string table, where offset = value - IQM_CUSTOM
         // this must be a valid string naming the type
         IQM_CUSTOM = 0x10
      };

      public enum VertexArrayFormat // vertex array format
      {
         IQM_BYTE = 0,
         IQM_UBYTE = 1,
         IQM_SHORT = 2,
         IQM_USHORT = 3,
         IQM_INT = 4,
         IQM_UINT = 5,
         IQM_HALF = 6,
         IQM_FLOAT = 7,
         IQM_DOUBLE = 8,
      };

      public struct iqmvertexarray
      {
         public VertexArrayType type;   // type or custom name
         public uint flags;
         public VertexArrayFormat format; // component format
         public uint size;   // number of components
         public uint offset; // offset to array of tightly packed components, with num_vertexes * size total entries 
         // offset must be aligned to max(sizeof(format), 4)
      };

      public struct iqmtriangle
      {
         public uint[] vertex;
      };

      public struct iqmadjacency
      {
         // each value is the index of the adjacent triangle for edge 0, 1, and 2, where ~0 (= -1) indicates no adjacent triangle
         // indexes are relative to the iqmheader.ofs_triangles array and span all meshes, where 0 is the first triangle, 1 is the second, 2 is the third, etc. 
         public uint[] triangle;
      };

      public struct iqmjoint
      {
         public string name;
         public int parent; // parent < 0 means this is a root bone
         public Vector3 translate;
         public Quaternion rotate;
         public Vector3 scale;
         // translate is translation <Tx, Ty, Tz>, and rotate is quaternion rotation <Qx, Qy, Qz, Qw>
         // rotation is in relative/parent local space
         // scale is pre-scaling <Sx, Sy, Sz>
         // output = (input*scale)*rotation + translation
      };

      public struct iqmpose
      {
         public int parent; // parent < 0 means this is a root bone
         public uint channelmask; // mask of which 10 channels are present for this joint pose
         public float[] channeloffset/*[10]*/, channelscale/*[10]*/;
         // channels 0..2 are translation <Tx, Ty, Tz> and channels 3..6 are quaternion rotation <Qx, Qy, Qz, Qw>
         // rotation is in relative/parent local space
         // channels 7..9 are scale <Sx, Sy, Sz>
         // output = (input*scale)*rotation + translation
      };

      public ushort[] frames; // frames is a big unsigned short array where each group of framechannels components is one frame

      public struct iqmanim
      {
         public string name;
         public uint first_frame, num_frames;
         public float framerate;
         public uint flags;
      };

      public enum AnimationFlags// iqmanim flags
      {
         IQM_LOOP = 1 << 0
      };

      public struct iqmbounds
      {
         public float[] bbmins/*[3]*/, bbmaxs/*[3]*/; // the minimum and maximum coordinates of the bounding box for this animation frame
         public float xyradius, radius; // the circular radius in the X-Y plane, as well as the spherical radius
      };

      public char[] text; // big array of all strings, each individual string being 0 terminated, with the first string always being the empty string "" (i.e. text[0] == 0)
      public char[] comment;

      public struct iqmextension
      {
         public uint name;
         public uint num_data, ofs_data;
         public uint ofs_extensions; // pointer to next extension
      };

      // vertex data is not really interleaved, but this just gives examples of standard types of the data arrays
      public struct iqmvertex
      {
         public float[] position/*[3]*/, texcoord/*[2]*/, normal/*[3]*/, tangent/*[4]*/;
         public byte blendindices/*[4]*/, blendweights/*[4]*/, color/*[4]*/;
      };


      #endregion

      public TextureBufferObject myFrames;
      public int boneCount = 0;

      public IQModelLoader(ResourceManager mgr)
      {
         myResourceManager = mgr;
      }

      public SkinnedModel loadFromFile(string filename)
      {
         Info.print("Loading IQM model {0}", filename);

         if (File.Exists(filename) == false)
         {
            Warn.print("Cannot find file {0}", filename);
            return null;
         }

         V3N3T2B4W4[] vertexData;
         ushort[] triangleIndexes;

         IQMHeader myHeader;
         byte[] myTexts;
         List<String> myComments = new List<String>();
         iqmvertexarray[] myVertArrays;
         iqmjoint[] myJoints;
         iqmpose[] myPoses;
         iqmanim[] myAnimataions;
         iqmbounds[] myBounds;
         iqmmesh[] meshData;
         ushort[] myFrameData;

         SkinnedModel sm = new SkinnedModel();

         System.IO.FileStream stream = null;
         System.IO.BinaryReader reader = null;
         try
         {
            // Open the specified file as a stream and create a reader
            stream = new System.IO.FileStream(filename, FileMode.Open, FileAccess.Read);
            reader = new System.IO.BinaryReader(stream);

            myHeader.magic = reader.ReadChars(16);
            myHeader.version = reader.ReadUInt32();
            if (myHeader.version != 2)
               return null;
            myHeader.filesize = reader.ReadUInt32();
            myHeader.flags = reader.ReadUInt32();
            myHeader.num_text = reader.ReadUInt32();
            myHeader.ofs_text = reader.ReadUInt32();
            myHeader.num_meshes = reader.ReadUInt32();
            myHeader.ofs_meshes = reader.ReadUInt32();
            myHeader.num_vertexarrays = reader.ReadUInt32();
            myHeader.num_vertexes = reader.ReadUInt32();
            myHeader.ofs_vertexarrays = reader.ReadUInt32();
            myHeader.num_triangles = reader.ReadUInt32();
            myHeader.ofs_triangles = reader.ReadUInt32();
            myHeader.ofs_adjacency = reader.ReadUInt32();
            myHeader.num_joints = reader.ReadUInt32();
            myHeader.ofs_joints = reader.ReadUInt32();
            myHeader.num_poses = reader.ReadUInt32();
            myHeader.ofs_poses = reader.ReadUInt32();
            myHeader.num_anims = reader.ReadUInt32();
            myHeader.ofs_anims = reader.ReadUInt32();
            myHeader.num_frames = reader.ReadUInt32();
            myHeader.num_framechannels = reader.ReadUInt32();
            myHeader.ofs_frames = reader.ReadUInt32();
            myHeader.ofs_bounds = reader.ReadUInt32();
            myHeader.num_comment = reader.ReadUInt32();
            myHeader.ofs_comment = reader.ReadUInt32();
            myHeader.num_extensions = reader.ReadUInt32();
            myHeader.ofs_extensions = reader.ReadUInt32();

            boneCount = (int)myHeader.num_joints;

            //read text
            myTexts = new byte[myHeader.num_text];
            stream.Seek(myHeader.ofs_text, SeekOrigin.Begin);
            for (int i = 0; i < myHeader.num_text; i++)
            {
               myTexts[i] = reader.ReadByte();
            }

            #region read geometry

            //create geometry fields
            for (int m = 0; m < myHeader.num_meshes; m++)
            {
               sm.myMeshes.Add(new Mesh());
            }

            //read the mesh data
            meshData = new iqmmesh[myHeader.num_meshes];
            stream.Seek(myHeader.ofs_meshes, SeekOrigin.Begin);
            for (int i = 0; i < myHeader.num_meshes; i++)
            {
               iqmmesh temp = new iqmmesh();
               UInt32 n = reader.ReadUInt32();
               temp.name = readNullTerminated(myTexts, n);
               n = reader.ReadUInt32();
               temp.material = readNullTerminated(myTexts, n);
               String fn = System.IO.Path.GetFileName(temp.material);
               fn = System.IO.Path.ChangeExtension(fn, ".png");

               String dir = System.IO.Path.GetDirectoryName(filename);
               fn = System.IO.Path.Combine(dir, fn);

               TextureDescriptor td = new TextureDescriptor(fn);
               Texture tex = myResourceManager.getResource(td) as Texture;
               Material m = new Material(temp.material);
               m.addAttribute(new TextureAttribute("diffuseMap", tex));
					m.myFeatures |= Material.Feature.Lighting;
					m.myFeatures |= Material.Feature.DiffuseMap;

               sm.myMeshes[i].material = m;

               temp.first_vertex = reader.ReadUInt32();
               temp.num_vertexes = reader.ReadUInt32();
               temp.first_triangle = reader.ReadUInt32();
               temp.num_triangles = reader.ReadUInt32();
               meshData[i] = temp;
            }

            //read vertex arrays
            myVertArrays = new iqmvertexarray[myHeader.num_vertexarrays];
            stream.Seek(myHeader.ofs_vertexarrays, SeekOrigin.Begin);
            for (int i = 0; i < myHeader.num_vertexarrays; i++)
            {
               iqmvertexarray temp = new iqmvertexarray();
               temp.type = (VertexArrayType)reader.ReadUInt32();
               temp.flags = reader.ReadUInt32();
               temp.format = (VertexArrayFormat)reader.ReadUInt32();
               temp.size = reader.ReadUInt32();
               temp.offset = reader.ReadUInt32();
               myVertArrays[i] = temp;
            }

            //read the vertex data
            vertexData = new V3N3T2B4W4[myHeader.num_vertexes];
            for (int i = 0; i < myHeader.num_vertexarrays; i++)
            {
               iqmvertexarray va = myVertArrays[i];
               switch (va.type)
               {
                  case VertexArrayType.IQM_POSITION:
                     {
                        stream.Seek(va.offset, SeekOrigin.Begin);
                        for (int j = 0; j < myHeader.num_vertexes; j++)
                        {
                           Vector3 temp = new Vector3();
                           temp.X = reader.ReadSingle();
                           temp.Y = reader.ReadSingle();
                           temp.Z = reader.ReadSingle();
                           vertexData[j].Position = temp;
                        }
                        break;
                     }
                  case VertexArrayType.IQM_TEXCOORD:
                     {
                        stream.Seek(va.offset, SeekOrigin.Begin);
                        for (int j = 0; j < myHeader.num_vertexes; j++)
                        {
                           Vector2 temp = new Vector2();
                           temp.X = reader.ReadSingle();
                           temp.Y = reader.ReadSingle();
                           vertexData[j].TexCoord = temp;
                        }
                        break;
                     }
                  case VertexArrayType.IQM_NORMAL:
                     {
                        stream.Seek(va.offset, SeekOrigin.Begin);
                        for (int j = 0; j < myHeader.num_vertexes; j++)
                        {
                           Vector3 temp = new Vector3();
                           temp.X = reader.ReadSingle();
                           temp.Y = reader.ReadSingle();
                           temp.Z = reader.ReadSingle();
                           vertexData[j].Normal = temp;
                        }
                        break;
                     }
                  case VertexArrayType.IQM_BLENDINDEXES:
                     {
                        stream.Seek(va.offset, SeekOrigin.Begin);
                        for (int j = 0; j < myHeader.num_vertexes; j++)
                        {
                           Vector4 temp = new Vector4();
                           temp.X = (float)reader.ReadByte();
                           temp.Y = (float)reader.ReadByte();
                           temp.Z = (float)reader.ReadByte();
                           temp.W = (float)reader.ReadByte();
                           vertexData[j].BoneId = temp;
                        }
                        break;
                     }
                  case VertexArrayType.IQM_BLENDWEIGHTS:
                     {
                        stream.Seek(va.offset, SeekOrigin.Begin);
                        for (int j = 0; j < myHeader.num_vertexes; j++)
                        {
                           Vector4 temp = new Vector4();
                           temp.X = ((float)reader.ReadByte()) / 255.0f;
                           temp.Y = ((float)reader.ReadByte()) / 255.0f;
                           temp.Z = ((float)reader.ReadByte()) / 255.0f;
                           temp.W = ((float)reader.ReadByte()) / 255.0f;
                           vertexData[j].BoneWeight = temp;
                        }
                        break;
                     }
               }
            }

            //read triangles indexes
            triangleIndexes = new ushort[myHeader.num_triangles * 3];
            stream.Seek(myHeader.ofs_triangles, SeekOrigin.Begin);
            for (int i = 0; i < myHeader.num_triangles * 3; i++)
            {
               triangleIndexes[i] = (ushort)reader.ReadUInt32();
            }
            #endregion

            #region read animation data
            //read joints
            myJoints = new iqmjoint[myHeader.num_joints];
            stream.Seek(myHeader.ofs_joints, SeekOrigin.Begin);
            for (int i = 0; i < myHeader.num_joints; i++)
            {
               iqmjoint temp = new iqmjoint();
               UInt32 n = reader.ReadUInt32();
               temp.name = readNullTerminated(myTexts, n);
               temp.parent = reader.ReadInt32();
               temp.translate = new Vector3();
               temp.translate.X = reader.ReadSingle();
               temp.translate.Y = reader.ReadSingle();
               temp.translate.Z = reader.ReadSingle();
               temp.rotate = new Quaternion();
               temp.rotate.X = reader.ReadSingle();
               temp.rotate.Y = reader.ReadSingle();
               temp.rotate.Z = reader.ReadSingle();
               temp.rotate.W = reader.ReadSingle();
               temp.rotate.Normalize();
               temp.scale = new Vector3();
               temp.scale.X = reader.ReadSingle();
               temp.scale.Y = reader.ReadSingle();
               temp.scale.Z = reader.ReadSingle();
               myJoints[i] = temp;
            }
				sm.boneCount = (int)myHeader.num_joints;

            //read poses
            myPoses = new iqmpose[myHeader.num_poses];
            stream.Seek(myHeader.ofs_poses, SeekOrigin.Begin);
            for (int i = 0; i < myHeader.num_poses; i++)
            {
               iqmpose temp = new iqmpose();
               temp.parent = reader.ReadInt32();
               temp.channelmask = reader.ReadUInt32();

               temp.channeloffset = new float[10];
               for (int j = 0; j < 10; j++)
                  temp.channeloffset[j] = reader.ReadSingle();

               temp.channelscale = new float[10];
               for (int j = 0; j < 10; j++)
                  temp.channelscale[j] = reader.ReadSingle();

               myPoses[i] = temp;
            }

            //read animations
            myAnimataions = new iqmanim[myHeader.num_anims];
            stream.Seek(myHeader.ofs_anims, SeekOrigin.Begin);
            for (int i = 0; i < myHeader.num_anims; i++)
            {
               iqmanim temp = new iqmanim();
               UInt32 n = reader.ReadUInt32();
               temp.name = readNullTerminated(myTexts, n);
               temp.first_frame = reader.ReadUInt32();
               temp.num_frames = reader.ReadUInt32();
               temp.framerate = reader.ReadSingle();
               temp.flags = reader.ReadUInt32();
               myAnimataions[i] = temp;

               sm.animations.Add(temp.name, new Animation(temp.name, (int)temp.first_frame, (int)temp.first_frame + (int)temp.num_frames - 1, temp.framerate, true/*((int)temp.flags & (int)AnimationFlags.IQM_LOOP) != 0*/));
            }

            //read frame data
            myFrameData = new ushort[myHeader.num_frames * myHeader.num_framechannels];
            stream.Seek(myHeader.ofs_frames, SeekOrigin.Begin);
            for (int i = 0; i < myHeader.num_frames * myHeader.num_framechannels; i++)
            {
               myFrameData[i] = reader.ReadUInt16();
            }

            #endregion

            //read bounds
            myBounds = new iqmbounds[myHeader.num_frames];
            stream.Seek(myHeader.ofs_bounds, SeekOrigin.Begin);
            for (int i = 0; i < myHeader.num_frames; i++)
            {
               iqmbounds temp = new iqmbounds();
               temp.bbmins = new float[3];
               temp.bbmaxs = new float[3];
               temp.bbmins[0] = reader.ReadSingle();
               temp.bbmins[1] = reader.ReadSingle();
               temp.bbmins[2] = reader.ReadSingle();
               temp.bbmaxs[0] = reader.ReadSingle();
               temp.bbmaxs[1] = reader.ReadSingle();
               temp.bbmaxs[2] = reader.ReadSingle();
               temp.xyradius = reader.ReadSingle();
               temp.radius = reader.ReadSingle();

               if (i == 0)
               {
                  sm.size = temp.radius;
               }
            }            

            //read comments
            stream.Seek(myHeader.ofs_comment, SeekOrigin.Begin);
            int charRead = 0;
            while (charRead < myHeader.num_comment)
            {
               char c = reader.ReadChar(); charRead++;
               string s = "";
               while (c != '\0')
               {
                  s += c;
                  c = reader.ReadChar(); charRead++;
               }

               myComments.Add(s);
            }

            //read extensions
            //TODO

            //setup the bone data
            Matrix4[] baseframe = new Matrix4[myHeader.num_joints];
            Matrix4[] inversebaseframe = new Matrix4[myHeader.num_joints];
            for (int i = 0; i < (int)myHeader.num_joints; i++)
            {
               iqmjoint joint = myJoints[i];
               Matrix4 r, t, s;
               r = Matrix4.CreateFromQuaternion(joint.rotate);
               t = Matrix4.CreateTranslation(joint.translate);
               s = Matrix4.CreateScale(joint.scale);
               baseframe[i] = s * r * t;
               inversebaseframe[i] = baseframe[i].Inverted();
               if (joint.parent >= 0)
               {
                  baseframe[i] = baseframe[i] * baseframe[joint.parent];
                  inversebaseframe[i] = inversebaseframe[joint.parent] * inversebaseframe[i];
               }
            }

            Matrix4[] absMatrix = new Matrix4[myHeader.num_frames * myHeader.num_poses];
            int count = 0;
            for (int i = 0; i < myHeader.num_frames; i++)
            {
               for (int j = 0; j < myHeader.num_poses; j++)
               {
                  iqmpose p = myPoses[j];
                  Quaternion rotate = new Quaternion();
                  Vector3 translate = new Vector3();
                  Vector3 scale = new Vector3();
                  translate.X = p.channeloffset[0]; if ((p.channelmask & 0x01) != 0) translate.X += myFrameData[count++] * p.channelscale[0];
                  translate.Y = p.channeloffset[1]; if ((p.channelmask & 0x02) != 0) translate.Y += myFrameData[count++] * p.channelscale[1];
                  translate.Z = p.channeloffset[2]; if ((p.channelmask & 0x04) != 0) translate.Z += myFrameData[count++] * p.channelscale[2];
                  rotate.X = p.channeloffset[3]; if ((p.channelmask & 0x08) != 0) rotate.X += myFrameData[count++] * p.channelscale[3];
                  rotate.Y = p.channeloffset[4]; if ((p.channelmask & 0x10) != 0) rotate.Y += myFrameData[count++] * p.channelscale[4];
                  rotate.Z = p.channeloffset[5]; if ((p.channelmask & 0x20) != 0) rotate.Z += myFrameData[count++] * p.channelscale[5];
                  rotate.W = p.channeloffset[6]; if ((p.channelmask & 0x40) != 0) rotate.W += myFrameData[count++] * p.channelscale[6];
                  scale.X = p.channeloffset[7]; if ((p.channelmask & 0x80) != 0) scale.X += myFrameData[count++] * p.channelscale[7];
                  scale.Y = p.channeloffset[8]; if ((p.channelmask & 0x100) != 0) scale.Y += myFrameData[count++] * p.channelscale[8];
                  scale.Z = p.channeloffset[9]; if ((p.channelmask & 0x200) != 0) scale.Z += myFrameData[count++] * p.channelscale[9];
                  // Concatenate each pose with the inverse base pose to avoid doing this at animation time.
                  // If the joint has a parent, then it needs to be pre-concatenated with its parent's base pose.
                  // Thus it all negates at animation time like so: 
                  //   (parentPose * parentInverseBasePose) * (parentBasePose * childPose * childInverseBasePose) =>
                  //   parentPose * (parentInverseBasePose * parentBasePose) * childPose * childInverseBasePose =>
                  //   parentPose * childPose * childInverseBasePose
                  rotate.Normalize();
                  Matrix4 r, t, s;
                  r = Matrix4.CreateFromQuaternion(rotate);
                  t = Matrix4.CreateTranslation(translate);
                  s = Matrix4.CreateScale(scale);
                  Matrix4 pose = s * r * t;
                  if (p.parent >= 0)
                  {
                     Matrix4 parent = baseframe[p.parent];
                     Matrix4 inv = inversebaseframe[j];
                     Matrix4 parentPose = absMatrix[i * myHeader.num_poses + p.parent];
                     absMatrix[i * myHeader.num_poses + j] = inv * pose * parent * parentPose;
                  }
                  else
                  {
                     Matrix4 inv = inversebaseframe[j];
                     absMatrix[i * myHeader.num_poses + j] = inv * pose;
                  }
               }
            }

            Vector4[] boneData = new Vector4[myHeader.num_frames * myHeader.num_poses * 4];
            int next = 0;
            for (int i = 0; i < myHeader.num_frames * myHeader.num_poses; i++)
            {
               boneData[next++] = absMatrix[i].Row0;
               boneData[next++] = absMatrix[i].Row1;
               boneData[next++] = absMatrix[i].Row2;
               boneData[next++] = absMatrix[i].Row3;
            }

            //setup the buffers
            sm.myVbo.setData(vertexData);
            List<ushort> indexes = new List<ushort>();
            int indexCount = 0;
            for (int m = 0; m < sm.myMeshes.Count; m++)
            {
               Mesh mesh = sm.myMeshes[m];
					mesh.primativeType = PrimitiveType.Triangles;
               mesh.indexBase = indexCount;

               for (int t = 0; t < meshData[m].num_triangles; t++)
               {
						//swap the order the indicies since we want counter clockwise triangles instead of clockwise
                  indexes.Add(triangleIndexes[meshData[m].first_triangle * 3 + (t * 3) + 0]);
						indexes.Add(triangleIndexes[meshData[m].first_triangle * 3 + (t * 3) + 2]);
						indexes.Add(triangleIndexes[meshData[m].first_triangle * 3 + (t * 3) + 1]);
						indexCount+=3;
               }

               mesh.indexCount = indexCount - mesh.indexBase;
            }

            sm.myIbo.setData(indexes);

            //upload the frame data
            sm.myFrames.setData(boneData);

            return sm;
         }
         catch (Exception ex)
         {
            throw new Exception("Error while loading IQM model from definition file ( " + filename + " ).", ex);
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

      string readNullTerminated(byte[] texts, UInt32 offset)
      {
         MemoryStream ms = new MemoryStream(texts);
         BinaryReader reader = new BinaryReader(ms);
         ms.Seek(offset, SeekOrigin.Begin);
         char c = reader.ReadChar();
         string ret = "";
         while (c != '\0')
         {
            ret += c;
            c = reader.ReadChar();
         }

         return ret;
      }
   }
}