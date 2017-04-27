using System;

namespace Util
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

      public abstract IResource create();
   }
}