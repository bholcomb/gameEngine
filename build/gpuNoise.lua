project "GpuNoise"
  language  "C#"
  kind      "SharedLib"
  files     { "../src/gpuNoise/**.cs", "../src/gpuNoise/**.glsl" }
  links     { "System", "System.Drawing", "OpenTK", "Util", "Graphics"}
  location "gpuNoise"
  vpaths { ["*"] = "../src/gpuNoise" }