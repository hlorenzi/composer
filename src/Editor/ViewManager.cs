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

        public int currentTrack;
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

            var currentTime = 0f;
            var currentSegment = 0;
            while (currentSegment <= this.project.sectionBreaks.Count)
            {
                var endTime = this.project.Length;
                var isLastRow = true;
                if (currentSegment < this.project.sectionBreaks.Count)
                {
                    endTime = this.project.sectionBreaks[currentSegment].time;
                    isLastRow = false;
                }

                var row = new Row(this, new Util.TimeRange(currentTime, endTime), isLastRow);

                row.trackSegmentKeyChanges = new TrackSegmentKeyChanges(this, row);
                row.trackSegments.Add(row.trackSegmentKeyChanges);

                row.trackSegmentMeterChanges = new TrackSegmentMeterChanges(this, row);
                row.trackSegments.Add(row.trackSegmentMeterChanges);

                foreach (var track in this.project.tracks)
                {
                    if (!track.visible)
                        continue;

                    if (track is Project.TrackPitchedNotes)
                        row.trackSegments.Add(new TrackSegmentPitchedNotes(
                            this, row,
                            new List<Project.TrackPitchedNotes> { (Project.TrackPitchedNotes)track }));
                }

                this.rows.Add(row);
                currentTime = endTime;
                currentSegment++;
            }

            foreach (var keyChange in this.project.keyChanges)
                this.elements.Add(new ElementKeyChange(this, keyChange));

            foreach (var meterChange in this.project.meterChanges)
                this.elements.Add(new ElementMeterChange(this, meterChange));

            foreach (var track in this.project.tracks)
            {
                if (!track.visible)
                    continue;

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
                    this.currentHoverRegion.dragFunc?.Invoke(this.currentHoverRegion);
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

            this.timeDragOrigin = this.GetTimeAtPosition(x, y, true);
            this.pitchDragOrigin = this.GetTrackSegmentAtPosition(x, y).GetPitchAtPosition(y);

            if (this.currentHoverRegion != null && this.currentHoverRegion.isolated)
            {
                this.currentDraggingIsolatedRegion = this.currentHoverRegion;
                this.currentDraggingIsolatedRegion.dragStartFunc?.Invoke(this.currentDraggingIsolatedRegion);
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
            this.mouseIsDown = false;

            if (this.currentDraggingIsolatedRegion != null)
            {
                this.currentDraggingIsolatedRegion.dragEndFunc?.Invoke(this.currentDraggingIsolatedRegion);

                if (this.currentHoverRegion == this.currentDraggingIsolatedRegion)
                    this.currentDraggingIsolatedRegion.clickFunc?.Invoke(this.currentDraggingIsolatedRegion);

                this.currentDraggingIsolatedRegion = null;
            }
            else
            {
                foreach (var element in this.elements)
                {
                    if (element.selected)
                        element.DragEnd();
                }

                this.RefreshTracks();
                this.RefreshElements();
            }

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


        public Row GetRowOverlapping(float time)
        {
            foreach (var row in this.rows)
            {
                if (row.timeRange.Overlaps(time))
                    return row;
            }

            return null;
        }


        public IEnumerable<Row> EnumerateRowsInTimeRange(Util.TimeRange timeRange)
        {
            foreach (var row in this.rows)
            {
                if (row.timeRange.OverlapsRange(timeRange))
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


        public float GetTimeAtPosition(float x, float y, bool clampToRowRange)
        {
            var time = 0f;
            var timeClampMin = 0f;
            var timeClampMax = 0f;

            if (this.rows.Count > 0)
            {
                if (y <= this.rows[0].layoutRect.yMin)
                {
                    time = this.rows[0].GetTimeAtPosition(x);
                    timeClampMin = this.rows[0].timeRange.Start;
                    timeClampMax = this.rows[0].timeRange.End;
                }
                else
                {
                    var lastRow = this.rows[this.rows.Count - 1];
                    time = lastRow.GetTimeAtPosition(x);
                    timeClampMin = lastRow.timeRange.Start;
                    timeClampMax = lastRow.timeRange.End;
                }
            }

            foreach (var row in this.rows)
            {
                if (row.layoutRect.ContainsY(y))
                {
                    time = row.GetTimeAtPosition(x);
                    timeClampMin = row.timeRange.Start;
                    timeClampMax = row.timeRange.End;
                    break;
                }
            }

            var timeClamped = System.Math.Max(timeClampMin, System.Math.Min(timeClampMax, time));
            return (clampToRowRange ? timeClamped : time);
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
            get { return 100f / this.project.WholeNoteDuration; }
        }


        public float TimeSnap
        {
            get { return this.project.WholeNoteDuration / 16; }
        }


        public float MouseTime
        {
            get
            {
                var time = this.GetTimeAtPosition(this.mouseCurrentX, this.mouseCurrentY, false);
                return (float)(System.Math.Round(time / TimeSnap) * TimeSnap);
            }
        }


        public float MouseTimeClampedToRow
        {
            get
            {
                var time = this.GetTimeAtPosition(this.mouseCurrentX, this.mouseCurrentY, true);
                return (float)(System.Math.Round(time / TimeSnap) * TimeSnap);
            }
        }


        public float DragTimeOffsetIrrespectiveOfRow
        {
            get
            {
                var offset =
                    (this.mouseCurrentX - this.mouseDragOriginX) / this.TimeToPixelsMultiplier;

                return (float)(System.Math.Round(offset / TimeSnap) * TimeSnap);
            }
        }


        public float DragTimeOffset
        {
            get
            {
                var offset =
                    this.GetTimeAtPosition(this.mouseCurrentX, this.mouseCurrentY, false) -
                    this.timeDragOrigin;

                return (float)(System.Math.Round(offset / TimeSnap) * TimeSnap);
            }
        }


        public float DragTimeOffsetClampedToRow
        {
            get
            {
                var offset =
                    this.GetTimeAtPosition(this.mouseCurrentX, this.mouseCurrentY, true) -
                    this.timeDragOrigin;

                return (float)(System.Math.Round(offset / TimeSnap) * TimeSnap);
            }
        }


        public float DragMidiPitchOffset
        {
            get
            {
                var pitchAtMouse =
                    this.GetTrackSegmentAtPosition(this.mouseCurrentX, this.mouseCurrentY)
                    .GetPitchAtPosition(this.mouseCurrentY);

                var offset = pitchAtMouse.MidiPitch - this.pitchDragOrigin.MidiPitch;
                return (float)System.Math.Round(offset);
            }
        }
    }
}
