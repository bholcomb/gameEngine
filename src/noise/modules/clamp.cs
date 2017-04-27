using System;

namespace Noise
{
    public sealed class Clamp : ModuleBase
    {
        public Clamp(ModuleBase source, Double low = 0.00, Double high = 1.00)
        {
            this.Source = source;
            this.Low = new Constant(low);
            this.High = new Constant(high);
        }

        public ModuleBase Source { get; set; }

        public ModuleBase Low { get; set; }

        public ModuleBase High { get; set; }

        public override Double Get(Double x, Double y)
        {
            return Utilities.Clamp(Source.Get(x, y), Low.Get(x, y), High.Get(x, y));
        }

        public override Double Get(Double x, Double y, Double z)
        {
            return Utilities.Clamp(Source.Get(x, y, z), Low.Get(x, y, z), High.Get(x, y, z));
        }

        public override Double Get(Double x, Double y, Double z, Double w)
        {
            return Utilities.Clamp(Source.Get(x, y, z, w), Low.Get(x, y, z, w), High.Get(x, y, z, w));
        }

        public override Double Get(Double x, Double y, Double z, Double w, Double u, Double v)
        {
            return Utilities.Clamp(Source.Get(x, y, z, w, u, v), Low.Get(x, y, z, w, u, v), High.Get(x, y, z, w, u, v));
        }
    }
}
