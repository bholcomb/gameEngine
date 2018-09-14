project "UI"
   language  "C#"
   kind      "SharedLib"
   files     { "../src/ui/**.cs", "../src/ui/**.glsl", "../src/ui/data/**.png"}
   links     { "System", "System.Drawing", "OpenTK", "Util", "Graphics", "Engine" }
   location "ui"
   vpaths { ["*"] = "../src/ui" }
 