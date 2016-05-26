namespace Composer.Project
{
    public class MeterChange
    {
        public float time;
        public Util.Meter newMeter;


        public MeterChange(float time, Util.Meter newMeter)
        {
            this.time = time;
            this.newMeter = newMeter;
        }
    }
}
