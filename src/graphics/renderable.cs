using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;


using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using Util;

namespace Graphics
{
   public abstract class Renderable
   {
      protected String myType;
      Matrix4 myTransform;
      Matrix4 myScale;
      Matrix4 myOrientation;
      Matrix4 myModelMatrix;
      bool myDirty = false;

      public Vector3 position;
      public Vector3 rotation;
      public Vector3 scale;

      public List<Controller> controllers = new List<Controller>();

      public Renderable(string type)
      {
         myType = type;
         myTransform = Matrix4.CreateTranslation(Vector3.Zero);
         myScale = Matrix4.CreateScale(Vector3.One);
         myOrientation = Matrix4.Identity;
         myModelMatrix = myTransform * myScale * myOrientation;
      }
      public String type { get { return myType; } }

      public abstract bool isVisible(Camera c);

      public Controller findController(String name)
      {
         Controller controller = controllers.Find(c => c.name == name);
         return controller;
      }

      public virtual void update(float dt)
      {
         foreach (Controller c in controllers)
         {
            c.update(dt);
         }
      }

      public Matrix4 modelMatrix { get { if (myDirty == true) updateModelMatrix();  return myModelMatrix; } }

      void updateModelMatrix()
      {
         myModelMatrix = myScale * myOrientation * myTransform;
         myDirty = false;
      }

      public virtual void setPosition(Vector3 newPos)
      {
         position = newPos;
         myTransform = Matrix4.CreateTranslation(newPos);
         myDirty = true;
      }

      public virtual void setOrientation(Vector3 newOri)
      {
         rotation = newOri;
         Matrix4 m = new Matrix4();
         myOrientation = m.fromHeadingPitchRoll(newOri.X, newOri.Y, newOri.Z);
         myDirty = true;
      }

      public virtual void setOrientation(Quaternion newOri)
      {
         Matrix4 m = Matrix4.CreateFromQuaternion(newOri);
         myOrientation = m;
         myDirty = true;
      }

      public virtual void setScale(Vector3 newScale)
      {
         scale = newScale;
         myScale = Matrix4.CreateScale(newScale);
         myDirty = true;
      }
   }
}