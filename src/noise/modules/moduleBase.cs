using System;

namespace Noise
{
    public abstract class ModuleBase
    {
        public virtual Int32 Seed { get; set; }

        public virtual Double Get(Double x, Double y) { return 0.00; }

        public virtual Double Get(Double x, Double y, Double z) { return 0.00; }

        public virtual Double Get(Double x, Double y, Double z, Double w) { return 0.00; }

        public virtual Double Get(Double x, Double y, Double z, Double w, Double u, Double v) { return 0.00; }

        public static implicit operator ModuleBase(Double value)
        {
            return new Constant(value);
        }
    }
}
