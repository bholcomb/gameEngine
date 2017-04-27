project "Noise"
  language  "C#"
  kind      "SharedLib"
  files     { "../src/noise/**.cs" }
  links     { "System", "System.Drawing", "OpenTK", "Util" }
  location "noise"
  vpaths { ["*"] = "../src/noise" }