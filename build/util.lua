project "Util"
  language  "C#"
  kind      "SharedLib"
  files     { "../src/util/**.cs" }
  links     { "System", "OpenTK", "Lua" }
  location "util"
  vpaths { ["*"] = "../src/util" }