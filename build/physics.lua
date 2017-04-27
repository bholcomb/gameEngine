project "Physics"
   generateMessages("../src/physics/events")
   language  "C#"
   kind      "SharedLib"
   flags {"Unsafe"}
   files     { "../src/physics/**.cs", "../src/physics/**.glsl", "../src/physics/**.event"  }
   links     { "System", "OpenTK", "Util", "Events", "Network" }
   location "physics"
   vpaths { ["*"] = "../src/physics" }
