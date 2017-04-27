using System;

using Util;

namespace Sim
{
   public static class SimManager
   {
      static EntityManager myEntityManager;

      static SimManager()
      {

      }

      public static bool init(Initializer init)
      {
         myEntityManager = new EntityManager();
         if (myEntityManager.init(init) == false)
         {
            Error.print("Error initializing entity manager");
            return false;
         }

         return true;
      }

      public static EntityManager entityManager
      {
         get { return myEntityManager; }
      }
   }
}