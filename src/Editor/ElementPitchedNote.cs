using System.Collections.Generic;
using System.Drawing;


namespace Composer.Editor
{
    class ElementPitchedNote : Element
    {
        int assignedTrack = -1;
        public Project.PitchedNote projectPitchedNote;
        public Project.TrackPitchedNotes projectTrackPitchedNode;
        List<Segment> segments;

        Util.TimeRange timeRange;
        Util.Pitch pitch;


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

            this.timeRange = this.projectPitchedNote.timeRange;
            this.pitch = this.projectPitchedNote.pitch;

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


        class Segment
        {
            public Util.Rect noteRect;
        }


        public override void RefreshLayout()
        {
            this.segments.Clear();
            this.interactableRegions.Clear();

            var tMult = this.manager.TimeToPixelsMultiplier;
            var pMult = this.manager.PitchedNoteHeight;

            foreach (var row in this.manager.EnumerateRowsInTimeRange(this.timeRange))
            {
                var trackPitchedNote = (TrackSegmentPitchedNotes)row.trackSegments[this.assignedTrack];

                if (this.pitch.MidiPitch < trackPitchedNote.minPitch.MidiPitch ||
                    this.pitch.MidiPitch > trackPitchedNote.maxPitch.MidiPitch)
                    continue;

                var midiPitchMinusTrackMin =
                    this.pitch.MidiPitch - trackPitchedNote.minPitch.MidiPitch;

                var startTimeMinusTrackStart = System.Math.Max(
                    0,
                    this.timeRange.Start - trackPitchedNote.row.timeRange.Start);

                var endTimeMinusTrackStart = System.Math.Min(
                    trackPitchedNote.row.timeRange.End,
                    this.timeRange.End) - trackPitchedNote.row.timeRange.Start;

                var noteRect = new Util.Rect(
                    trackPitchedNote.contentRect.xMin + tMult * startTimeMinusTrackStart,
                    trackPitchedNote.contentRect.yMax - pMult * (midiPitchMinusTrackMin + 1),
                    trackPitchedNote.contentRect.xMin + tMult * endTimeMinusTrackStart,
                    trackPitchedNote.contentRect.yMax - pMult * midiPitchMinusTrackMin);

                this.segments.Add(new Segment { noteRect = noteRect });

                this.interactableRegions.Add(
                    new InteractableRegion(InteractableRegion.CursorKind.MoveAll, noteRect));
            }
        }


        public override void BeginModify()
        {
            this.manager.project.RemovePitchedNote(
                this.manager.project.GetTrackIndex(this.projectTrackPitchedNode),
                this.projectPitchedNote);
        }


        public override void EndModify()
        {
            this.projectPitchedNote.timeRange = this.timeRange;
            this.projectPitchedNote.pitch = this.pitch;

            this.manager.project.InsertPitchedNote(
                this.manager.project.GetTrackIndex(this.projectTrackPitchedNode),
                this.projectPitchedNote);
        }


        public override void Drag()
        {
            this.timeRange =
                this.projectPitchedNote.timeRange.OffsetBy(this.manager.DragTimeOffsetClampedToRow);

            this.pitch =
                this.projectPitchedNote.pitch.OffsetMidiPitchBy(this.manager.DragMidiPitchOffset);
        }


        public override void OnPressUp(bool ctrlKey, bool shiftKey)
        {
            this.pitch =
                this.pitch.OffsetMidiPitchBy(shiftKey ? 12 : 1);
        }


        public override void OnPressDown(bool ctrlKey, bool shiftKey)
        {
            this.pitch =
                this.pitch.OffsetMidiPitchBy(shiftKey ? -12 : -1);
        }


        public override void OnPressRight(bool ctrlKey, bool shiftKey)
        {
            if (shiftKey)
            {
                this.timeRange.Duration =
                    System.Math.Max(
                        this.manager.TimeSnap,
                        this.timeRange.Duration + this.manager.TimeSnap);
            }
            else
                this.timeRange =
                    this.timeRange.OffsetBy(this.manager.TimeSnap);
        }


        public override void OnPressLeft(bool ctrlKey, bool shiftKey)
        {
            if (shiftKey)
            {
                this.timeRange.Duration =
                    System.Math.Max(
                        this.manager.TimeSnap,
                        this.timeRange.Duration - this.manager.TimeSnap);
            }
            else
                this.timeRange =
                    this.timeRange.OffsetBy(-this.manager.TimeSnap);
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
