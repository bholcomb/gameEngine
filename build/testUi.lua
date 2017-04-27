project "Test UI"
  language  "C#"
  kind      "ConsoleApp"
  files     { "../src/testUi/**.cs" }
  links     { "System", "System.Windows", "System.Windows.Forms", "System.Drawing", "OpenTK", "Graphics", "Util", "UI"}
  location "testUi"
  vpaths { ["*"] = "../src/testUi" }
 