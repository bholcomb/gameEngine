using System;

namespace Noise
{
    public sealed class ScaleDomain : ModuleBase
    {
        public ScaleDomain(ModuleBase source, 
            Double xScale = 1.00, Double yScale = 1.00, Double zScale = 1.00,
            Double wScale = 1.00, Double uScale = 1.00, Double vScale = 1.00)
        {
            this.Source = source;
            this.XScale = new Constant(xScale);
            this.YScale = new Constant(yScale);
            this.ZScale = new Constant(zScale);
            this.WScale = new Constant(wScale);
            this.UScale = new Constant(uScale);
            this.VScale = new Constant(vScale);
        }

        public ModuleBase Source { get; set; }

        public ModuleBase XScale { get; set; }

        public ModuleBase YScale { get; set; }

        public ModuleBase ZScale { get; set; }

        public ModuleBase WScale { get; set; }

        public ModuleBase UScale { get; set; }

        public ModuleBase VScale { get; set; }

        public void SetScales(
            Double xScale = 1.00, Double yScale = 1.00, Double zScale = 1.00,
            Double wScale = 1.00, Double uScale = 1.00, Double vScale = 1.00)
        {
            this.XScale = new Constant(xScale);
            this.YScale = new Constant(yScale);
            this.ZScale = new Constant(zScale);
            this.WScale = new Constant(wScale);
            this.UScale = new Constant(uScale);
            this.VScale = new Constant(vScale);
        }

        public override Double Get(Double x, Double y)
        {
            return this.Source.Get(
                x * this.XScale.Get(x, y), 
                y * this.YScale.Get(x, y));
        }

        public override Double Get(Double x, Double y, Double z)
        {
            return this.Source.Get(
                x * this.XScale.Get(x, y, z), 
                y * this.YScale.Get(x, y, z), 
                z * this.ZScale.Get(x, y, z));
        }

        public override Double Get(Double x, Double y, Double z, Double w)
        {
            return this.Source.Get(
                x * this.XScale.Get(x, y, z, w), 
                y * this.YScale.Get(x, y, z, w), 
                z * this.ZScale.Get(x, y, z, w),
                w * this.WScale.Get(x, y, z, w));
        }

        public override Double Get(Double x, Double y, Double z, Double w, Double u, Double v)
        {
            return this.Source.Get(
                x * this.XScale.Get(x, y, z, w, u, v),
                y * this.YScale.Get(x, y, z, w, u, v),
                z * this.ZScale.Get(x, y, z, w, u, v),
                w * this.WScale.Get(x, y, z, w, u, v),
                u * this.UScale.Get(x, y, z, w, u, v),
                v * this.VScale.Get(x, y, z, w, u, v));
        }
    }
}