using System;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using Util;

namespace Graphics
{ 
   public class SimpleModel
   {
      public static Model createCube(Vector3 min, Vector3 max, Texture t)
      {
         Model model= new Model();

         ushort[] index ={0, 1, 2,
                        0, 2, 3, //front
                        4, 5, 6,
                        4,6, 7, //left
                        8,9,10,
                        8,10,11, //right
                        12,13,14,
                        12,14,15, // top 
                        16,17,18,
                        16,18,19, // bottom
                        20,21,22,
                        20,22,23  // back
                        };

         V3N3T2[] verts = new V3N3T2[24];
         //front face
         verts[0].Position = new Vector3(max.X, min.Y, min.Z); verts[0].TexCoord = new Vector2(0, 0);
         verts[1].Position = new Vector3(min.X, min.Y, min.Z); verts[1].TexCoord = new Vector2(1, 0);
         verts[2].Position = new Vector3(min.X, max.Y, min.Z); verts[2].TexCoord = new Vector2(1, 1);
         verts[3].Position = new Vector3(max.X, max.Y, min.Z); verts[3].TexCoord = new Vector2(0, 1);

         //left
         verts[4].Position = new Vector3(min.X, min.Y, min.Z); verts[4].TexCoord = new Vector2(0, 0);
         verts[5].Position = new Vector3(min.X, min.Y, max.Z); verts[5].TexCoord = new Vector2(1, 0);
         verts[6].Position = new Vector3(min.X, max.Y, max.Z); verts[6].TexCoord = new Vector2(1, 1);
         verts[7].Position = new Vector3(min.X, max.Y, min.Z); verts[7].TexCoord = new Vector2(0, 1);

         //right
         verts[8].Position = new Vector3(max.X, min.Y, max.Z); verts[8].TexCoord = new Vector2(0, 0); 
         verts[9].Position = new Vector3(max.X, min.Y, min.Z); verts[9].TexCoord = new Vector2(1, 0); 
         verts[10].Position = new Vector3(max.X, max.Y, min.Z); verts[10].TexCoord = new Vector2(1, 1);
         verts[11].Position = new Vector3(max.X, max.Y, max.Z); verts[11].TexCoord = new Vector2(0, 1);

         //top
         verts[12].Position = new Vector3(min.X, max.Y, max.Z); verts[12].TexCoord = new Vector2(0, 0);
         verts[13].Position = new Vector3(max.X, max.Y, max.Z); verts[13].TexCoord = new Vector2(1, 0);
         verts[14].Position = new Vector3(max.X, max.Y, min.Z); verts[14].TexCoord = new Vector2(1, 1);
         verts[15].Position = new Vector3(min.X, max.Y, min.Z); verts[15].TexCoord = new Vector2(0, 1);

         //bottom
         verts[16].Position = new Vector3(min.X, min.Y, min.Z); verts[16].TexCoord = new Vector2(0, 0);
         verts[17].Position = new Vector3(max.X, min.Y, min.Z); verts[17].TexCoord = new Vector2(1, 0);
         verts[18].Position = new Vector3(max.X, min.Y, max.Z); verts[18].TexCoord = new Vector2(1, 1);
         verts[19].Position = new Vector3(min.X, min.Y, max.Z); verts[19].TexCoord = new Vector2(0, 1);

         //back
         verts[20].Position = new Vector3(min.X, min.Y, max.Z); verts[20].TexCoord = new Vector2(0, 0);
         verts[21].Position = new Vector3(max.X, min.Y, max.Z); verts[21].TexCoord = new Vector2(1, 0);
         verts[22].Position = new Vector3(max.X, max.Y, max.Z); verts[22].TexCoord = new Vector2(1, 1);
         verts[23].Position = new Vector3(min.X, max.Y, max.Z); verts[23].TexCoord = new Vector2(0, 1);

         model.myBindings = V3N3T2.bindings();
         VertexBufferObject vbo = new VertexBufferObject(BufferUsageHint.StaticDraw);
         vbo.setData(verts);
         model.myVbos.Add(vbo);
         model.myIbo.setData(index);

         Mesh mesh = new Mesh();
         mesh.primativeType = PrimitiveType.Triangles;
         mesh.indexBase = 0;
         mesh.indexCount = 36;
         mesh.material = new Material("simple cube");
         mesh.material.addAttribute(new TextureAttribute("diffuseMap", t));
         mesh.material.myFeatures |= Material.Feature.DiffuseMap;
         mesh.material.hasTransparency = t.hasAlpha;

         model.myMeshes.Add(mesh);
         return model;
      }

      public static Model CreatePlane(Vector3 min, Vector3 max, Texture t)
      {
         Model model = new Model();

         ushort[] index ={0, 1, 2,
                        0, 2, 3, //front
                        };

         V3N3T2[] verts = new V3N3T2[4];

         //face
         verts[0].Position = new Vector3(max.X, min.Y, min.Z); verts[0].TexCoord = new Vector2(0, 0);
         verts[1].Position = new Vector3(min.X, min.Y, min.Z); verts[1].TexCoord = new Vector2(1, 0);
         verts[2].Position = new Vector3(min.X, max.Y, min.Z); verts[2].TexCoord = new Vector2(1, 1);
         verts[3].Position = new Vector3(max.X, max.Y, min.Z); verts[3].TexCoord = new Vector2(0, 1);

         model.myBindings = V3N3T2.bindings();
         VertexBufferObject vbo = new VertexBufferObject(BufferUsageHint.StaticDraw);
         vbo.setData(verts);
         model.myVbos.Add(vbo);
         model.myIbo.setData(index);

         Mesh mesh = new Mesh();
         mesh.primativeType = PrimitiveType.Triangles;
         mesh.indexBase = 0;
         mesh.indexCount = 6;
         mesh.material = new Material("simple plane");
         mesh.material.addAttribute(new TextureAttribute("diffuseMap", t));
         mesh.material.myFeatures |= Material.Feature.DiffuseMap;
         mesh.material.hasTransparency = t.hasAlpha;

         model.myMeshes.Add(mesh);
         return model;
      }
   }
}