project "World Editor"
  language  "C#"
  kind      "ConsoleApp"
  files     { "../src/worldEditor/**.cs", "../src/worldEditor/**.glsl" }
  links     {  "System", "System.Drawing", "System.Windows.Forms", "OpenTK", "Terrain", "TerrainEditor", "Util", "Lua", "Noise", "UI", "Graphics", "GpuNoise" }
  location "worldEditor"
  vpaths { ["*"] = "../src/worldEditor" }
 