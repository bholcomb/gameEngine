using System;

namespace Noise
{
    public sealed class Select : ModuleBase
    {
        public Select(ModuleBase source, Double low = 0.00, Double high = 0.00, Double falloff = 0.00, Double threshold = 0.00)
        {
            this.Source = source;
            this.Low = new Constant(low);
            this.High = new Constant(high);
            this.Falloff = new Constant(falloff);
            this.Threshold = new Constant(threshold);
        }

        public ModuleBase Source { get; set; }

        public ModuleBase Low { get; set; }

        public ModuleBase High { get; set; }

        public ModuleBase Threshold { get; set; }

        public ModuleBase Falloff { get; set; }

        public override Double Get(Double x, Double y)
        {
            var value = this.Source.Get(x, y);
            var falloff = this.Falloff.Get(x, y);
            var threshold = this.Threshold.Get(x, y);

            if (falloff > 0.0)
            {
                if (value < (threshold - falloff))
                {
                    // Lies outside of falloff area below threshold, return first source
                    return this.Low.Get(x, y);
                }
                if (value > (threshold + falloff))
                {
                    // Lies outside of falloff area above threshold, return second source
                    return this.High.Get(x, y);
                }
                // Lies within falloff area.
                var lower = threshold - falloff;
                var upper = threshold + falloff;
                var blend = Utilities.QuinticBlend((value - lower) / (upper - lower));
                return Utilities.Lerp(blend, this.Low.Get(x, y), this.High.Get(x, y));
            }

            return (value < threshold ? this.Low.Get(x, y) : this.High.Get(x, y));
        }

        public override Double Get(Double x, Double y, Double z)
        {
            var value = this.Source.Get(x, y, z);
            var falloff = this.Falloff.Get(x, y, z);
            var threshold = this.Threshold.Get(x, y, z);

            if (falloff > 0.0)
            {
                if (value < (threshold - falloff))
                {
                    // Lies outside of falloff area below threshold, return first source
                    return this.Low.Get(x, y, z);
                }
                if (value > (threshold + falloff))
                {
                    // Lies outside of falloff area above threshold, return second source
                    return this.High.Get(x, y, z);
                }
                // Lies within falloff area.
                var lower = threshold - falloff;
                var upper = threshold + falloff;
                var blend = Utilities.QuinticBlend((value - lower) / (upper - lower));
                return Utilities.Lerp(blend, this.Low.Get(x, y, z), this.High.Get(x, y, z));
            }

            return (value < threshold ? this.Low.Get(x, y, z) : this.High.Get(x, y, z));
        }

        public override Double Get(Double x, Double y, Double z, Double w)
        {
            var value = this.Source.Get(x, y, z, w);
            var falloff = this.Falloff.Get(x, y, z, w);
            var threshold = this.Threshold.Get(x, y, z, w);

            if (falloff > 0.0)
            {
                if (value < (threshold - falloff))
                {
                    // Lies outside of falloff area below threshold, return first source
                    return this.Low.Get(x, y, z, w);
                }
                if (value > (threshold + falloff))
                {
                    // Lise outside of falloff area above threshold, return second source
                    return this.High.Get(x, y, z, w);
                }
                // Lies within falloff area.
                var lower = threshold - falloff;
                var upper = threshold + falloff;
                var blend = Utilities.QuinticBlend((value - lower) / (upper - lower));
                return Utilities.Lerp(blend, this.Low.Get(x, y, z, w), this.High.Get(x, y, z, w));
            }

            return value < threshold ? this.Low.Get(x, y, z, w) : this.High.Get(x, y, z, w);
        }

        public override Double Get(Double x, Double y, Double z, Double w, Double u, Double v)
        {
            var value = this.Source.Get(x, y, z, w, u, v);
            var falloff = this.Falloff.Get(x, y, z, w, u, v);
            var threshold = this.Threshold.Get(x, y, z, w, u, v);

            if (falloff > 0.0)
            {
                if (value < (threshold - falloff))
                {
                    // Lies outside of falloff area below threshold, return first source
                    return this.Low.Get(x, y, z, w, u, v);
                }
                if (value > (threshold + falloff))
                {
                    // Lies outside of falloff area above threshold, return second source
                    return this.High.Get(x, y, z, w, u, v);
                }
                // Lies within falloff area.
                var lower = threshold - falloff;
                var upper = threshold + falloff;
                var blend = Utilities.QuinticBlend((value - lower) / (upper - lower));
                return Utilities.Lerp(blend, this.Low.Get(x, y, z, w, u, v), this.High.Get(x, y, z, w, u, v));
            }

            return (value < threshold ? this.Low.Get(x, y, z, w, u, v) : this.High.Get(x, y, z, w, u, v));
        }
    }

}
