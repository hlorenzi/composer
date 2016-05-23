using System.Collections.Generic;
using System.Drawing;


namespace Composer.Editor
{
    class ViewManager
    {
        ControlEditor control;
        public Project.Project project;
        public List<Row> rows;
        public List<Element> elements;

        bool mouseIsDown;
        float mouseCurrentX, mouseCurrentY;
        float mouseDragOriginX, mouseDragOriginY;
        float timeDragOrigin;
        Util.Pitch pitchDragOrigin;

        public Element currentHoverElement;
        public InteractableRegion currentHoverRegion;
        public InteractableRegion currentDraggingIsolatedRegion;


        public ViewManager(ControlEditor control, Project.Project project)
        {
            this.control = control;
            this.project = project;
            this.rows = new List<Row>();
            this.elements = new List<Element>();
        }


        public void Rebuild()
        {
            this.rows.Clear();
            this.elements.Clear();

            var projectPitchedNoteTracks = new List<Project.TrackPitchedNotes>();
            foreach (var track in this.project.tracks)
            {
                if (track is Project.TrackPitchedNotes)
                    projectPitchedNoteTracks.Add((Project.TrackPitchedNotes)track);
            }

            var currentTime = 0f;
            var currentSegment = 0;
            while (currentSegment <= this.project.segmentBreaks.Count)
            {
                var endTime = this.project.Length;
                if (currentSegment < this.project.segmentBreaks.Count)
                    endTime = this.project.segmentBreaks[currentSegment].time;

                var row = new Row(this, new Util.TimeRange(currentTime, endTime));

                foreach (var track in this.project.tracks)
                {
                    if (track is Project.TrackPitchedNotes)
                        row.trackSegments.Add(new TrackSegmentPitchedNotes(
                            this, row,
                            new List<Project.TrackPitchedNotes> { (Project.TrackPitchedNotes)track }));
                }

                this.rows.Add(row);
                currentTime = endTime;
                currentSegment++;
            }

            var tracks = new List<Project.TrackPitchedNotes>();
            foreach (var track in this.project.tracks)
            {
                var trackPitchedNotes = (track as Project.TrackPitchedNotes);
                if (trackPitchedNotes != null)
                {
                    foreach (var note in trackPitchedNotes.notes)
                        this.elements.Add(new ElementPitchedNote(this, trackPitchedNotes, note));
                }
            }

            foreach (var element in this.elements)
            {
                element.AssignTrack();
            }

            RefreshTracks();
            RefreshElements();
        }


        public void RefreshTracks()
        {
            var y = TopMargin;
            foreach (var row in this.rows)
            {
                row.Rebuild(LeftMargin, y);
                y = row.layoutRect.yMax;
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
                if (this.currentHoverRegion != null && this.currentHoverRegion.isolated)
                    this.currentHoverRegion.dragFunc(this.currentHoverRegion);
                else
                {
                    foreach (var element in this.elements)
                    {
                        if (element.selected)
                        {
                            element.Drag();
                            element.Rebuild();
                        }
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

                foreach (var row in this.rows)
                {
                    foreach (var region in row.interactableRegions)
                    {
                        if (region.rect.Contains(x, y))
                        {
                            this.currentHoverElement = null;
                            this.currentHoverRegion = region;
                        }
                    }
                }

                if (this.currentHoverRegion != null)
                {
                    switch (this.currentHoverRegion.cursorKind)
                    {
                        case InteractableRegion.CursorKind.Select:
                            this.control.Cursor = System.Windows.Forms.Cursors.Hand; break;
                        case InteractableRegion.CursorKind.MoveAll:
                            this.control.Cursor = System.Windows.Forms.Cursors.SizeAll; break;
                        case InteractableRegion.CursorKind.MoveHorizontal:
                            this.control.Cursor = System.Windows.Forms.Cursors.SizeWE; break;
                        default:
                            this.control.Cursor = System.Windows.Forms.Cursors.Default; break;
                    }
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

            if ((!ctrlKey && (this.currentHoverElement == null || !this.currentHoverElement.selected)) ||
                this.currentHoverRegion != null && this.currentHoverRegion.isolated)
                UnselectAll();

            if (this.currentHoverElement != null)
                this.currentHoverElement.selected = true;

            var trackSegment = this.GetTrackSegmentAtPosition(x, y);
            this.timeDragOrigin = trackSegment.GetTimeAtPosition(x, y);
            this.pitchDragOrigin = trackSegment.GetPitchAtPosition(x, y);

            if (this.currentHoverRegion != null && this.currentHoverRegion.isolated)
            {
                this.currentDraggingIsolatedRegion = this.currentHoverRegion;
                this.currentDraggingIsolatedRegion.dragStartFunc(this.currentDraggingIsolatedRegion);
            }
            else
            {
                foreach (var element in this.elements)
                {
                    if (element.selected)
                        element.DragStart();
                }
            }

            this.control.Refresh();
        }


        public void OnMouseUp(float x, float y)
        {
            if (this.currentDraggingIsolatedRegion != null)
            {
                this.currentDraggingIsolatedRegion.dragEndFunc(this.currentDraggingIsolatedRegion);
                this.currentDraggingIsolatedRegion = null;
            }
            else
            {
                foreach (var element in this.elements)
                {
                    if (element.selected)
                        element.DragEnd();
                }
            }

            this.mouseIsDown = false;
            this.RefreshTracks();
            this.RefreshElements();
            this.control.Refresh();
        }


        public void Draw(Graphics g)
        {
            foreach (var row in this.rows)
                row.Draw(g);

            foreach (var element in this.elements)
                element.Draw(g, currentHoverElement == element, element.selected);

            foreach (var row in this.rows)
                row.DrawOverlay(g);
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
            get { return 20; }
        }


        public float RightMargin
        {
            get { return 20; }
        }


        public float TopMargin
        {
            get { return 20; }
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
                var trackAtMouse = this.GetTrackSegmentAtPosition(this.mouseCurrentX, this.mouseCurrentY);
                var timeAtMouse = trackAtMouse.GetTimeAtPosition(this.mouseCurrentX, this.mouseCurrentY);
                var timeAtMouseClipped = 
                    System.Math.Max(trackAtMouse.row.timeRange.Start,
                    System.Math.Min(trackAtMouse.row.timeRange.End, timeAtMouse));

                var offset = timeAtMouseClipped - this.timeDragOrigin;
                return (float)(System.Math.Round(offset / TimeSnap) * TimeSnap);
            }
        }


        public float DragTimeOverflowOffset
        {
            get
            {
                var trackAtMouse = this.GetTrackSegmentAtPosition(this.mouseCurrentX, this.mouseCurrentY);
                var timeAtMouse = trackAtMouse.GetTimeAtPosition(this.mouseCurrentX, this.mouseCurrentY);

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
