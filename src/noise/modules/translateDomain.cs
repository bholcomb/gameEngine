using System;

namespace Noise
{
    public sealed class TranslateDomain : ModuleBase
    {
        public TranslateDomain(
            ModuleBase source,
            Double xAxis = 0.00, Double yAxis = 0.00, Double zAxis = 0.00,
            Double wAxis = 0.00, Double uAxis = 0.00, Double vAxis = 0.00)
        {
            this.Source = source;
            this.XAxis = new Constant(xAxis);
            this.YAxis = new Constant(yAxis);
            this.ZAxis = new Constant(zAxis);
            this.WAxis = new Constant(wAxis);
            this.UAxis = new Constant(uAxis);
            this.VAxis = new Constant(vAxis);
        }

        public ModuleBase Source { get; set; }

        public ModuleBase XAxis { get; set; }

        public ModuleBase YAxis { get; set; }

        public ModuleBase ZAxis { get; set; }

        public ModuleBase WAxis { get; set; }

        public ModuleBase UAxis { get; set; }

        public ModuleBase VAxis { get; set; }

        public override Double Get(Double x, Double y)
        {
            return this.Source.Get(
                x + this.XAxis.Get(x, y),
                y + this.YAxis.Get(x, y));
        }

        public override Double Get(Double x, Double y, Double z)
        {
            return this.Source.Get(
                x + this.XAxis.Get(x, y, z),
                y + this.YAxis.Get(x, y, z),
                z + this.ZAxis.Get(x, y, z));
        }

        public override Double Get(Double x, Double y, Double z, Double w)
        {
            return Source.Get(
                x + this.XAxis.Get(x, y, z, w),
                y + this.YAxis.Get(x, y, z, w),
                z + this.ZAxis.Get(x, y, z, w),
                w + this.WAxis.Get(x, y, z, w));
        }

        public override Double Get(Double x, Double y, Double z, Double w, Double u, Double v)
        {
            return this.Source.Get(
                x + this.XAxis.Get(x, y, z, w, u, v),
                y + this.YAxis.Get(x, y, z, w, u, v),
                z + this.ZAxis.Get(x, y, z, w, u, v),
                w + this.WAxis.Get(x, y, z, w, u, v),
                u + this.UAxis.Get(x, y, z, w, u, v),
                v + this.VAxis.Get(x, y, z, w, u, v));
        }
    }
}