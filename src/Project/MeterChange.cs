namespace Composer.Project
{
    public class MeterChange
    {
        public float time;
        public Util.Meter meter;


        public MeterChange(float time, Util.Meter newMeter)
        {
            this.time = time;
            this.meter = newMeter;
        }


        public string GetDisplayString()
        {
            return this.meter.numerator + " / " + this.meter.denominator;
        }
    }
}
