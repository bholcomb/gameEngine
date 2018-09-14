using System;
using System.Collections.Generic;

namespace Util
{
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

         if (res == null)
         {
            throw new Exception("Failed to create resource");
         }

         return res;
      }

      public void release(IResource res)
      {
         res.Dispose();
         myResources.Remove(resourceName(res));
      }

      public void releaseAllResources()
      {
         foreach (IResource res in myResources.Values)
         {
            res.Dispose();
         }

         myResources.Clear();
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