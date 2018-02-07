project "Particle Editor"
  language  "C#"
  kind      "WindowedApp"
  files     { "../src/particleEditor/**.cs" }
  links     {  "System", "System.Windows", "System.Windows.Forms", "System.Data", "System.Data.Linq", "System.Drawing", "OpenTK", "OpenTK.GLControl", "Util", "Lua", "Graphics" }
  location "../src/particleEditor"
  vpaths { ["*"] = "../src/particleEditor" }
 