namespace Composer.Util
{
    public struct TimeRange
    {
        float start;
        float end;


        public TimeRange(float start, float end)
        {
            this.start = start;
            this.end = end;
        }


        public static TimeRange Make(float start, float end)
        {
            return new TimeRange { start = start, end = end };
        }


        public float Start
        {
            get { return this.start; }
        }


        public float End
        {
            get { return this.end; }
        }


        public float Duration
        {
            get { return this.end - this.start; }
        }


        public TimeRange OffsetBy(float amount)
        {
            return new TimeRange(this.start + amount, this.end + amount);
        }


        public bool Overlaps(Util.TimeRange other)
        {
            return this.start < other.end && this.end > other.start;
        }
    }
}
