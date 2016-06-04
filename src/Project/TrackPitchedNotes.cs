using System.Collections.Generic;


namespace Composer.Project
{
    public class TrackPitchedNotes : Track
    {
        public Util.TimeRangeSortedList<PitchedNote> notes;


        public TrackPitchedNotes(string name)
        {
            this.name = name;
            this.notes = new Util.TimeRangeSortedList<PitchedNote>(n => n.timeRange);
        }


        public void InsertPitchedNote(PitchedNote pitchedNote)
        {
            this.notes.Add(pitchedNote);
        }


        public override void InsertEmptySpace(float startTime, float duration)
        {
            this.SplitNotesAt(startTime);
            foreach (var note in this.notes.EnumerateEntirelyAfter(startTime))
                note.timeRange = note.timeRange.OffsetBy(duration);
        }


        public override void CutRange(Util.TimeRange timeRange)
        {
            this.SplitNotesAt(timeRange.Start);
            this.SplitNotesAt(timeRange.End);
            this.notes.RemoveOverlappingRange(timeRange);
            foreach (var note in this.notes.EnumerateEntirelyAfter(timeRange.End))
                note.timeRange = note.timeRange.OffsetBy(-timeRange.Duration);
        }


        public void SplitNotesAt(float splitTime, Util.Pitch? onlyAtPitch = null)
        {
            var newNotes = new List<PitchedNote>();

            foreach (var note in this.notes.EnumerateOverlapping(splitTime))
            {
                if (onlyAtPitch.HasValue && note.pitch.MidiPitch != onlyAtPitch.Value.MidiPitch)
                    continue;

                newNotes.Add(new PitchedNote
                {
                    pitch = note.pitch,
                    timeRange = Util.TimeRange.StartEnd(splitTime, note.timeRange.End)
                });
                note.timeRange.End = splitTime;
            }

            this.notes.Sort();
            this.notes.AddRange(newNotes);
        }
    }
}
