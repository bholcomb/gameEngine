using System;

namespace Noise
{
    public sealed class Invert : ModuleBase
    {
        public Invert(ModuleBase source)
        {
            this.Source = source;
        }
        
        public ModuleBase Source { set; get; }

        public override Double Get(Double x, Double y)
        {
            return -this.Source.Get(x, y);
        }

        public override Double Get(Double x, Double y, Double z)
        {
            return -this.Source.Get(x, y, z);
        }

        public override Double Get(Double x, Double y, Double z, Double w)
        {
            return -this.Source.Get(x, y, z, w);
        }

        public override Double Get(Double x, Double y, Double z, Double w, Double u, Double v)
        {
            return -this.Source.Get(x, y, z, w, u, v);
        }
    }
}
