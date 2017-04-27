entity={}
entity.templates={}

function ENTITY(t)
	--applyFunctionMetaTable(t)
   entity.templates[t.type]=t
   print("Registering Entity type: "..t.type)
end
