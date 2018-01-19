solution "GameEngine"
   location("../")
   configurations { "Debug", "Release" }
   platforms{"x32", "x64"}
   targetdir "../bin"
   debugdir "../bin"
   startproject "GameEngine"
   dotnetframework ("4.6.1")
   
   configuration { "**.glsl" }
      buildaction "Embed"
      
   configuration { "**.png" }
      buildaction "Embed"
 
  configuration { "Debug" }
    defines { "DEBUG", "TRACE" }
    symbols "On"
    optimize "Off"
 
  configuration { "Release" }
    optimize "Speed"
    
--helper functions for code generation    
include("generateMessages.lua")
    
group "Libs"
include("audio.lua")
include("engine.lua")
include("events.lua")
include("network.lua")
include("gpuNoise.lua")
include("graphics.lua")
include("noise.lua")
include("sim.lua")
include("terrain.lua")
include("terrainEditor.lua")
include("ui.lua")
include("util.lua")
include("physics.lua")
include("lua.lua")

group "Tools"
include("particleEditor.lua")
include("messageCodeGen.lua")
include("worldEditor.lua")

group "Apps"
include("terrainServer.lua")

group "Tests"
include("testTerrain.lua")
include("testGraphics.lua")
include("testLua.lua")
include("testUi.lua")
include("testNoise.lua")
include("testPlanet.lua")
