using System;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using Util;

namespace Graphics
{
   public struct AttributeInfo
   {
      public int id;
      public string name;
      public int size;
      public ActiveAttribType type;
   }

   public struct UniformInfo
   {
      public int id;
      public string name;
      public int size;
      public ActiveUniformType type;
   }

   public struct UniformBlockInfo
   {
      public int id;
      public string name;
   }

   public struct UniformData
   {
      public int location;
      public Uniform.UniformType type;
      public Object data;

      public UniformData(int id, Uniform.UniformType t, Object d)
      {
         location = id;
         type = t;
         data = d;
      }
   }

   public class Uniform
   {
      public enum UniformType { Bool, Int, Float, Double, Vec2, Vec3, Vec4, IVec2, IVec3, IVec4, DVec2, DVec3, DVec4, Quat, Color4, Mat4, DMat4, Mat4Array, DMat4Array };

      protected String myName;
      protected int myLocation;
      protected int mySize;
      protected UniformType myType;
      protected struct InternalValue
      {
         public bool myBool;
         public int myInt;
         public float myFloat;
         public double myDouble;
         public Vector2 myVec2;
         public Vector2d myDVec2;
         public Vector3 myVec3;
         public Vector3d myDVec3;
         public Vector4 myVec4;
         public Vector4d myDVec4;
         public Matrix4 myMat4;
         public Matrix4d myDMat4;
         public Matrix4[] myMat4Array;
         public Matrix4d[] myDMat4Array;
      };

      protected InternalValue myValue;

      public Uniform(ShaderProgram program, UniformInfo ui)
      {
         myName = ui.name;
         myLocation = ui.id;
         mySize = ui.size;
         dirty = true;
         switch (ui.type)
         {
            case ActiveUniformType.Bool:
               myType = UniformType.Bool;
               break;
            case ActiveUniformType.Image1D:
            case ActiveUniformType.Image2D:
            case ActiveUniformType.Image3D:
            case ActiveUniformType.Sampler1D:
            case ActiveUniformType.Sampler2D:
            case ActiveUniformType.Sampler3D:
            case ActiveUniformType.SamplerCube:
            case ActiveUniformType.Sampler2DShadow:
            case ActiveUniformType.Sampler2DArray:
            case ActiveUniformType.SamplerBuffer:
            case ActiveUniformType.UnsignedIntSamplerBuffer:              
            case ActiveUniformType.Int:
               myType = UniformType.Int;
               break;
            case ActiveUniformType.Float:
               myType = UniformType.Float;
               break;
            case ActiveUniformType.FloatVec2:
               myType = UniformType.Vec2;
               break;
            case ActiveUniformType.FloatVec3:
               myType = UniformType.Vec3;
               break;
            case ActiveUniformType.FloatVec4:
               myType = UniformType.Vec4;
               break;
            case ActiveUniformType.FloatMat4:
               if (ui.size > 1)
                  myType = UniformType.Mat4Array;
               else
                  myType = UniformType.Mat4;
               break;
            case ActiveUniformType.IntVec2:
               myType = UniformType.IVec2;
               break;
            case ActiveUniformType.IntVec3:
               myType = UniformType.IVec3;
               break;
            case ActiveUniformType.IntVec4:
               myType = UniformType.IVec4;
               break;
            default:
               throw new Exception(String.Format("Need to support: {0}", ui.type));
         }
      }

      public int location { get { return myLocation; } }
      public String name { get { return myName; } }

      public bool dirty { get; set; }

      #region "Set Values"
      public void setValue(bool val)
      {
         if (myValue.myBool != val)
         {
            myValue.myBool = val;
            dirty = true;
         }
      }

      public void setValue(int val)
      {
         if (myValue.myInt != val)
         {
            myValue.myInt = val;
            dirty = true;
         }
      }

      public void setValue(float val)
      {
         if (myValue.myFloat != val)
         {
            myValue.myFloat = val;
            dirty = true;
         }
      }

      public void setValue(Vector2 val)
      {
         if (myValue.myVec2 != val)
         {
            myValue.myVec2 = val;
            dirty = true;
         }
      }

      public void setValue(Vector3 val)
      {
         if (myValue.myVec3 != val)
         {
            myValue.myVec3 = val;
            dirty = true;
         }
      }

      public void setValue(Vector4 val)
      {
         if (myValue.myVec4 != val)
         {
            myValue.myVec4 = val;
            dirty = true;
         }
      }

      public void setValue(Quaternion val)
      {
         Vector4 castVal = new Vector4(val.Xyz, val.W);
         if (myValue.myVec4 != castVal)
         {
            myValue.myVec4 = castVal;
            dirty = true;
         }
      }

      public void setValue(Color4 val)
      {
         Vector4 castVal = new Vector4(val.R, val.G, val.B, val.A);
         if (myValue.myVec4 != castVal)
         {
            myValue.myVec4 = castVal;
            dirty = true;
         }
      }

      public void setValue(Matrix4 val)
      {
         if (myValue.myMat4 != val)
         {
            myValue.myMat4 = val;
            dirty = true;
         }
      }

      public void setValue(Matrix4[] val)
      {
         if (myValue.myMat4Array != val)
         {
            myValue.myMat4Array = val;
            dirty = true;
         }
      }
      #endregion

      public void apply()
      {
         if (dirty == true)
         {
            switch (myType)
            {
               case UniformType.Bool:
                  {
                     int val = myValue.myBool == true ? 1 : 0;
                     GL.Uniform1(myLocation, val);
                  }
                  break;
               case UniformType.Int:
                  {
                     GL.Uniform1(myLocation, myValue.myInt);
                  }
                  break;
               case UniformType.Float:
                  {
                     GL.Uniform1(myLocation, myValue.myFloat);
                  }
                  break;
               case UniformType.Vec2:
                  {
                     GL.Uniform2(myLocation, myValue.myVec2);
                  }
                  break;
					case UniformType.IVec2:
						{
							GL.Uniform2(myLocation, (int)myValue.myVec2.X, (int)myValue.myVec2.Y);
						}
						break;
					case UniformType.Vec3:
                  {
                     GL.Uniform3(myLocation, myValue.myVec3);
                  }
                  break;
					case UniformType.IVec3:
						{
							GL.Uniform3(myLocation, (int)myValue.myVec3.X, (int)myValue.myVec3.Y, (int)myValue.myVec3.Z);
						}
						break;
					case UniformType.Vec4:
                  {
                     GL.Uniform4(myLocation, myValue.myVec4);
                  }
                  break;
					case UniformType.IVec4:
						{
							GL.Uniform4(myLocation, (int)myValue.myVec4.X, (int)myValue.myVec4.Y, (int)myValue.myVec4.Z, (int)myValue.myVec4.W);
						}
						break;
               case UniformType.Mat4:
                  {
                     GL.UniformMatrix4(myLocation, false, ref myValue.myMat4);
                  }
                  break;
               case UniformType.Mat4Array:
                  {
                     float[] floats = null;

                     Matrix4Ext.ToOpenGL(myValue.myMat4Array, ref floats);
                     GL.UniformMatrix4(myLocation, myValue.myMat4Array.Length, true, floats);
                  }
                  break;
            }

            dirty = false;
         }
      }
   }
}
