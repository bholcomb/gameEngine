project "Audio"
  language  "C#"
  kind      "SharedLib"
  files     { "../src/audio/**.cs" }
  links     { "System", "Util", "OpenTK", "NVorbis"}
  location "audio"
  vpaths { ["*"] = "../src/audio" }
