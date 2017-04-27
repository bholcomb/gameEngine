project "Terrain Server"
  language  "C#"
  kind      "ConsoleApp"
  files     { "../src/terrainServer/**.cs" }
  links     { "System", "OpenTK", "Terrain", "Util", "Engine", "Events", "Network" }
  location "terrainServer"
  vpaths { ["*"] = "../src/terrainServer" }
 