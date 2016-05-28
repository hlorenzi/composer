namespace Composer.Util
{
    public class Key
    {
        public RelativePitch tonicPitch;
        public Scale scale;


        public Key(RelativePitch tonicPitch, Scale scale)
        {
            this.tonicPitch = tonicPitch;
            this.scale = scale;
        }


        public bool HasPitch(RelativePitch relativePitch)
        {
            return
                Util.ScaleData.HasRelativePitch(
                    this.scale,
                    Util.RelativePitchData.DisplaceBy(relativePitch, this.tonicPitch));
        }
    }
}
