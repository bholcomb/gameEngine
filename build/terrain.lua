project "Terrain"
   generateMessages("../src/terrain/events")
   language  "C#"
   kind      "SharedLib"
   clr "Unsafe"
   files     { "../src/terrain/**.cs", "../src/terrain/**.glsl", "../src/terrain/**.event"  }
   links     { "System", "OpenTK", "Util", "GpuNoise", "Graphics", "Engine", "Network", "Physics", "Lua" }
   location "terrain"
   vpaths { ["*"] = "../src/terrain" }
