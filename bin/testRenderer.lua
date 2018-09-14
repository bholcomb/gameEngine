renderer={
        shaderManager ={
            shaderComponents="../data/shaders";
        };
        materialManager={};
        lightManager={};
}


terrain={
   cachePath="../data/terrain/chunks/world.chunk";
	terrainDefinition="../data/terrain/terrain.json";
	biomeDefinition="../data/terrain/biome.json";
	source="remote";
	address="127.0.0.1";
	port=2377;
}
