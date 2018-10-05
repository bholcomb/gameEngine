renderer={
        shaderManager ={
            shaderComponents="../data/shaders";
        };
        materialManager={};
        lightManager={};
}


terrain={
   cachePath="../data/chunks/world.chunk";
	terrainDefinition="../data/terrain.json";
	biomeDefinition="../data/biome.json";
	source="remote";
	address="127.0.0.1";
	port=2377;
}
