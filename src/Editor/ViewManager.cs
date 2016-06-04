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

        float width, height;
        float scrollX, scrollY;
        Util.Rect layoutRect;

        enum MouseAction
        {
            Selection, Cursor, Scrolling
        }

        bool mouseIsDown;
        MouseAction mouseAction;
        float mouseCurrentX, mouseCurrentY;
        float mouseDragOriginX, mouseDragOriginY;
        float timeDragOrigin;
        int trackDragOrigin;
        Util.Pitch pitchDragOrigin;

        public bool cursorVisible;
        public float cursorTime1, cursorTime2;
        public int cursorTrack1, cursorTrack2;
        public bool noteInsertionMode;
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


        public void SetSize(float width, float height)
        {
            this.width = width;
            this.height = height;
        }


        public void SetCursorVisible(bool visible)
        {
            this.cursorVisible = visible;
        }


        public void SetCursorTimeRange(float time1, float time2)
        {
            this.cursorTime1 =
                System.Math.Max(0,
                System.Math.Min(this.project.Length, time1));

            this.cursorTime2 =
                System.Math.Max(0,
                System.Math.Min(this.project.Length, time2));
        }


        public void SetCursorTrackIndices(int track1, int track2)
        {
            this.cursorTrack1 = track1;
            this.cursorTrack2 = track2;
        }


        public bool GetInsertionPosition(out int trackIndex, out float time)
        {
            trackIndex = -1;
            time = 0;

            if (!this.cursorVisible)
                return false;

            if (this.cursorTrack1 != this.cursorTrack2)
                return false;

            var trackSegment = this.rows[0].trackSegments[this.cursorTrack1];
            var trackSegmentPitchedNotes = trackSegment as TrackSegmentPitchedNotes;
            if (trackSegmentPitchedNotes != null)
            {
                trackIndex = this.project.GetTrackIndex(trackSegmentPitchedNotes.projectTracks[0]);
                time = this.cursorTime1;
                return true;
            }

            return false;
        }


        public void SetNoteInsertionMode(bool enabled)
        {
            this.noteInsertionMode = enabled;
        }


        public Project.PitchedNote GetNoteInsertionModeNote()
        {
            if (!this.noteInsertionMode)
                return null;

            foreach (var element in this.elements)
            {
                if (element.selected)
                {
                    var note = element as ElementPitchedNote;
                    if (note != null)
                        return note.projectPitchedNote;
                }
            }

            return null;
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

            this.cursorVisible = false;
            this.SetCursorTimeRange(this.cursorTime1, this.cursorTime2);
            this.SetCursorTrackIndices(this.cursorTrack1, this.cursorTrack2);

            Refresh();
            this.ScrollTo(this.scrollX, this.scrollY);
        }


        public void Refresh()
        {
            this.layoutRect = new Util.Rect(0, 0, 0, 0);

            var y = TopMargin;
            foreach (var row in this.rows)
            {
                row.Rebuild(LeftMargin, y);
                y = row.layoutRect.yMax;

                this.layoutRect = this.layoutRect.Include(row.layoutRect);
            }

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


        public void SetPitchedNoteSelection(int projectTrackIndex, Project.PitchedNote pitchedNote, bool selected)
        {
            var projectTrack = this.project.tracks[projectTrackIndex];

            foreach (var element in this.elements)
            {
                var note = element as ElementPitchedNote;
                if (note != null)
                {
                    if (note.projectTrackPitchedNode == projectTrack &&
                        note.projectPitchedNote == pitchedNote)
                        note.selected = selected;
                }
            }
        }


        public void ScrollTo(float x, float y)
        {
            this.scrollX =
                System.Math.Max(0,
                System.Math.Min(this.layoutRect.xMax - 100, x));

            this.scrollY =
                System.Math.Max(0,
                System.Math.Min(this.layoutRect.yMax - this.height + 30, y));
        }


        public void OnPressUp(bool ctrlKey, bool shiftKey)
        {
            foreach (var element in this.elements)
            {
                if (element.selected)
                {
                    element.OnPressUp(ctrlKey, shiftKey);
                    element.Rebuild();
                }
            }

            this.Refresh();
            this.control.Refresh();
        }


        public void OnPressDown(bool ctrlKey, bool shiftKey)
        {
            foreach (var element in this.elements)
            {
                if (element.selected)
                {
                    element.OnPressDown(ctrlKey, shiftKey);
                    element.Rebuild();
                }
            }

            this.Refresh();
            this.control.Refresh();
        }


        public void OnPressRight(bool ctrlKey, bool shiftKey)
        {
            var anySelected = false;
            foreach (var element in this.elements)
            {
                if (element.selected)
                {
                    anySelected = true;
                    element.OnPressRight(ctrlKey, shiftKey);
                    element.Rebuild();
                }
            }

            if (!anySelected)
            {
                this.SetCursorTimeRange(this.cursorTime1 + this.TimeSnap, this.cursorTime2 + this.TimeSnap);
                this.SetCursorVisible(true);
            }

            this.Refresh();
            this.control.Refresh();
        }


        public void OnPressLeft(bool ctrlKey, bool shiftKey)
        {
            var anySelected = false;

            foreach (var element in this.elements)
            {
                if (element.selected)
                {
                    anySelected = true;
                    element.OnPressLeft(ctrlKey, shiftKey);
                    element.Rebuild();
                }
            }

            if (!anySelected)
            {
                this.SetCursorTimeRange(this.cursorTime1 - this.TimeSnap, this.cursorTime2 - this.TimeSnap);
                this.SetCursorVisible(true);
            }

            this.Refresh();
            this.control.Refresh();
        }


        public void OnMouseMove(float x, float y)
        {
            this.mouseCurrentX = x + scrollX;
            this.mouseCurrentY = y + scrollY;

            if (this.mouseIsDown)
            {
                if (this.mouseAction == MouseAction.Selection)
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
                }
                else if (this.mouseAction == MouseAction.Scrolling)
                {
                    var deltaX = this.mouseDragOriginX - this.mouseCurrentX;
                    var deltaY = this.mouseDragOriginY - this.mouseCurrentY;

                    this.ScrollTo(this.scrollX + deltaX, this.scrollY + deltaY);

                    this.mouseCurrentX = this.mouseDragOriginX = x + scrollX;
                    this.mouseCurrentY = this.mouseDragOriginY = y + scrollY;
                }
                else if (this.mouseAction == MouseAction.Cursor)
                {
                    var track = this.GetTrackIndexAtPosition(this.mouseCurrentX, this.mouseCurrentY);

                    var time = this.GetTimeAtPosition(this.mouseCurrentX, this.mouseCurrentY, true);
                    var timeSnapped = (float)(System.Math.Round(time / TimeSnap) * TimeSnap);

                    this.SetCursorTimeRange(this.cursorTime1, timeSnapped);
                    this.SetCursorTrackIndices(this.cursorTrack1, track);
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
                        if (region.rect.Contains(x + scrollX, y + scrollY))
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
                        if (region.rect.Contains(x + scrollX, y + scrollY))
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


        public void OnMouseDown(bool scroll, float x, float y, bool ctrlKey, bool shiftKey)
        {
            this.mouseIsDown = true;
            this.mouseDragOriginX = x + scrollX;
            this.mouseDragOriginY = y + scrollY;
            this.noteInsertionMode = false;

            if (scroll)
                this.mouseAction = MouseAction.Scrolling;
            else
            {
                this.mouseAction = MouseAction.Selection;
                this.cursorVisible = false;

                if ((!ctrlKey && (this.currentHoverElement == null || !this.currentHoverElement.selected)) ||
                    this.currentHoverRegion != null && this.currentHoverRegion.isolated)
                    UnselectAll();

                if (this.currentHoverElement != null)
                    this.currentHoverElement.selected = true;

                this.timeDragOrigin =
                    this.GetTimeAtPosition(this.mouseDragOriginX, this.mouseDragOriginY, true);

                this.trackDragOrigin =
                    this.GetTrackIndexAtPosition(this.mouseDragOriginX, this.mouseDragOriginY);

                var timeSnapped = (float)(System.Math.Round(this.timeDragOrigin / TimeSnap) * TimeSnap);

                this.pitchDragOrigin =
                    this.GetTrackSegmentAtPosition(this.mouseDragOriginX, this.mouseDragOriginY).
                    GetPitchAtPosition(this.mouseDragOriginY);
                
                if (this.currentHoverRegion != null && this.currentHoverRegion.isolated)
                {
                    this.currentDraggingIsolatedRegion = this.currentHoverRegion;
                    this.currentDraggingIsolatedRegion.dragStartFunc?.Invoke(this.currentDraggingIsolatedRegion);
                }
                else if (!ctrlKey && !shiftKey && this.currentHoverRegion == null)
                {
                    this.mouseAction = MouseAction.Cursor;
                    this.SetCursorTimeRange(timeSnapped, timeSnapped);
                    this.SetCursorTrackIndices(this.trackDragOrigin, this.trackDragOrigin);
                    this.cursorVisible = true;
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
        }


        public void OnMouseUp(float x, float y)
        {
            this.mouseIsDown = false;

            if (this.mouseAction == MouseAction.Selection)
            {
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

                    this.Refresh();
                }

                this.control.Refresh();
            }
        }


        public void Draw(Graphics g)
        {
            g.TranslateTransform(-scrollX, -scrollY);

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


        public int GetTrackIndexAtPosition(float x, float y)
        {
            foreach (var row in this.rows)
            {
                for (var i = 0; i < row.trackSegments.Count; i++)
                {
                    if (row.trackSegments[i].layoutRect.ContainsY(y))
                        return i;
                }
            }

            if (y <= this.TopMargin)
                return 0;

            var lastRow = this.rows[this.rows.Count - 1];
            return lastRow.trackSegments.Count - 1;
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


        public Util.TimeRange CursorTimeRange
        {
            get
            {
                return new Util.TimeRange(
                    System.Math.Min(this.cursorTime1, this.cursorTime2),
                    System.Math.Max(this.cursorTime1, this.cursorTime2));
            }
        }


        public int CursorFirstTrackIndex
        {
            get
            {
                return System.Math.Min(this.cursorTrack1, this.cursorTrack2);
            }
        }


        public int CursorLastTrackIndex
        {
            get
            {
                return System.Math.Max(this.cursorTrack1, this.cursorTrack2);
            }
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
