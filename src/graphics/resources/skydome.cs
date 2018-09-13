using System;
using System.IO;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using Util;

namespace Graphics
{
   public class SkydomeDescriptor : ResourceDescriptor
   {
      public SkydomeDescriptor(string descriptorFilename)
         : base()
      {
         type = "skydome";
         path = Path.GetDirectoryName(descriptorFilename);
         JsonObject definition = JsonObject.loadFile(descriptorFilename);
         descriptor = definition["skydome"];
         name = (string)descriptor["name"];
      }

      public override IResource create(ResourceManager mgr)
      {
         ObjModelDescriptor desc = new ObjModelDescriptor(Path.Combine(path, (string)descriptor["mesh"]));
         Model dome = Renderer.resourceManager.getResource(desc) as Model;
         
         dome.myMeshes[0].material = new Material(name);
         dome.myMeshes[0].material.myFeatures |= Material.Feature.Skydome;

         TextureDescriptor td = new TextureDescriptor(Path.Combine(path, (string)descriptor["sun"]), true);
         Texture t = Renderer.resourceManager.getResource(td) as Texture;
         dome.myMeshes[0].material.addAttribute(new TextureAttribute("sun", t));

         td = new TextureDescriptor(Path.Combine(path, (string)descriptor["tint1"]), true);
         t = Renderer.resourceManager.getResource(td) as Texture;
         dome.myMeshes[0].material.addAttribute(new TextureAttribute("tint1", t));

         td = new TextureDescriptor(Path.Combine(path, (string)descriptor["tint2"]), true);
         t = Renderer.resourceManager.getResource(td) as Texture;
         dome.myMeshes[0].material.addAttribute(new TextureAttribute("tint2", t));

         td = new TextureDescriptor(Path.Combine(path, (string)descriptor["clouds1"]), true);
         t = Renderer.resourceManager.getResource(td) as Texture;
         dome.myMeshes[0].material.addAttribute(new TextureAttribute("clouds1", t));

         td = new TextureDescriptor(Path.Combine(path, (string)descriptor["clouds2"]), true);
         t = Renderer.resourceManager.getResource(td) as Texture;
         dome.myMeshes[0].material.addAttribute(new TextureAttribute("clouds2", t));

         td = new TextureDescriptor(Path.Combine(path, (string)descriptor["moon"]), true);
         t = Renderer.resourceManager.getResource(td) as Texture;
         dome.myMeshes[0].material.addAttribute(new TextureAttribute("moon", t));

         return dome;
      }
   }
}
