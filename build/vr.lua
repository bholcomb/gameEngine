project "VR"
   language  "C#"
   kind      "SharedLib"
   files     { "../src/vr/**.cs", "../src/vr/**.glsl", "../src/vr/data/**.png"}
   links     { "System", "System.Drawing", "OpenTK", "Util", "Graphics", "Events" }
   location "vr"
   clr "unsafe"
   vpaths { ["*"] = "../src/vr" }
 