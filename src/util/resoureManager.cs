using System;
using System.Collections.Generic;

namespace Util
{
   public static class ResourceManager
   {
      static Dictionary<String, IResource> myResources;

      static ResourceManager()
      {
         myResources = new Dictionary<String, IResource>();
      }

      public static bool init()
      {
         return true;
      }

      public static IResource getResource(ResourceDescriptor desc)
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

      public static String resourceName(IResource res)
      {
         foreach( KeyValuePair<String, IResource> kv in myResources)
         {
            if (kv.Value == res)
            {
               return kv.Key;
            }
         }

         throw new Exception("Cannot find matching resource");
      }

      public static void release(IResource res)
      {
         res.Dispose();
         myResources.Remove(resourceName(res));
      }

      public static void releaseAllResources(Type resourceType)
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

      static IResource load(ResourceDescriptor desc)
      {
         IResource res;

         res = desc.create();

         if (res != null)
         {
            myResources[desc.name] = res;
         }

         return res;
      }
   }
}