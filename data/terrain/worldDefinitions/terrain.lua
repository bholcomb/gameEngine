terrain = {
   generator = {
      heat = {
         output = "Heat Combiner";
         size = {
            1024.0,
            1024.0
         };
         nodes = {
            {
               H = 1.0;
               name = "Heat Fractal";
               offset = 0.0;
               octaves = 4.0;
               lacunarity = 2.0;
               frequency = 3.0;
               type = "Fractal2d";
               gain = 1.0;
               method = "fBm";
            },
            {
               type = "AutoCorrect";
               source = "Heat Fractal";
               name = "Heat AutoCorrect";
            },
            {
               y1 = 0.53591471910477;
               name = "South Gradient";
               y0 = 0.0;
               type = "Gradient";
               x1 = 0.0;
               x0 = 0.0;
            },
            {
               y1 = 0.5050505399704;
               name = "North Gradient";
               y0 = 1.0;
               type = "Gradient";
               x1 = 0.0;
               x0 = 0.0;
            },
            {
               action = "Multiply";
               type = "Combiner";
               inputs = {
                  "Heat AutoCorrect",
                  "South Gradient",
                  "North Gradient"
               };
               name = "Heat Combiner";
            }
         };
      };
      moisture = {
         output = "Moisture AutoCorrect";
         size = {
            1024.0,
            1024.0
         };
         nodes = {
            {
               H = 0.99562293291092;
               name = "Moisture Fractal";
               offset = -0.0011222958564758;
               octaves = 2.0;
               lacunarity = 1.9988777637482;
               frequency = 5.8499999046326;
               type = "Fractal2d";
               gain = 0.99562293291092;
               method = "multiFractal";
            },
            {
               type = "AutoCorrect";
               source = "Moisture Fractal";
               name = "Moisture AutoCorrect";
            }
         };
      };
      elevation = {
         output = "Elevation AutoCorrect";
         size = {
            1024.0,
            1024.0
         };
         nodes = {
            {
               H = 0.7823793888092;
               name = "Elevation Fractal";
               offset = 0.29629635810852;
               octaves = 10.0;
               lacunarity = 2.4029181003571;
               frequency = 2.0444443225861;
               type = "Fractal2d";
               gain = 1.0169472694397;
               method = "multiFractal";
            },
            {
               type = "AutoCorrect";
               source = "Elevation Fractal";
               name = "Elevation AutoCorrect";
            }
         };
      };
   };
   world = {
      seed = 101475.0;
      size = {
         1024.0,
         1024.0
      };
   };
}