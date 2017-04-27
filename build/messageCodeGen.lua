project "Message Code Gen"
	kind "ConsoleApp"
	files     { "../src/msgCodeGen/**.lua" }
	location "msgCodeGen"
	configuration("**.lua")
		buildmessage("Creating code generator from %{file.basename}.lua")
		buildcommands {'$(SolutionDir)/src/msgCodeGen/glue.exe $(SolutionDir)/src/msgCodeGen/srlua.exe %{file.abspath} $(SolutionDir)/bin/%{file.basename}.exe'}
		buildoutputs { '$(SolutionDir)/bin/%{file.basename}.exe' }
