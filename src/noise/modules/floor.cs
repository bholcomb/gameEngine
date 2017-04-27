using System;

namespace Noise
{
    public sealed class Floor : ModuleBase
    {
        public Floor()
        {
            this.Source = new Constant(0.00);
        }

        public Floor(ModuleBase source)
        {
            this.Source = source;
        }

        public ModuleBase Source { get; set; }

        public override Double Get(Double x, Double y)
        {
            return Math.Floor(this.Source.Get(x, y));
        }

        public override Double Get(Double x, Double y, Double z)
        {
            return Math.Floor(this.Source.Get(x, y, z));
        }

        public override Double Get(Double x, Double y, Double z, Double w)
        {
            return Math.Floor(this.Source.Get(x, y, z, w));
        }

        public override Double Get(Double x, Double y, Double z, Double w, Double u, Double v)
        {
            return Math.Floor(this.Source.Get(x, y, z, w, u, v));
        }
    }
}
