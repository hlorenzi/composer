namespace Composer.Project
{
    public class KeyChange
    {
        public float time;
        public Util.Key key;


        public KeyChange(float time, Util.Key key)
        {
            this.time = time;
            this.key = key;
        }


        public string GetDisplayString()
        {
            return
                Util.RelativePitchData.GetSimpleName(this.key.tonicPitch) +
                " " +
                Util.ScaleData.GetName(this.key.scale);
        }
    }
}
