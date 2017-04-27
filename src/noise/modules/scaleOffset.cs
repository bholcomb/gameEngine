using System;

namespace Noise
{
    public sealed class ScaleOffset : ModuleBase
    {
        public ScaleOffset(ModuleBase source, Double scale = 1.00, Double offset = 0.00)
        {
            this.Source = source;
            this.Scale = new Constant(scale);
            this.Offset = new Constant(offset);
        }

        public ModuleBase Source { get; set; }

        public ModuleBase Scale { get; set; }

        public ModuleBase Offset { get; set; }

        public override Double Get(Double x, Double y)
        {
            return this.Source.Get(x, y) * this.Scale.Get(x, y) + this.Offset.Get(x, y);
        }

        public override Double Get(Double x, Double y, Double z)
        {
            return this.Source.Get(x, y, z) * this.Scale.Get(x, y, z) + this.Offset.Get(x, y, z);
        }

        public override Double Get(Double x, Double y, Double z, Double w)
        {
            return this.Source.Get(x, y, z, w) * this.Scale.Get(x, y, z, w) + this.Offset.Get(x, y, z, w);
        }
            
        public override Double Get(Double x, Double y, Double z, Double w, Double u, Double v)
        {
            return this.Source.Get(x, y, z, w, u, v) * this.Scale.Get(x, y, z, w, u, v) + this.Offset.Get(x, y, z, w, u, v);
        }
    }
}
