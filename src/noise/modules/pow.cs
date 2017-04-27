using System;

namespace Noise
{
    public sealed class Pow : ModuleBase
    {
        public Pow(ModuleBase source, Double power)
        {
            this.Source = source;
            this.Power = new Constant(power);
        }

        public Pow(ModuleBase source, ModuleBase power)
        {
            this.Source = source;
            this.Power = power;
        }

        public ModuleBase Source { get; set; }

        public ModuleBase Power { get; set; }

        public override Double Get(Double x, Double y)
        {
            return Math.Pow(this.Source.Get(x, y), this.Power.Get(x, y));
        }

        public override Double Get(Double x, Double y, Double z)
        {
            return Math.Pow(this.Source.Get(x, y, z), this.Power.Get(x, y, z));
        }

        public override Double Get(Double x, Double y, Double z, Double w)
        {
            return Math.Pow(this.Source.Get(x, y, z, w), this.Power.Get(x, y, z, w));
        }

        public override Double Get(Double x, Double y, Double z, Double w, Double u, Double v)
        {
            return Math.Pow(this.Source.Get(x, y, z, w, u, v), this.Power.Get(x, y, z, w, u, v));
        }
    }
}