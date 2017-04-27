project "Test Graphics"
  language  "C#"
  kind      "ConsoleApp"
  files     { "../src/testGraphics/**.cs" }
  links     { "System", "System.Windows", "System.Windows.Forms", "System.Drawing", "OpenTK", "Graphics", "Util", "UI", "Terrain", "Lua" }
  location "testGraphics"
  vpaths { ["*"] = "../src/testGraphics" }
 