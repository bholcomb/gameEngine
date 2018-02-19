using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using OpenTK;
using Util;

namespace Graphics
{
   public class ParticleManager
   {
      static List<ParticleSystem> particleSystems = new List<ParticleSystem>();
      static Dictionary<string, ParticleFeatureCreator> featureFactory = new Dictionary<string, ParticleFeatureCreator>();
     
      static ParticleManager()
      {
         addFeatureCreator(new LinearForceFeatureCreator());
         addFeatureCreator(new RadialForceFeatureCreator());
         addFeatureCreator(new TangentialForceFeatureCreator());
         addFeatureCreator(new DragForceFeatureCreator());
         addFeatureCreator(new RotationFeatureCreator());
         addFeatureCreator(new ScaleFeatureCreator());
         addFeatureCreator(new AlphaFeatureCreator());
         addFeatureCreator(new AlphaFromLifeFeatureCreator());
         addFeatureCreator(new EmitterFeatureCreator());
      }

      public static void tick(float dt)
      {
         List<ParticleSystem> toRemove=new List<ParticleSystem>();
         foreach (ParticleSystem ps in particleSystems)
         {
            ps.update(dt);
            if (ps.ended() == true)
            {
               toRemove.Add(ps);
            }
         }

         foreach (ParticleSystem ps in toRemove)
         {
            removeParticleSystem(ps);
         }
      }

      public static void addParticleSystem(ParticleSystem ps)
      {
         particleSystems.Add(ps);
      }

      public static void removeParticleSystem(ParticleSystem ps)
      {
         particleSystems.Remove(ps);
      }

      public static void addFeatureCreator(ParticleFeatureCreator fc)
      {
         featureFactory.Add(fc.name, fc);
      }

      public static ParticleSystem loadDefinition(string path)
      {
         JsonObject initData = JsonObject.loadFile(path);
         return createSystem(initData);
      }

      public static ParticleSystem createSystem(JsonObject initData)
      {
         ParticleSystem ps = new ParticleSystem();
         ps.continuous = (bool)initData["continuous"];
         ps.lifetime = (float)initData["lifetime"];
         ps.maxParticles = (int)initData["maxParticles"];
         String textureName = (string)initData["material"];
         TextureDescriptor td = new TextureDescriptor(textureName);
         ps.material = Renderer.resourceManager.getResource(td) as Texture;

         JsonObject features = initData["features"];
         foreach(String key in features.keys)
         {
            ParticleFeatureCreator creator = null;
            if (featureFactory.TryGetValue(key, out creator) == true)
            {
               JsonObject featureData = features[key];
               ParticleFeature f = creator.create(featureData);
               ps.addFeature(f);
            }
            else
            {
               //don't get here
               throw new Exception(String.Format("Unable to find creator for {0}", key));
            }
         }

         addParticleSystem(ps);
         return ps;
      }
   }
}
