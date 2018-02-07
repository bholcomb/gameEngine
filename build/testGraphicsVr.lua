project "Test Graphics-VR"
  language  "C#"
  kind      "ConsoleApp"
  files     { "../src/testGraphicsVr/**.cs" }
  links     { "System", "System.Windows", "System.Windows.Forms", "System.Drawing", "OpenTK", "Graphics", "Util", "UI", "Terrain", "Lua", "VR" }
  location "testGraphics"
  vpaths { ["*"] = "../src/testGraphicsVr" }
 