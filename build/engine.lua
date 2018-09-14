project "Engine"
   generateMessages("../src/engine/events")
   language  "C#"
   kind      "SharedLib"
   files     { "../src/engine/**.cs", "../src/engine/**.event"}
   links     { "System", "System.Drawing", "OpenTK", "Util", "Lua"}
   location "engine"
   vpaths { ["*"] = "../src/engine" }