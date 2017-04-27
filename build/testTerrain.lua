project "Test Terrain"
  language  "C#"
  kind      "ConsoleApp"
  files     { "../src/testTerrain/**.cs" }
  links     { "System", "System.Windows", "System.Windows.Forms", "System.Drawing", "OpenTK", "Graphics", "Util", "UI", "Terrain", "TerrainEditor", "Lua" }
  location "testTerrain"
  vpaths { ["*"] = "../src/testTerrain" }
 