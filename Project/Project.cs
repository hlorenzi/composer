using System.Collections.Generic;
using System.Linq;


namespace Composer.Project
{
    public class Project
    {
        float length;
        public Util.SortedList<SectionBreak> sectionBreaks;
        public Util.SortedList<MeterChange> meterChanges;
        public List<Track> tracks;


        public Project()
        {
            this.length = this.WholeNoteDuration * 4;
            this.sectionBreaks = new Util.SortedList<SectionBreak>((a, b) => a.time.CompareTo(b.time));
            this.meterChanges = new Util.SortedList<MeterChange>((a, b) => a.time.CompareTo(b.time));
            this.tracks = new List<Track>();

            this.InsertMeterChange(new MeterChange(0, new Util.Meter(4, 4)));
        }


        public float WholeNoteDuration
        {
            get { return 960; }
        }


        public float Length
        {
            get { return this.length; }
        }


        public void InsertSectionBreak(SectionBreak newSectionBreak)
        {
            if (newSectionBreak.time <= 0 || newSectionBreak.time >= this.length)
                return;

            this.sectionBreaks.RemoveAll(sb => sb.time == newSectionBreak.time);
            this.sectionBreaks.Add(newSectionBreak);
        }


        public void InsertMeterChange(MeterChange newMeterChange)
        {
            if (newMeterChange.time <= 0 || newMeterChange.time >= this.length)
                return;

            this.meterChanges.RemoveAll(mc => mc.time == newMeterChange.time);
            this.meterChanges.Add(newMeterChange);
        }


        public void MoveSectionBreak(SectionBreak sectionBreak, float timeOffset)
        {
            this.sectionBreaks.Remove(sectionBreak);
            sectionBreak.time += timeOffset;
            this.InsertSectionBreak(sectionBreak);
        }


        public void MoveMeterChange(MeterChange meterChange, float timeOffset)
        {
            this.meterChanges.Remove(meterChange);
            meterChange.time += timeOffset;
            this.InsertMeterChange(meterChange);
        }


        public void InsertEmptySpace(float startTime, float duration)
        {
            if (startTime < 0 || duration <= 0)
                return;

            this.length += duration;

            foreach (var sectionBreak in this.sectionBreaks.Clone())
            {
                if (sectionBreak.time >= startTime)
                    this.MoveSectionBreak(sectionBreak, duration);
            }

            foreach (var meterChange in this.meterChanges.Clone())
            {
                if (meterChange.time >= startTime)
                    this.MoveMeterChange(meterChange, duration);
            }

            foreach (var track in this.tracks)
                track.InsertEmptySpace(startTime, duration);
        }


        public void CutRange(Util.TimeRange timeRange)
        {
            foreach (var track in this.tracks)
                track.CutRange(timeRange);

            this.sectionBreaks.RemoveAll(sb => timeRange.Overlaps(sb.time));
            foreach (var sectionBreak in this.sectionBreaks.Clone())
            {
                if (sectionBreak.time >= timeRange.Start)
                    this.MoveSectionBreak(sectionBreak, -timeRange.Duration);
            }

            this.meterChanges.RemoveAll(mc => timeRange.Overlaps(mc.time));
            foreach (var meterChange in this.meterChanges.Clone())
            {
                if (meterChange.time >= timeRange.Start)
                    this.MoveMeterChange(meterChange, -timeRange.Duration);
            }

            this.length -= timeRange.Duration;
        }


        public void InsertSection(float atTime, float defaultDuration)
        {
            if (atTime < 0 || defaultDuration <= 0)
                return;

            this.InsertEmptySpace(atTime, defaultDuration);
            this.InsertSectionBreak(new SectionBreak(atTime));
            this.InsertSectionBreak(new SectionBreak(atTime + defaultDuration));
        }
    }
}
