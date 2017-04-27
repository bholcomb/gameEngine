project "Test Lua"
  language  "C#"
  kind      "ConsoleApp"
  files     { "../src/testLua/**.cs" }
  links     { "System", "Lua"}
  location "testLua"
  vpaths { ["*"] = "../src/testLua" }
 