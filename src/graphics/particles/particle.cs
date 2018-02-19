using System;

using OpenTK;
using OpenTK.Graphics;

namespace Graphics
{
   public struct ParticleInitializationData
   {
      public EmitterFeature.EmitterType myEmitterType { get; set; }
      public bool myContinous { get; set; }
      public float myEmissionRate { get; set; }

      public Vector3 myP1 { get; set; }
      public Vector3 myP1Variance  { get; set; }
      public Vector3 myP2 { get; set; }
      public Vector3 myP2Variance { get; set; }
      public Vector3 myVelocity { get; set; }
      public Vector3 myVelocityVariance { get; set; }
      public Vector3 mySize { get; set; }
      public Vector3 mySizeVariance { get; set; }
      public Color4 myColor { get; set; }
      public Color4 myColorVariance { get; set; }

      public float myRotation { get; set; }
      public float myRotationVariance { get; set; }
      public float myMass { get; set; }
      public float myMassVariance { get; set; }
      public float myLifetime { get; set; }
      public float myLifetimeVariance { get; set; }
   };

   public class Particle
   {
      public Vector3 position;
      public Vector3 velocity;
      public Vector3 force;
      public Vector3 up=Vector3.UnitY;
      public float rotation;
      public float life;
      public float mass=1.0f;
      public Color4 color;
      public Vector3 size;

      public Particle()
      { }

      public void tick(float dt)
      {
         //move the particles
         Vector3 accel = force / mass;
         velocity += accel * dt;
         position += velocity * dt;
         life -= dt;
      }
   }
}
