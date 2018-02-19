using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using Util;

namespace Graphics
{
   public class ParticleSystem : Renderable
   {
      public List<Particle> particles = new List<Particle>();     
      public VertexBufferObject<V3C4S3R> vbo = new VertexBufferObject<V3C4S3R>(BufferUsageHint.StreamDraw);
 
      public List<ParticleFeature> features { get; set; }
      public Texture material { get; set; }
      public bool continuous { get; set; }
      public float lifetime { get; set; }
      public int maxParticles { get; set; }
      public Color4 color { get; set; }

      public ParticleSystem()
         :base("particle")
      {
         continuous = true;
         lifetime = 10.0f;
         maxParticles = 100;
         features = new List<ParticleFeature>();
         position = Vector3.Zero;
         color = Color4.White;
      }

      public override bool isVisible(Camera c)
      {
         //TODO: calculate size
         return c.containsSphere(position, 10.0f);
      }

      public override void update(float dt)
      {
         if (continuous == false)
            lifetime -= dt;

         //reset the force on each particle
         foreach (Particle p in particles)
         {
            p.force = Vector3.Zero;
         }

         //update each of the features
         foreach (ParticleFeature pf in features)
         {
            if(pf.active==true)
               pf.tick(ref particles, dt);
         }

         //update each of the particles
         List<Particle> toRemove = new List<Particle>();
         foreach (Particle p in particles)
         {
            p.tick(dt);
            if (p.life <= 0)
               toRemove.Add(p);
         }

         //remove any dead ones
         foreach (Particle p in toRemove)
         {
            particles.Remove(p);
         }
      }

      public bool ended()
      {
         if (continuous == true) return false;

         return lifetime <= 0;
      }

      public void addFeature(ParticleFeature feature)
      {
         features.Add(feature);
         feature.mySystem = this;
      }

      public void removeFeature(ParticleFeature feature)
      {
         features.Remove(feature);
      }

      public void updateVbo()
      {
         V3C4S3R[] data=new V3C4S3R[particles.Count]; 
         for (int i = 0; i < particles.Count; i++)
         {
            data[i].Position = particles[i].position;
            data[i].Color.X = particles[i].color.R;
            data[i].Color.Y = particles[i].color.G;
            data[i].Color.Z = particles[i].color.B;
            data[i].Color.W = particles[i].color.A;
            data[i].Size = particles[i].size;
            data[i].Rotation = particles[i].rotation;
         }
       
         vbo.setData(data);
      }
   }
}
