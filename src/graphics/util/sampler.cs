using System;

namespace Graphics
{
   public class Sampler2D
   {

   }  
   
   public class Sampler3D
   {

   }

   public class SamplerCubemap
   {
      /*
      From: https://scalibq.wordpress.com/2013/06/23/cubemaps/
       
       1) The face is selected by looking at the absolute values of the components of the 3d vector (|x|, |y|, |z|). The component with the absolute value of the largest magnitude determines the major axis. The sign of the component selects the positive or negative direction.

      2) The selected face is addressed as a regular 2D texture with U, V coordinates within a (0..1) range. The U and V are calculated from the two components that were not the major axis. So for example, if  we have +x as our cubemap face, then Y and Z will be used to calculate U and V:

      U = ((-Z/|X|) + 1)/2

      V = ((-Y/|X|) + 1)/2 
     */
   }
}