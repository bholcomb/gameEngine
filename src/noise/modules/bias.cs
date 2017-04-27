using System;

namespace Noise
{
    public sealed class Bias : ModuleBase
    {
        public Bias(ModuleBase source, Double bias)
        {
            this.Source = source;
            this.bias = new Constant(bias);
        }

        public Bias(ModuleBase source, ModuleBase bias)
        {
            this.Source = source;
            this.bias = bias;
        }

        public ModuleBase Source { get; set; }

        public ModuleBase bias { get; set; }

        public override Double Get(Double x, Double y)
        {
           return Utilities.Bias(this.bias.Get(x, y), this.Source.Get(x, y));
        }

        public override Double Get(Double x, Double y, Double z)
        {
           return Utilities.Bias(this.bias.Get(x, y, z), this.Source.Get(x, y, z));
        }

        public override Double Get(Double x, Double y, Double z, Double w)
        {
           return Utilities.Bias(this.bias.Get(x, y, z, w), this.Source.Get(x, y, z, w));
        }

        public override Double Get(Double x, Double y, Double z, Double w, Double u, Double v)
        {
           return Utilities.Bias(this.bias.Get(x, y, z, w, u, v), this.Source.Get(x, y, z, w, u, v));
        }
    }
}