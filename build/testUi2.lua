project "Test UI2"
  language  "C#"
  kind      "ConsoleApp"
  files     { "../src/testUi2/**.cs" }
  links     { "System", "System.Windows", "System.Windows.Forms", "System.Drawing", "OpenTK", "Graphics", "Util", "UI2"}
  location "testUi2"
  vpaths { ["*"] = "../src/testUi2" }
 