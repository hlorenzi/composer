using System.Collections.Generic;
using System.Drawing;


namespace Composer.Editor
{
    class ViewManager
    {
        ControlEditor control;
        Project.Project project;
        public List<Row> rows;
        public List<Element> elements;

        bool mouseIsDown;
        float mouseCurrentX, mouseCurrentY;
        float mouseDragOriginX, mouseDragOriginY;
        float timeDragOrigin;
        Util.Pitch pitchDragOrigin;

        Element currentHoverElement;
        InteractableRegion currentHoverRegion;


        public ViewManager(ControlEditor control, Project.Project project)
        {
            this.control = control;
            this.project = project;
            this.rows = new List<Row>();
            this.elements = new List<Element>();
        }


        public class Row
        {
            public Util.TimeRange timeRange;
            public List<TrackSegment> trackSegments = new List<TrackSegment>();
        }


        public void RebuildTracks()
        {
            this.rows.Clear();

            var tracks = new List<Project.TrackPitchedNotes>();
            foreach (var track in this.project.tracks)
            {
                if (track is Project.TrackPitchedNotes)
                    tracks.Add((Project.TrackPitchedNotes)track);
            }
            
            for (var i = 0; i < 3; i++)
            {
                var row = new Row();
                row.timeRange = new Util.TimeRange(i * this.project.TimeInWholeNote * 4, (i + 1) * this.project.TimeInWholeNote * 4);
                row.trackSegments.Add(new TrackSegmentPitchedNotes(this, row, tracks));
                this.rows.Add(row);
            }
        }


        public void RefreshTracks()
        {
            var y = TopMargin;
            foreach (var row in this.rows)
            {
                foreach (var track in row.trackSegments)
                {
                    track.Rebuild(LeftMargin, y);
                    y = track.layoutRect.yMax;
                }
            }
        }


        public void RebuildElements()
        {
            this.elements.Clear();

            var tracks = new List<Project.TrackPitchedNotes>();
            foreach (var track in this.project.tracks)
            {
                var trackPitchedNotes = (track as Project.TrackPitchedNotes);
                if (trackPitchedNotes != null)
                {
                    foreach (var note in trackPitchedNotes.notes)
                        this.elements.Add(new ElementPitchedNote(this, note));
                }
            }

            foreach (var element in this.elements)
            {
                element.AssignTrack();
            }
        }


        public void RefreshElements()
        {
            foreach (var element in this.elements)
            {
                element.Rebuild();
            }
        }


        public void UnselectAll()
        {
            foreach (var element in this.elements)
                element.selected = false;
        }


        public void OnMouseMove(float x, float y)
        {
            this.mouseCurrentX = x;
            this.mouseCurrentY = y;

            if (this.mouseIsDown)
            {
                foreach (var element in this.elements)
                {
                    if (element.selected)
                    {
                        element.Drag();
                        element.Rebuild();
                    }
                }

                this.control.Refresh();
            }
            else
            {
                var lastHoverRegion = this.currentHoverRegion;

                this.currentHoverElement = null;
                this.currentHoverRegion = null;

                foreach (var element in this.elements)
                {
                    foreach (var region in element.interactableRegions)
                    {
                        if (region.rect.Contains(x, y))
                        {
                            this.currentHoverElement = element;
                            this.currentHoverRegion = region;
                        }
                    }
                }

                if (this.currentHoverRegion != null)
                {
                    if (this.currentHoverRegion.kind == InteractableRegion.Kind.MoveAll)
                        this.control.Cursor = System.Windows.Forms.Cursors.Hand;
                    else
                        this.control.Cursor = System.Windows.Forms.Cursors.Default;
                }
                else
                    this.control.Cursor = System.Windows.Forms.Cursors.Default;

                if (this.currentHoverRegion != lastHoverRegion)
                    this.control.Refresh();
            }
        }


        public void OnMouseDown(float x, float y, bool ctrlKey, bool shiftKey)
        {
            this.mouseIsDown = true;
            this.mouseDragOriginX = x;
            this.mouseDragOriginY = y;

            if (!ctrlKey && (this.currentHoverElement == null || !this.currentHoverElement.selected))
                UnselectAll();

            if (this.currentHoverElement != null)
                this.currentHoverElement.selected = true;

            var trackSegment = this.GetTrackSegmentAtPosition(x, y);
            this.timeDragOrigin = trackSegment.GetTimeAtPosition(x, y);
            this.pitchDragOrigin = trackSegment.GetPitchAtPosition(x, y);

            foreach (var element in this.elements)
            {
                if (element.selected)
                    element.DragStart();
            }

            this.control.Refresh();
        }


        public void OnMouseUp(float x, float y)
        {
            this.mouseIsDown = false;
            this.RefreshTracks();
            this.RefreshElements();
            this.control.Refresh();
        }


        public void Draw(Graphics g)
        {
            foreach (var row in this.rows)
            {
                foreach (var track in row.trackSegments)
                    track.Draw(g);
            }

            foreach (var element in this.elements)
                element.Draw(g, currentHoverElement == element, element.selected);
        }


        public IEnumerable<Row> EnumerateRowsInTimeRange(Util.TimeRange timeRange)
        {
            foreach (var row in this.rows)
            {
                if (row.timeRange.Overlaps(timeRange))
                    yield return row;
            }
        }


        public TrackSegment GetTrackSegmentAtPosition(float x, float y)
        {
            foreach (var row in this.rows)
            {
                foreach (var track in row.trackSegments)
                {
                    if (track.layoutRect.ContainsY(y))
                        return track;
                }
            }

            if (y <= this.TopMargin)
                return this.rows[0].trackSegments[0];

            var lastRow = this.rows[this.rows.Count - 1];
            return lastRow.trackSegments[lastRow.trackSegments.Count - 1];
        }


        public float LeftMargin
        {
            get { return 10; }
        }


        public float RightMargin
        {
            get { return 10; }
        }


        public float TopMargin
        {
            get { return 10; }
        }


        public float PitchedNoteHeight
        {
            get { return 8; }
        }


        public float TimeToPixelsMultiplier
        {
            get { return 100f / this.project.TimeInWholeNote; }
        }


        public float TimeSnap
        {
            get { return this.project.TimeInWholeNote / 16; }
        }


        public float DragTimeOffset
        {
            get
            {
                var timeAtMouse =
                    this.GetTrackSegmentAtPosition(this.mouseCurrentX, this.mouseCurrentY)
                    .GetTimeAtPosition(this.mouseCurrentX, this.mouseCurrentY);

                var offset = timeAtMouse - this.timeDragOrigin;
                return (float)(System.Math.Round(offset / TimeSnap) * TimeSnap);
            }
        }


        public float DragMidiPitchOffset
        {
            get
            {
                var pitchAtMouse =
                    this.GetTrackSegmentAtPosition(this.mouseCurrentX, this.mouseCurrentY)
                    .GetPitchAtPosition(this.mouseCurrentX, this.mouseCurrentY);

                var offset = pitchAtMouse.MidiPitch - this.pitchDragOrigin.MidiPitch;
                return (float)System.Math.Round(offset);
            }
        }
    }
}
