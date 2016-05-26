using System.Collections.Generic;


namespace Composer.Project
{
    public class TrackPitchedNotes : Track
    {
        public Util.TimeRangeSortedList<PitchedNote> notes;


        public TrackPitchedNotes()
        {
            this.notes = new Util.TimeRangeSortedList<PitchedNote>(n => n.timeRange);
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


        public void SplitNotesAt(float splitTime)
        {
            var newNotes = new List<PitchedNote>();

            foreach (var note in this.notes.EnumerateOverlapping(splitTime))
            {
                note.timeRange.End = splitTime;
                newNotes.Add(new PitchedNote
                {
                    pitch = note.pitch,
                    timeRange = Util.TimeRange.StartEnd(splitTime, note.timeRange.End)
                });
            }

            this.notes.Sort();
            this.notes.AddRange(newNotes);
        }
    }
}
