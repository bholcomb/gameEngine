project "TerrainEditor"
  language  "C#"
  kind      "SharedLib"
  files     { "../src/terrainEditor/**.cs" }
  links     { "System", "System.Drawing", "OpenTK", "Util", "Terrain", "Graphics", "Engine", "UI", "Physics", "GpuNoise" }
  location "terrainEditor"
  vpaths { ["*"] = "../src/terrainEditor" }
 