using System;

using OpenTK;

namespace Util
{
   public class Icosahedron
   {
      const float tao = 1.61803399f;

      public Vector3[] verts = new Vector3[] { new Vector3( 1, tao, 0 ), new Vector3( -1, tao,0), new Vector3( 1,-tao,0), new Vector3( -1,-tao,0),
                new Vector3( 0,1, tao), new Vector3( 0,-1, tao), new Vector3( 0,1,-tao), new Vector3( 0,-1,-tao),
                new Vector3(tao,0,1), new Vector3( -tao,0,1), new Vector3(tao,0,-1), new Vector3( -tao,0,-1)
         };

      public int[][] faces = new int[][] { new int[] { 0, 1, 4 }, new int[] { 1, 9, 4 }, new int[] { 4, 9, 5 }, new int[] { 5, 9, 3 },
                               new int[] { 2, 3, 7 }, new int[] { 3, 2, 5 }, new int[] { 7, 10, 2 }, new int[] { 0, 8, 10 },
                               new int[] { 0, 4, 8 }, new int[] { 8, 2, 10 }, new int[] { 8, 4, 5 }, new int[] { 8, 5, 2 },
                               new int[] { 1, 0, 6 }, new int[] { 11, 1, 6 }, new int[] { 3, 9, 11 }, new int[] { 6, 10, 7 },
                               new int[] { 3, 11, 7 }, new int[] { 11, 6, 7 }, new int[] { 6, 0, 10 }, new int[] { 9, 1, 11 } };
      public Icosahedron(float size = 1.0f)
      {
         for (int i = 0; i < 12; i++)
         {
            verts[i] = verts[i] * size;
         }
      }
   }
}


