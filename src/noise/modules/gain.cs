using System;

namespace Noise
{
    public sealed class Gain : ModuleBase
    {
        public Gain(ModuleBase source, Double gain = 0.00)
        {
            this.Source = source;
            this.gain = new Constant(gain);
        }

        public Gain(ModuleBase source, ModuleBase gain)
        {
            this.Source = source;
            this.gain = gain;
        }

        public ModuleBase Source { get; set; }

        public ModuleBase gain { get; set; }

        public override Double Get(Double x, Double y)
        {
           return Utilities.Gain(this.gain.Get(x, y), this.Source.Get(x, y));
        }

        public override Double Get(Double x, Double y, Double z)
        {
           return Utilities.Gain(this.gain.Get(x, y, z), this.Source.Get(x, y, z));
        }

        public override Double Get(Double x, Double y, Double z, Double w)
        {
           return Utilities.Gain(this.gain.Get(x, y, z, w), this.Source.Get(x, y, z, w));
        }

        public override Double Get(Double x, Double y, Double z, Double w, Double u, Double v)
        {
           return Utilities.Gain(this.gain.Get(x, y, z, w, u, v), this.Source.Get(x, y, z, w, u, v));
        }
    }
}
