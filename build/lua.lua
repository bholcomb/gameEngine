project "Lua"
  language  "C#"
  kind      "SharedLib"
  files     { "../src/lua/**.cs" }
  links     { "System", "OpenTK"}
  location "lua"
  vpaths { ["*"] = "../src/lua" }