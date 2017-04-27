using System;
using OpenTK;
using OpenTK.Graphics;

namespace ParticleEditor
{
   public class Particle
   {
      public Vector3 position;
      public Vector3 velocity;
      public Vector3 force;
      public Vector3 up=Vector3.UnitY;
      public float life;
      public float mass=1.0f;
      public Color4 color;
      public Vector3 scale;

      public Particle()
      { }

      public void tick(float dt)
      {
         //move the particles
         velocity += (force/mass) * dt;
         position += velocity * dt;
         life -= dt;
      }
   }
}
