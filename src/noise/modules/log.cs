using System;

namespace Noise
{
    public sealed class Log : ModuleBase
    {
        public Log(ModuleBase source)
        {
            this.Source = source;
        }
        
        public ModuleBase Source { set; get; }

        public override Double Get(Double x, Double y)
        {
            return Math.Log(this.Source.Get(x, y));
        }

        public override Double Get(Double x, Double y, Double z)
        {
            return Math.Log(this.Source.Get(x, y, z));
        }

        public override Double Get(Double x, Double y, Double z, Double w)
        {
            return Math.Log(this.Source.Get(x, y, z, w));
        }

        public override Double Get(Double x, Double y, Double z, Double w, Double u, Double v)
        {
            return Math.Log(this.Source.Get(x, y, z, w, u, v));
        }
    }
}
