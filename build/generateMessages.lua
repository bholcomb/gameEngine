--scan a directory for all the files
function findMessageFiles(directory)
   local i, t, popen = 0, {}, io.popen
   for filename in popen('dir "'..directory..'" /b'):lines() do
      if(filename:sub(-6)==".event") then
         i = i + 1
         t[i] = filename
         print("Found: "..filename)
      end
   end

   return t
end

--call the code generator on the messages
function generateMessages(directory)
   local files=findMessageFiles(directory)
   local basePath=directory
   for k,v in pairs(files) do
      local messageFile=basePath..'/'..v
      local cmd=_WORKING_DIR..'/../bin/messageCodeGen.exe -i '..messageFile..' -o '..basePath..'/'
      --print("Executing: "..cmd)
      os.execute(cmd)
   end
end
