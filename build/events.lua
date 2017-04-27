project "Events"
  generateMessages("../src/events")
  language  "C#"
  kind      "SharedLib"
  files     { "../src/events/**.cs", "../src/events/**.event" }
  links     { "System", "OpenTK", "Util"}
  location "events"
  vpaths { ["*"] = "../src/events" }
 