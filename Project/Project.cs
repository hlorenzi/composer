using System.Collections.Generic;


namespace Composer.Project
{
    public class Project
    {
        float length;
        public Util.SortedList<SegmentBreak> segmentBreaks;
        public Util.SortedList<MeterChange> meterChanges;
        public List<Track> tracks;


        public Project()
        {
            this.length = this.TimeInWholeNote * 4;
            this.segmentBreaks = new Util.SortedList<SegmentBreak>((a, b) => a.time.CompareTo(b.time));
            this.meterChanges = new Util.SortedList<MeterChange>((a, b) => a.time.CompareTo(b.time));
            this.tracks = new List<Track>();

            this.AddMeterChange(new MeterChange(0, new Util.Meter(4, 4)));
        }


        public float TimeInWholeNote
        {
            get { return 960; }
        }


        public float Length
        {
            get { return this.length; }
        }


        public bool SetLength(float newLength)
        {
            if (newLength <= 0)
                return false;

            this.length = newLength;
            return true;
        }


        public bool AddSegmentBreak(SegmentBreak segmentBreak)
        {
            this.segmentBreaks.Add(segmentBreak);
            return true;
        }


        public bool AddMeterChange(MeterChange meterChange)
        {
            this.meterChanges.Add(meterChange);
            return true;
        }
    }
}
