using System.Collections.Generic;


namespace Composer.Project
{
    public class Project
    {
        float length;
        public Util.TimeSortedList<SectionBreak> sectionBreaks;
        public Util.TimeSortedList<KeyChange> keyChanges;
        public Util.TimeSortedList<MeterChange> meterChanges;
        public List<Track> tracks;


        public Project(float startingLength)
        {
            this.length = startingLength;
            this.sectionBreaks = new Util.TimeSortedList<SectionBreak>(sb => sb.time);
            this.keyChanges = new Util.TimeSortedList<KeyChange>(kc => kc.time);
            this.meterChanges = new Util.TimeSortedList<MeterChange>(mc => mc.time);
            this.tracks = new List<Track>();
        }


        public float WholeNoteDuration
        {
            get { return 960; }
        }


        public float Length
        {
            get { return this.length; }
        }


        public int GetTrackIndex(Track track)
        {
            return this.tracks.FindIndex(tr => tr == track);
        }


        public void InsertSectionBreak(SectionBreak newSectionBreak)
        {
            if (newSectionBreak.time <= 0 || newSectionBreak.time >= this.length)
                return;

            this.sectionBreaks.RemoveAll(sb => sb.time == newSectionBreak.time);
            this.sectionBreaks.Add(newSectionBreak);
        }


        public void InsertKeyChange(KeyChange newKeyChange)
        {
            if (newKeyChange.time < 0 || newKeyChange.time >= this.length)
                return;

            this.keyChanges.RemoveAll(kc => kc.time == newKeyChange.time);
            this.keyChanges.Add(newKeyChange);
        }


        public void InsertMeterChange(MeterChange newMeterChange)
        {
            if (newMeterChange.time < 0 || newMeterChange.time >= this.length)
                return;

            this.meterChanges.RemoveAll(mc => mc.time == newMeterChange.time);
            this.meterChanges.Add(newMeterChange);
        }


        public void InsertPitchedNote(int trackIndex, PitchedNote pitchedNote)
        {
            pitchedNote.timeRange.Start = System.Math.Max(
                0,
                pitchedNote.timeRange.Start);

            pitchedNote.timeRange.End = System.Math.Min(
                this.length,
                pitchedNote.timeRange.End);

            if (pitchedNote.timeRange.Duration <= 0)
                return;

            var track = (TrackPitchedNotes)this.tracks[trackIndex];
            track.InsertPitchedNote(pitchedNote);
        }


        public void RemovePitchedNote(int trackIndex, PitchedNote pitchedNote)
        {
            var track = (TrackPitchedNotes)this.tracks[trackIndex];
            track.RemovePitchedNote(pitchedNote);
        }


        public void RemoveSectionBreak(SectionBreak sectionBreak)
        {
            this.sectionBreaks.Remove(sectionBreak);
        }


        public void RemoveKeyChange(KeyChange keyChange)
        {
            this.keyChanges.Remove(keyChange);
        }


        public void RemoveMeterChange(MeterChange meterChange)
        {
            this.meterChanges.Remove(meterChange);
        }


        public void InsertEmptySpace(float startTime, float duration)
        {
            if (startTime < 0 || duration <= 0)
                return;

            this.length += duration;

            foreach (var sectionBreak in this.sectionBreaks.Clone())
            {
                if (sectionBreak.time >= startTime)
                {
                    this.RemoveSectionBreak(sectionBreak);
                    sectionBreak.time += duration;
                    this.InsertSectionBreak(sectionBreak);
                }
            }

            foreach (var keyChange in this.keyChanges.Clone())
            {
                if (keyChange.time >= startTime)
                {
                    this.RemoveKeyChange(keyChange);
                    keyChange.time += duration;
                    this.InsertKeyChange(keyChange);
                }
            }

            foreach (var meterChange in this.meterChanges.Clone())
            {
                if (meterChange.time >= startTime)
                {
                    this.RemoveMeterChange(meterChange);
                    meterChange.time += duration;
                    this.InsertMeterChange(meterChange);
                }
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
                {
                    this.RemoveSectionBreak(sectionBreak);
                    sectionBreak.time -= timeRange.Duration;
                    this.InsertSectionBreak(sectionBreak);
                }
            }

            this.keyChanges.RemoveAll(kc => timeRange.Overlaps(kc.time));
            foreach (var keyChange in this.keyChanges.Clone())
            {
                if (keyChange.time >= timeRange.Start)
                {
                    this.RemoveKeyChange(keyChange);
                    keyChange.time -= timeRange.Duration;
                    this.InsertKeyChange(keyChange);
                }
            }

            this.meterChanges.RemoveAll(mc => timeRange.Overlaps(mc.time));
            foreach (var meterChange in this.meterChanges.Clone())
            {
                if (meterChange.time >= timeRange.Start)
                {
                    this.RemoveMeterChange(meterChange);
                    meterChange.time -= timeRange.Duration;
                    this.InsertMeterChange(meterChange);
                }
            }

            this.length -= timeRange.Duration;
        }


        public void InsertSection(float atTime, float duration)
        {
            if (atTime < 0 || duration <= 0)
                return;

            this.InsertEmptySpace(atTime, duration);
            this.InsertSectionBreak(new SectionBreak(atTime));
            this.InsertSectionBreak(new SectionBreak(atTime + duration));
        }
    }
}
