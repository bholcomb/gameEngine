project "Test Planet"
  language  "C#"
  kind      "ConsoleApp"
  files     { "../src/testPlanet/**.cs" }
  links     { "System", "System.Windows", "System.Windows.Forms", "System.Drawing", "OpenTK", "Graphics", "Util", "UI", "GpuNoise"}
  location "testPlanet"
  vpaths { ["*"] = "../src/testPlanet" }
 