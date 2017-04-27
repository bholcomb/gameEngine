project "Test Noise"
  language  "C#"
  kind      "ConsoleApp"
  files     { "../src/testNoise/**.cs" }
  links     { "System", "System.Windows", "System.Windows.Forms", "System.Drawing", "OpenTK", "Graphics", "Util", "UI", "GpuNoise"}
  location "testNoise"
  vpaths { ["*"] = "../src/testNoise" }
 