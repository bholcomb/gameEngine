project "TerrainEditor"
  language  "C#"
  kind      "SharedLib"
  files     { "../src/terrainEditor/**.cs" }
  links     { "System", "System.Drawing", "OpenTK", "Util", "Terrain", "Graphics", "Events", "UI", "Physics", "Noise" }
  location "terrainEditor"
  vpaths { ["*"] = "../src/terrainEditor" }
 