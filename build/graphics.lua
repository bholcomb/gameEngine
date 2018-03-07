project "Graphics" 
   language  "C#"
   kind      "SharedLib"
   files     { "../src/graphics/**.cs", "../src/graphics/**.glsl", "../src/graphics/**.lua" ,"../src/graphics/data/**.png"}
   links     { "System", "System.Drawing", "OpenTK", "Util", "Lua", "AssimpNet" }
   clr "unsafe"
   location "graphics"
   vpaths { ["*"] = "../src/graphics" }
   
   configuration { "**.lua" }
      buildaction "Embed"
