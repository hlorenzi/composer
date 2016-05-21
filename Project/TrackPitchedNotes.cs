namespace Composer.Project
{
    public class TrackPitchedNotes : Track
    {
        public Util.TimeRangeSortedList<PitchedNote> notes;


        public TrackPitchedNotes()
        {
            this.notes = new Util.TimeRangeSortedList<PitchedNote>(n => n.timeRange);
        }
    }
}
