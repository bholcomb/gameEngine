using System;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;

using Util;

namespace Graphics
{
   public abstract class ParticleFeatureCreator
   {
      protected String myName;

      public ParticleFeatureCreator()
      {
      }

      public abstract ParticleFeature create(JsonObject initData);

      public String name
      {
         get { return myName; }
      }
   }

   public abstract class ParticleFeature
   {
      static Random theRandom = new Random();

      public ParticleSystem mySystem;
      public enum FeatureType { INIT, UPDATE, END }
      public FeatureType myType;

      protected String myName;

      public bool active { get; set; }
      public String name { get { return myName; } }

      public ParticleFeature(FeatureType type, String n)
      {
         myType = type;
         active = true;
         myName = n;
      }

      public abstract void tick(ref List<Particle> particles, float dt);

      public static float randomValueBetween(float min, float max)
      {
         float d = max - min;
         float p = ((float)theRandom.Next(100)) / 100.0f;

         return min + (p * d);
      }

      public static float valueWithVariance(float val, float var)
      {
         float p = ((float)theRandom.Next(200) - 100.0f) / 100.0f;
         float ret = val + (p * var);
         return ret;
      }

      public static Vector2 valueWithVariance(Vector2 val, Vector2 var)
      {
         float px = ((float)theRandom.Next(200) - 100.0f) / 100.0f;
         float py = ((float)theRandom.Next(200) - 100.0f) / 100.0f;
         Vector2 ret = Vector2.Zero;
         ret.X = val.X + (px * var.X);
         ret.Y = val.Y + (py * var.Y);
         return ret;
      }

      public static Vector3 valueWithVariance(Vector3 val, Vector3 var)
      {
         float px = ((float)theRandom.Next(200) - 100.0f) / 100.0f;
         float py = ((float)theRandom.Next(200) - 100.0f) / 100.0f;
         float pz = ((float)theRandom.Next(200) - 100.0f) / 100.0f;
         Vector3 ret = Vector3.Zero;
         ret.X = val.X + (px * var.X);
         ret.Y = val.Y + (py * var.Y);
         ret.Z = val.Z + (pz * var.Z);
         return ret;
      }
   }

   public class LinearForceFeatureCreator : ParticleFeatureCreator
   {
      public LinearForceFeatureCreator() : base() { { myName = "linear"; } }
      public override ParticleFeature create(JsonObject initData)
      {
         Vector3 vec = new Vector3();
         if (initData["force"] != null)
         {
            JsonObject v = initData["force"];
            vec.X = (float)v["x"];
            vec.Y = (float)v["y"];
            vec.Z = (float)v["z"];
         }
         LinearForceFeature lf = new LinearForceFeature(vec);
         return lf;
      }
   }

   public class LinearForceFeature : ParticleFeature
   {
      public Vector3 force { get; set; }

      public LinearForceFeature(Vector3 f)
         : base(ParticleFeature.FeatureType.UPDATE, "linear")
      {
         force = f;
      }

      public override void tick(ref List<Particle> particles, float dt)
      {
         foreach (Particle p in particles)
         {
            p.force += force;
         }
      }
   }

   public class RadialForceFeatureCreator : ParticleFeatureCreator
   {
      public RadialForceFeatureCreator() : base() { myName = "radial"; }
      public override ParticleFeature create(JsonObject initData)
      {
         float val = 0f;
         if (initData["magnitude"] != null)
         {
            val = (float)initData["magnitude"];
         }
         RadialForceFeature lf = new RadialForceFeature(val);
         return lf;
      }
   }

   public class RadialForceFeature : ParticleFeature
   {
      public float value { get; set; }

      public RadialForceFeature(float magnitude)
         : base(ParticleFeature.FeatureType.UPDATE, "radial")
      {
         value = magnitude;
      }

      public override void tick(ref List<Particle> particles, float dt)
      {
         foreach (Particle p in particles)
         {
            Vector3 v = p.position * -1;
            if (v.LengthSquared > 0)
            {
               v.Y = 0; //don't account for any force in the up direction
               //v.Normalize();
               v = v * value;
               p.force += v * p.mass;
            }
         }
      }
   }

   public class TangentialForceFeatureCreator : ParticleFeatureCreator
   {
      public TangentialForceFeatureCreator() : base() { myName = "tangent"; }
      public override ParticleFeature create(JsonObject initData)
      {
         float val = 0f;
         if (initData["magnitude"] != null)
         {
            val = (float)initData["magnitude"];
         }
         TangentialForceFeature lf = new TangentialForceFeature(val);
         return lf;
      }
   }

   public class TangentialForceFeature : ParticleFeature
   {
      public float value { get; set; }

      public TangentialForceFeature(float magnitude)
         : base(ParticleFeature.FeatureType.UPDATE, "tangent")
      {
         value = magnitude;
      }

      public override void tick(ref List<Particle> particles, float dt)
      {
         foreach (Particle p in particles)
         {
            Vector3 v = p.position * -1;
            if (v.LengthSquared > 0)
            {
               v = Vector3.Cross(p.up, v); //this should give the vector to the right
               v.Y = 0;  //don't account for any force in the up direction
               //v.Normalize();
               v = v * value;
               p.force += v * p.mass;
            }
         }
      }
   }

   public class DragForceFeatureCreator : ParticleFeatureCreator
   {
      public DragForceFeatureCreator() : base() { myName = "drag"; }
      public override ParticleFeature create(JsonObject initData)
      {
         float val = 0f;
         if (initData["magnitude"] != null)
         {
            val = (float)initData["magnitude"];
         }
         DragForceFeature lf = new DragForceFeature(val);
         return lf;
      }
   }

   public class DragForceFeature : ParticleFeature
   {
      public float value { get; set; }

      public DragForceFeature(float magnitude)
         : base(ParticleFeature.FeatureType.UPDATE, "drag")
      {
         value = magnitude;
      }

      public override void tick(ref List<Particle> particles, float dt)
      {
         foreach (Particle p in particles)
         {
            Vector3 f = p.velocity * -value;
            p.force += f;
         }
      }
   }

   public class RotationFeatureCreator : ParticleFeatureCreator
   {
      public RotationFeatureCreator() : base() { myName = "rotation"; }
      public override ParticleFeature create(JsonObject initData)
      {
         float val = 0f;
         if (initData["value"] != null)
         {
            val = (float)initData["value"];
         }
         RotationFeature lf = new RotationFeature(val);
         return lf;
      }
   }

   public class RotationFeature : ParticleFeature
   {
      public float value { get; set; }

      public RotationFeature(float rate)
         : base(ParticleFeature.FeatureType.UPDATE, "rotation")
      {
         value = rate;
      }

      public override void tick(ref List<Particle> particles, float dt)
      {
         foreach (Particle p in particles)
         {
            p.rotation += value * dt;
         }
      }
   }

   public class ScaleFeatureCreator : ParticleFeatureCreator
   {
      public ScaleFeatureCreator() : base() { myName = "scale"; }
      public override ParticleFeature create(JsonObject initData)
      {
         Vector3 vec = new Vector3();
         if (initData["scale"] != null)
         {
            JsonObject v = initData["scale"];
            vec.X = (float)v["x"];
            vec.Y = (float)v["y"];
            vec.Z = (float)v["z"];
         }
         ScaleFeature lf = new ScaleFeature(vec);
         return lf;
      }
   }

   public class ScaleFeature : ParticleFeature
   {
      public Vector3 scale { get; set; }

      public ScaleFeature(Vector3 rate)
         : base(ParticleFeature.FeatureType.UPDATE, "scale")
      {
         scale = rate;
      }

      public override void tick(ref List<Particle> particles, float dt)
      {
         foreach (Particle p in particles)
         {
            p.size += scale * dt;
            if (p.size.X < 0) p.size.X = 0;
            if (p.size.Y < 0) p.size.Y = 0;
            if (p.size.Z < 0) p.size.Z = 0;
         }
      }
   }

   public class AlphaFeatureCreator : ParticleFeatureCreator
   {
      public AlphaFeatureCreator() : base() { myName = "alpha"; }
      public override ParticleFeature create(JsonObject initData)
      {
         float val = 0f;
         if (initData["alphaRate"] != null)
         {
            val = (float)initData["alphaRate"];
         }
         AlphaFeature lf = new AlphaFeature(val);
         return lf;
      }
   }

   public class AlphaFeature : ParticleFeature
   {
      public float value { get; set; }

      public AlphaFeature(float rate)
         : base(ParticleFeature.FeatureType.UPDATE, "alpha")
      {
         value = rate;
      }

      public override void tick(ref List<Particle> particles, float dt)
      {
         foreach (Particle p in particles)
         {
            p.color.A += value * dt;
            if (p.color.A < 0.0f) p.color.A = 0.0f;
            if (p.color.A > 1.0f) p.color.A = 1.0f;
         }
      }
   }

   public class AlphaFromLifeFeatureCreator : ParticleFeatureCreator
   {
      public AlphaFromLifeFeatureCreator() : base() { myName = "alphaFromLife"; }
      public override ParticleFeature create(JsonObject initData)
      {
         AlphaFromLifeFeature lf = new AlphaFromLifeFeature();
         return lf;
      }
   }

   public class AlphaFromLifeFeature : ParticleFeature
   {
      public AlphaFromLifeFeature()
         : base(ParticleFeature.FeatureType.UPDATE, "alphaFromLife")
      {
      }

      public override void tick(ref List<Particle> particles, float dt)
      {
         foreach (Particle p in particles)
         {
            p.color.A = p.life;
            if (p.color.A < 0.0f) p.color.A = 0.0f;
            if (p.color.A > 1.0f) p.color.A = 1.0f;
         }
      }
   }

   public class EmitterFeatureCreator : ParticleFeatureCreator
   {
      public EmitterFeatureCreator() : base() { myName = "emitter"; }
      public override ParticleFeature create(JsonObject initData)
      {
         ParticleInitializationData init = new ParticleInitializationData();
         String emmitType = (String)initData["emitterType"];
         switch (emmitType)
         {
            case "point": init.myEmitterType = EmitterFeature.EmitterType.POINT; break;
            case "line": init.myEmitterType = EmitterFeature.EmitterType.LINE; break;
            case "plane": init.myEmitterType = EmitterFeature.EmitterType.PLANE; break;
            case "circle": init.myEmitterType = EmitterFeature.EmitterType.CIRCLE; break;
         }

         init.myContinous = (bool)initData["continuous"];
         init.myEmissionRate = (float)initData["emissionRate"];
         init.myP1 = (Vector3)initData["p1"];
         init.myP1Variance = (Vector3)initData["p1Variance"];
         init.myP2 = (Vector3)initData["p2"];
         init.myP2Variance = (Vector3)initData["p2Variance"];
         init.myVelocity = (Vector3)initData["velocity"];
         init.myVelocityVariance = (Vector3)initData["velocityVariance"];
         init.mySize = (Vector3)initData["size"];
         init.mySizeVariance = (Vector3)initData["sizeVariance"];
         init.myColor = (Color4)initData["color"];
         init.myColorVariance = (Color4)initData["colorVariance"];
         init.myRotation = (float)initData["rotation"];
         init.myRotationVariance = (float)initData["rotationVariance"];
         init.myMass = (float)initData["mass"];
         init.myMassVariance = (float)initData["massVariance"];
         init.myLifetime = (float)initData["lifetime"];
         init.myLifetimeVariance = (float)initData["lifetimeVariance"];

         EmitterFeature lf = new EmitterFeature(init);
         return lf;
      }
   }

   public class EmitterFeature : ParticleFeature
   {
      public enum EmitterType { POINT, LINE, CIRCLE, PLANE };
      public EmitterType emitterType { get; set; }
      public float emitterLifetime { get; set; }
      public bool continuous { get; set; }
      public float emisionRate { get; set; }
      public Vector3 p1 { get; set; }
      public Vector3 p1Var { get; set; }
      public Vector3 p2 { get; set; }
      public Vector3 P2Var { get; set; }
      public Vector3 velocity { get; set; }
      public Vector3 velocityVar { get; set; }
      public Vector3 size { get; set; }
      public Vector3 sizeVar { get; set; }
      public Color4 color { get; set; }
      public Color4 colorVar { get; set; }
      public float rotation { get; set; }
      public float rotationVar { get; set; }
      public float mass { get; set; }
      public float massVar { get; set; }
      public float lifetime { get; set; }
      public float lifetimeVar { get; set; }

      float myEmmisionTimer;

      public EmitterFeature(ParticleInitializationData init)
         : base(ParticleFeature.FeatureType.INIT, "emitter")
      {
         emitterType = init.myEmitterType;
         continuous = init.myContinous;
         emisionRate = init.myEmissionRate;
         p1 = init.myP1;
         p1Var = init.myP1Variance;
         p2 = init.myP2;
         P2Var = init.myP2Variance;
         velocity = init.myVelocity;
         velocityVar = init.myVelocityVariance;
         size = init.mySize;
         sizeVar = init.mySizeVariance;
         color = init.myColor;
         colorVar = init.myColorVariance;
         rotation = init.myRotation;
         rotationVar = init.myRotationVariance;
         mass = init.myMass;
         massVar = init.myMassVariance;
         lifetime = init.myLifetime;
         lifetimeVar = init.myLifetimeVariance;
      }

      public override void tick(ref List<Particle> particles, float dt)
      {
         //check for the emmitter being done
         if (continuous == false)
         {
            emitterLifetime -= dt;
            if (emitterLifetime <= 0)
            {
               active = false;
            }
         }

         //there are particles allowed to be created
         if ((mySystem.maxParticles - particles.Count) > 0)
         {
            //add the elapsed time to the emmision timer
            myEmmisionTimer += dt;
            float emmitFreq = 1 / emisionRate; //get the rate

            while ((myEmmisionTimer - emmitFreq) > 0)
            {
               myEmmisionTimer -= emmitFreq;

               //create them with some inital conditions
               Particle p = new Particle();

               p.position = newPosition();
               p.velocity = valueWithVariance(velocity, velocityVar);
               p.size = valueWithVariance(size, sizeVar);
               p.color = color;
               p.rotation = valueWithVariance(rotation, rotationVar);
               p.mass = valueWithVariance(mass, massVar);
               p.life = valueWithVariance(lifetime, lifetimeVar);

               particles.Add(p);
               if (particles.Count == mySystem.maxParticles)
               {
                  break;
               }
            }
         }
      }

      Vector3 newPosition()
      {
         switch (emitterType)
         {
            case EmitterType.POINT:
               {
                  return mySystem.position + valueWithVariance(p1, p1Var);
               }
            case EmitterType.LINE:
               {
                  Vector3 v = p2 - p1;
                  float p = randomValueBetween(0.0f, 1.0f);
                  v = p1 + (v * p);
                  return mySystem.position + valueWithVariance(v, p1Var);
               }
            case EmitterType.CIRCLE:
               {
                  float r = randomValueBetween(0.0f, (float)Math.PI * 2.0f);
                  float x = p1.X * (float)Math.Cos((double)r);
                  float z = p1.Y * (float)Math.Sin((double)r);
                  Vector3 pos = new Vector3(x, 0.0f, z);
                  if (p2.LengthSquared > 0)
                  {
                     Quaternion q = Quaternion.FromAxisAngle(p2, 0);
                     pos = Vector3.TransformVector(pos, q.toMatrix());
                  }
                  return mySystem.position + valueWithVariance(pos, p1Var);
               }
            case EmitterType.PLANE:
               {
                  float x = randomValueBetween(0.0f, p1.X);
                  float z = randomValueBetween(0.0f, p1.Y);
                  Vector3 pos = new Vector3(x, 0.0f, z);
                  if (p2.LengthSquared > 0)
                  {
                     Quaternion q = Quaternion.FromAxisAngle(p2, 0);
                     pos = Vector3.TransformVector(pos, q.toMatrix());
                  }
                  return mySystem.position + valueWithVariance(pos, p1Var);
               }
         }

         return Vector3.Zero;
      }
   }

   public class FeatureSerializer
   {
      public FeatureSerializer()
      {

      }

      public static JsonObject serialize(ParticleFeature feature)
      {
         JsonObject obj = null;
         if (feature is LinearForceFeature) obj = serialize(feature as LinearForceFeature);
         if (feature is RadialForceFeature) obj = serialize(feature as RadialForceFeature);
         if (feature is TangentialForceFeature) obj = serialize(feature as TangentialForceFeature);
         if (feature is DragForceFeature) obj = serialize(feature as DragForceFeature);
         if (feature is RotationFeature) obj = serialize(feature as RotationFeature);
         if (feature is ScaleFeature) obj = serialize(feature as ScaleFeature);
         if (feature is AlphaFeature) obj = serialize(feature as AlphaFeature);
         if (feature is AlphaFromLifeFeature) obj = serialize(feature as AlphaFromLifeFeature);
         if (feature is EmitterFeature) obj = serialize(feature as EmitterFeature);

         return obj;
      }

      public static JsonObject serialize(LinearForceFeature feature)
      {
         JsonObject obj = new JsonObject(JsonObject.JsonType.OBJECT);
         JsonObject force = feature.force;
         obj["force"] = force;
         return obj;
      }

      public static JsonObject serialize(RadialForceFeature feature)
      {
         JsonObject obj = new JsonObject(JsonObject.JsonType.OBJECT);
         obj["magnitude"] = feature.value;
         return obj;
      }

      public static JsonObject serialize(TangentialForceFeature feature)
      {
         JsonObject obj = new JsonObject(JsonObject.JsonType.OBJECT);
         obj["magnitude"] = feature.value;
         return obj;
      }

      public static JsonObject serialize(DragForceFeature feature)
      {
         JsonObject obj = new JsonObject(JsonObject.JsonType.OBJECT);
         obj["magnitude"] = feature.value;
         return obj;
      }

      public static JsonObject serialize(RotationFeature feature)
      {
         JsonObject obj = new JsonObject(JsonObject.JsonType.OBJECT);
         obj["value"] = feature.value;
         return obj;
      }

      public static JsonObject serialize(ScaleFeature feature)
      {
         JsonObject obj = new JsonObject(JsonObject.JsonType.OBJECT);
         JsonObject scale = feature.scale;
         obj["scale"] = scale;
         return obj;
      }

      public static JsonObject serialize(AlphaFeature feature)
      {
         JsonObject obj = new JsonObject(JsonObject.JsonType.OBJECT);
         obj["alphaRate"] = feature.value;
         return obj;
      }

      public static JsonObject serialize(AlphaFromLifeFeature feature)
      {
         JsonObject obj = new JsonObject(JsonObject.JsonType.OBJECT);
         return obj;
      }

      public static JsonObject serialize(EmitterFeature feature)
      {
         JsonObject obj = new JsonObject(JsonObject.JsonType.OBJECT);
         switch (feature.emitterType)
         {
            case EmitterFeature.EmitterType.POINT: obj["emitterType"] = "point"; break;
            case EmitterFeature.EmitterType.LINE: obj["emitterType"] = "line"; break;
            case EmitterFeature.EmitterType.PLANE: obj["emitterType"] = "plane"; break;
            case EmitterFeature.EmitterType.CIRCLE: obj["emitterType"] = "circle"; break;
         }
         obj["continuous"] = feature.continuous;
         obj["emissionRate"] = feature.emisionRate;
         obj["p1"] = feature.p1;
         obj["P1Variance"] = feature.p1Var;
         obj["p2"] = feature.p2;
         obj["p2Variance"] = feature.P2Var;
         obj["velocity"] = feature.velocity;
         obj["velocityVariance"] = feature.velocityVar;
         obj["size"] = feature.size;
         obj["sizeVariance"] = feature.sizeVar;
         obj["color"] = feature.color;
         obj["colorVariance"] = feature.colorVar;
         obj["rotation"] = feature.rotation;
         obj["rotationVariance"] = feature.rotationVar;
         obj["mass"] = feature.mass;
         obj["massVariance"] = feature.massVar;
         obj["lifetime"] = feature.lifetime;
         obj["lifetimeVariance"] = feature.lifetimeVar;
         return obj;
      }
   }
}
