using System.Collections.Generic;
using System.Drawing;


namespace Composer.Editor
{
    class TrackSegmentPitchedNotes : TrackSegment
    {
        public Util.Pitch minPitch, maxPitch;
        public List<Project.TrackPitchedNotes> projectTracks;
        public Util.Rect notesRect;


        public TrackSegmentPitchedNotes(
            ViewManager manager,
            Row row,
            List<Project.TrackPitchedNotes> projectTracks)
            : base(manager, row)
        {
            this.projectTracks = projectTracks;
        }


        public override void Rebuild(float x, float y)
        {
            this.minPitch = Util.Pitch.FromMidiPitch(0);
            this.maxPitch = Util.Pitch.FromMidiPitch(12);

            // Find minimum and maximum note pitches in the segment's time range.
            foreach (var track in this.projectTracks)
            {
                foreach (var note in track.notes.EnumerateInsideRange(this.row.timeRange))
                {
                    if (note.pitch.MidiPitch < this.minPitch.MidiPitch)
                        this.minPitch = note.pitch;

                    if (note.pitch.MidiPitch > this.maxPitch.MidiPitch)
                        this.maxPitch = note.pitch;
                }
            }

            this.layoutRect = new Util.Rect(
                x,
                y,
                x + this.row.timeRange.Duration * this.manager.TimeToPixelsMultiplier,
                y + (this.maxPitch.MidiPitch - this.minPitch.MidiPitch + 1) * this.manager.PitchedNoteHeight + 10);

            this.notesRect = new Util.Rect(
                x,
                y,
                x + this.row.timeRange.Duration * this.manager.TimeToPixelsMultiplier,
                y + (this.maxPitch.MidiPitch - this.minPitch.MidiPitch + 1) * this.manager.PitchedNoteHeight);
        }


        public override float GetTimeAtPosition(float x, float y)
        {
            return this.row.timeRange.Start + (x - this.notesRect.xMin) / this.manager.TimeToPixelsMultiplier;
        }


        public override Util.Pitch GetPitchAtPosition(float x, float y)
        {
            return Util.Pitch.FromMidiPitch(
                System.Math.Max(this.minPitch.MidiPitch,
                System.Math.Min(this.maxPitch.MidiPitch,
                    this.minPitch.MidiPitch + (this.notesRect.yMax - y) / this.manager.PitchedNoteHeight)));
        }


        public override void Draw(Graphics g)
        {
            var endTime = System.Math.Max(
                this.row.timeRange.End,
                this.row.resizeEndTime - this.row.timeRange.Start);

            var endX = (int)(this.layoutRect.xMin + endTime * this.manager.TimeToPixelsMultiplier);

            for (var p = 0; p <= this.maxPitch.MidiPitch - this.minPitch.MidiPitch; p++)
            {
                g.DrawLine(Pens.LightGray,
                    (int)this.notesRect.xMin, (int)(this.notesRect.yMax - (p + 1) * this.manager.PitchedNoteHeight),
                    endX, (int)(this.notesRect.yMax - (p + 1) * this.manager.PitchedNoteHeight));
            }

            g.DrawRectangle(Pens.Black,
                (int)this.notesRect.xMin, (int)this.notesRect.yMin,
                endX - (int)this.notesRect.xMin, (int)this.notesRect.ySize);

            g.DrawLine(Pens.Black,
                (int)this.notesRect.xMax - 1, (int)this.notesRect.yMin,
                (int)this.notesRect.xMax - 1, (int)this.notesRect.yMax);
        }
    }
}
