project "Test IcoPlanet"
  language  "C#"
  kind      "ConsoleApp"
  files     { "../src/testIcoPlanet/**.cs" , "../src/testIcoPlanet/**.glsl" }
  links     { "System", "System.Windows", "System.Windows.Forms", "System.Drawing", "OpenTK", "Graphics", "Util", "UI", "GpuNoise"}
  location "testIcoPlanet"
  vpaths { ["*"] = "../src/testIcoPlanet" }
 