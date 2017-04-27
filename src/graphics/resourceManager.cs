using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;


using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using Util;

namespace Graphics
{
   public interface IResource : IDisposable
   {
   }

   public abstract class ResourceDescriptor
   {
      public ResourceDescriptor()
      {
      }

      public ResourceDescriptor(String resName)
      {
         name = resName;
      }

      public string name { get; protected set; }
      public string type { get; protected set; }
      public string path { get; protected set; }
      public JsonObject descriptor { get; protected set; }

      public abstract IResource create(ResourceManager mgr);
   }

   public class ResourceManager
   {
      Dictionary<String, IResource> myResources;

      public ResourceManager()
      {
         myResources = new Dictionary<String, IResource>();
      }

      public IResource getResource(ResourceDescriptor desc)
      {
         IResource res;
         if (myResources.TryGetValue(desc.name, out res))
         {
            return res;
         }

         //need to try and load the IResource here
         res = load(desc);

         return res;
      }

      public void release(IResource res)
      {
         res.Dispose();
         myResources.Remove(resourceName(res));
      }

      public void releaseAllResources(Type resourceType)
      {
         List<IResource> toRemove = new List<IResource>();
         foreach (IResource res in myResources.Values)
         {
            if (res.GetType() == resourceType)
            {
               toRemove.Add(res);
            }
         }

         foreach (IResource res in toRemove)
         {
            release(res);
         }
      }

       IResource load(ResourceDescriptor desc)
      {
         IResource res;

         res = desc.create(this);

         if (res != null)
         {
            myResources[desc.name] = res;
         }

         return res;
      }

      public String resourceName(IResource res)
      {
         foreach (KeyValuePair<String, IResource> kv in myResources)
         {
            if (kv.Value == res)
            {
               return kv.Key;
            }
         }

         throw new Exception("Cannot find matching resource");
      }
   }
}