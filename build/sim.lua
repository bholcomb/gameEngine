project "Sim"
	generateMessages("../src/sim/events")
	language  "C#"
	kind      "SharedLib"
	files     { "../src/sim/**.cs", "../src/sim/**.event"}
	links     { "System", "OpenTK", "Util", "Engine", "Renderer", "Audio", "Events", "Noise", "Terrain", "UI", "Lua" }
	location "sim"
	vpaths { ["*"] = "../src/sim" }
