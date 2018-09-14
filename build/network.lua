project "Network"
  language  "C#"
  kind      "SharedLib"
  files     { "../src/network/**.cs" }
  links     { "System", "OpenTK", "Util", "Engine" }
  location "network"
  vpaths { ["*"] = "../src/network" }
