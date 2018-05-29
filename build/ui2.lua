project "UI2"
   language  "C#"
   kind      "SharedLib"
   files     { "../src/ui2/**.cs", "../src/ui2/**.glsl", "../src/ui2/data/**.png"}
   links     { "System", "System.Drawing", "OpenTK", "Util", "Graphics", "Events" }
   location "ui2"
   vpaths { ["*"] = "../src/ui2" }
 