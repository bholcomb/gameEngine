using System;

namespace Noise
{
    public sealed class Constant : ModuleBase
    {
        public Constant()
        {
            this.Value = 0.00;
        }

        public Constant(Double value)
        {
            this.Value = value;
        }

        public Double Value { get;  set; }

        public override Double Get(Double x, Double y)
        {
            return this.Value;
        }

        public override Double Get(Double x, Double y, Double z)
        {
            return this.Value;
        }

        public override Double Get(Double x, Double y, Double z, Double w)
        {
            return this.Value;
        }

        public override Double Get(Double x, Double y, Double z, Double w, Double u, Double v)
        {
            return this.Value;
        }
    }
}
