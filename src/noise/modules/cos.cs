using System;

namespace Noise
{
    public sealed class Cos : ModuleBase
    {
        public Cos(ModuleBase source)
        {
            this.Source = source;
        }

        public ModuleBase Source { get; set; }

        public override Double Get(Double x, Double y)
        {
            return Math.Cos(this.Source.Get(x, y));
        }

        public override Double Get(Double x, Double y, Double z)
        {
            return Math.Cos(this.Source.Get(x, y, z));
        }

        public override Double Get(Double x, Double y, Double z, Double w)
        {
            return Math.Cos(this.Source.Get(x, y, z, w));
        }

        public override Double Get(Double x, Double y, Double z, Double w, Double u, Double v)
        {
            return Math.Cos(this.Source.Get(x, y, z, w, u, v));
        }
    }
}
