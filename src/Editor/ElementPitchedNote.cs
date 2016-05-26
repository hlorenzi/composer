using System.Collections.Generic;
using System.Drawing;


namespace Composer.Editor
{
    class ElementPitchedNote : Element
    {
        int assignedTrack = -1;
        Project.PitchedNote projectPitchedNote;
        Project.TrackPitchedNotes projectTrackPitchedNode;
        List<Segment> segments;

        Util.TimeRange timeRangeDragStart;
        Util.Pitch pitchDragStart;


        public ElementPitchedNote(
            ViewManager manager,
            Project.TrackPitchedNotes projectTrackPitchedNode,
            Project.PitchedNote pitchedNote)
            : base(manager)
        {
            this.projectTrackPitchedNode = projectTrackPitchedNode;
            this.projectPitchedNote = pitchedNote;
            this.interactableRegions = new List<InteractableRegion>();
            this.segments = new List<Segment>();
        }


        class Segment
        {
            public Util.Rect noteRect;
        }


        public override void AssignTrack()
        {
            this.assignedTrack = -1;
            for (var i = 0; i < this.manager.rows[0].trackSegments.Count; i++)
            {
                var trackPitchedNote = (this.manager.rows[0].trackSegments[i] as TrackSegmentPitchedNotes);
                if (trackPitchedNote != null &&
                    trackPitchedNote.projectTracks.Contains(this.projectTrackPitchedNode))
                {
                    this.assignedTrack = i;
                    break;
                }
            }
        }


        public override void Rebuild()
        {
            this.segments.Clear();
            this.interactableRegions.Clear();

            var tMult = this.manager.TimeToPixelsMultiplier;
            var pMult = this.manager.PitchedNoteHeight;

            foreach (var row in this.manager.EnumerateRowsInTimeRange(this.projectPitchedNote.timeRange))
            {
                var trackPitchedNote = (TrackSegmentPitchedNotes)row.trackSegments[this.assignedTrack];

                if (this.projectPitchedNote.pitch.MidiPitch < trackPitchedNote.minPitch.MidiPitch ||
                    this.projectPitchedNote.pitch.MidiPitch > trackPitchedNote.maxPitch.MidiPitch)
                    continue;

                var midiPitchMinusTrackMin =
                    this.projectPitchedNote.pitch.MidiPitch - trackPitchedNote.minPitch.MidiPitch;

                var startTimeMinusTrackStart = System.Math.Max(
                    0,
                    this.projectPitchedNote.timeRange.Start - trackPitchedNote.row.timeRange.Start);

                var endTimeMinusTrackStart = System.Math.Min(
                    trackPitchedNote.row.timeRange.End,
                    this.projectPitchedNote.timeRange.End) - trackPitchedNote.row.timeRange.Start;

                var noteRect = new Util.Rect(
                    trackPitchedNote.notesRect.xMin + tMult * startTimeMinusTrackStart,
                    trackPitchedNote.notesRect.yMax - pMult * (midiPitchMinusTrackMin + 1),
                    trackPitchedNote.notesRect.xMin + tMult * endTimeMinusTrackStart,
                    trackPitchedNote.notesRect.yMax - pMult * midiPitchMinusTrackMin);

                this.segments.Add(new Segment { noteRect = noteRect });

                this.interactableRegions.Add(
                    new InteractableRegion(InteractableRegion.CursorKind.MoveAll, noteRect));
            }
        }


        public override void DragStart()
        {
            this.timeRangeDragStart = this.projectPitchedNote.timeRange;
            this.pitchDragStart = this.projectPitchedNote.pitch;
        }


        public override void Drag()
        {
            this.projectPitchedNote.timeRange =
                this.timeRangeDragStart.OffsetBy(this.manager.DragTimeOffsetClampedToRow);

            this.projectPitchedNote.pitch =
                this.pitchDragStart.OffsetMidiPitchBy(this.manager.DragMidiPitchOffset);
        }


        public override void Draw(Graphics g, bool hovering, bool selected)
        {
            foreach (var segment in this.segments)
            {
                if (selected)
                {
                    g.FillRectangle(
                        Brushes.Salmon,
                        segment.noteRect.xMin + 1,
                        segment.noteRect.yMin + 1,
                        segment.noteRect.xSize - 1,
                        segment.noteRect.ySize - 1);

                    g.FillRectangle(
                        Brushes.LightSalmon,
                        segment.noteRect.xMin + 1,
                        segment.noteRect.yMin + 3,
                        segment.noteRect.xSize - 1,
                        segment.noteRect.ySize - 5);
                }
                else
                {
                    g.FillRectangle(
                        (hovering ? Brushes.Salmon : Brushes.Red),
                        segment.noteRect.xMin + 1,
                        segment.noteRect.yMin + 1,
                        segment.noteRect.xSize - 1,
                        segment.noteRect.ySize - 1);
                }
            }
        }
    }
}
