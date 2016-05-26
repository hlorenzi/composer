using System.Collections.Generic;
using System.Drawing;


namespace Composer.Editor
{
    class Row
    {
        ViewManager manager;
        public Util.TimeRange timeRange;
        public Util.Rect layoutRect;
        public List<InteractableRegion> interactableRegions;

        public TrackSegmentMeterChanges trackSegmentMeterChanges;
        public List<TrackSegment> trackSegments;

        public bool isLastRow;
        public float resizeEndTime;

        InteractableRegion regionSectionHandle;
        InteractableRegion regionAddSectionBeforeButton;
        InteractableRegion regionAddSectionAfterButton;

        const int SECTION_HANDLE_HEIGHT = 16;
        const int ADD_SECTION_BUTTON_SIZE = 20;
        const int ADD_SECTION_BUTTON_MARGIN = 2;


        public Row(ViewManager manager, Util.TimeRange timeRange, bool isLastRow)
        {
            this.manager = manager;
            this.timeRange = timeRange;
            this.resizeEndTime = timeRange.End;
            this.trackSegments = new List<TrackSegment>();
            this.interactableRegions = new List<InteractableRegion>();
            this.isLastRow = isLastRow;
        }


        public void Rebuild(float x, float y)
        {
            this.resizeEndTime = timeRange.End;

            this.layoutRect = new Util.Rect(
                x, y,
                x, y + ADD_SECTION_BUTTON_SIZE + ADD_SECTION_BUTTON_MARGIN + SECTION_HANDLE_HEIGHT);

            foreach (var track in this.trackSegments)
            {
                track.Rebuild(x, this.layoutRect.yMax);
                this.layoutRect = this.layoutRect.Include(track.layoutRect);
            }

            this.layoutRect.yMax += ADD_SECTION_BUTTON_MARGIN;

            this.interactableRegions.Clear();

            this.regionAddSectionBeforeButton = new InteractableRegion(
                InteractableRegion.CursorKind.Select,
                new Util.Rect(
                    this.layoutRect.xMin,
                    this.layoutRect.yMin + ADD_SECTION_BUTTON_MARGIN,
                    this.layoutRect.xMin + ADD_SECTION_BUTTON_SIZE,
                    this.layoutRect.yMin + ADD_SECTION_BUTTON_MARGIN + ADD_SECTION_BUTTON_SIZE));
            this.regionAddSectionBeforeButton.SetButton(this.Click_AddSectionBefore);
            this.interactableRegions.Add(this.regionAddSectionBeforeButton);

            if (this.isLastRow)
            {
                this.regionAddSectionAfterButton = new InteractableRegion(
                    InteractableRegion.CursorKind.Select,
                    new Util.Rect(
                        this.layoutRect.xMin,
                        this.layoutRect.yMax + ADD_SECTION_BUTTON_MARGIN,
                        this.layoutRect.xMin + ADD_SECTION_BUTTON_SIZE,
                        this.layoutRect.yMax + ADD_SECTION_BUTTON_MARGIN + ADD_SECTION_BUTTON_SIZE));
                this.regionAddSectionAfterButton.SetButton(this.Click_AddSectionAfter);
                this.interactableRegions.Add(this.regionAddSectionAfterButton);
            }

            this.regionSectionHandle = new InteractableRegion(
                InteractableRegion.CursorKind.MoveHorizontal,
                new Util.Rect(
                    this.layoutRect.xMax - 5,
                    this.layoutRect.yMin + ADD_SECTION_BUTTON_SIZE + ADD_SECTION_BUTTON_MARGIN * 2,
                    this.layoutRect.xMax + 5,
                    this.layoutRect.yMin + ADD_SECTION_BUTTON_SIZE + ADD_SECTION_BUTTON_MARGIN * 2 + 10));
            this.regionSectionHandle.SetIsolated(null, this.Drag_SectionHandle, this.DragEnd_SectionHandle);
            this.interactableRegions.Add(this.regionSectionHandle);
        }


        public float GetTimeAtPosition(float x)
        {
            return this.timeRange.Start + (x - this.layoutRect.xMin) / this.manager.TimeToPixelsMultiplier;
        }


        public void Drag_SectionHandle(InteractableRegion region)
        {
            this.resizeEndTime = System.Math.Max(
                this.timeRange.Start,
                this.timeRange.End + this.manager.DragTimeOffsetIrrespectiveOfRow);
        }


        public void DragEnd_SectionHandle(InteractableRegion region)
        {
            if (this.resizeEndTime >= this.timeRange.Start &&
                this.resizeEndTime != this.timeRange.End)
            {
                if (this.resizeEndTime < this.timeRange.End)
                    this.manager.project.CutRange(
                        Util.TimeRange.StartEnd(this.resizeEndTime, this.timeRange.End));
                else
                    this.manager.project.InsertEmptySpace(
                        this.timeRange.End, this.resizeEndTime - this.timeRange.End);
            }
            this.manager.Rebuild();
        }


        public void Click_AddSectionBefore(InteractableRegion region)
        {
            this.manager.project.InsertSection(this.timeRange.Start, this.manager.project.WholeNoteDuration * 4);
            this.manager.Rebuild();
        }


        public void Click_AddSectionAfter(InteractableRegion region)
        {
            this.manager.project.InsertSection(this.timeRange.End, this.manager.project.WholeNoteDuration * 4);
            this.manager.Rebuild();
        }


        public void Draw(Graphics g)
        {
            foreach (var track in this.trackSegments)
                track.Draw(g);
        }


        public void DrawOverlay(Graphics g)
        {
            if (this.resizeEndTime != this.timeRange.End)
            {
                var overlayStartTime = (this.resizeEndTime < this.timeRange.End) ?
                    this.resizeEndTime : this.timeRange.End;

                var overlayEndTime = (this.resizeEndTime > this.timeRange.End) ?
                    this.resizeEndTime : this.timeRange.End;

                var overlayStartX =
                    (int)(this.layoutRect.xMin +
                    (overlayStartTime - this.timeRange.Start) * this.manager.TimeToPixelsMultiplier);

                var overlayEndX =
                    (int)(this.layoutRect.xMin +
                    (overlayEndTime - this.timeRange.Start) * this.manager.TimeToPixelsMultiplier);

                using (var brush = new SolidBrush(Color.FromArgb(180, Color.White)))
                {
                    g.FillRectangle(brush,
                        overlayStartX + 1,
                        (int)this.layoutRect.yMin + ADD_SECTION_BUTTON_SIZE + ADD_SECTION_BUTTON_MARGIN * 2,
                        overlayEndX - overlayStartX,
                        (int)this.layoutRect.ySize - ADD_SECTION_BUTTON_SIZE - ADD_SECTION_BUTTON_MARGIN * 2);
                }
            }

            var segmentHandleSelected = this.manager.currentDraggingIsolatedRegion == this.regionSectionHandle;
            var segmentHandleHovering = this.manager.currentHoverRegion == this.regionSectionHandle;

            var endX = (int)(this.layoutRect.xMin +
                (this.resizeEndTime - this.timeRange.Start) * this.manager.TimeToPixelsMultiplier);

            using (var pen = new Pen(
                segmentHandleSelected ? Color.Gray :
                segmentHandleHovering ? Color.DarkGray : Color.Black,
                3))
            {
                g.DrawLine(pen,
                    endX, (int)this.layoutRect.yMin + ADD_SECTION_BUTTON_SIZE + ADD_SECTION_BUTTON_MARGIN * 2,
                    endX, (int)this.layoutRect.yMax);

                g.FillRectangle(
                    segmentHandleSelected ? Brushes.Gray :
                    segmentHandleHovering ? Brushes.DarkGray : Brushes.Black,
                    endX - 5, (int)this.layoutRect.yMin + ADD_SECTION_BUTTON_SIZE + ADD_SECTION_BUTTON_MARGIN * 2,
                    11, 11);
            }

            DrawAddSectionButton(g, this.layoutRect.xMin, this.layoutRect.yMin + ADD_SECTION_BUTTON_MARGIN,
                this.manager.currentDraggingIsolatedRegion == this.regionAddSectionBeforeButton,
                this.manager.currentHoverRegion == this.regionAddSectionBeforeButton);

            if (this.isLastRow)
                DrawAddSectionButton(g, this.layoutRect.xMin, this.layoutRect.yMax + ADD_SECTION_BUTTON_MARGIN,
                    this.manager.currentDraggingIsolatedRegion == this.regionAddSectionAfterButton,
                    this.manager.currentHoverRegion == this.regionAddSectionAfterButton);
        }


        private void DrawAddSectionButton(Graphics g, float x, float y, bool selected, bool hovering)
        {
            g.FillRectangle(
                selected ? Brushes.DarkCyan :
                hovering ? Brushes.LightCyan : Brushes.Transparent,
                x, y,
                ADD_SECTION_BUTTON_SIZE, ADD_SECTION_BUTTON_SIZE);

            g.DrawRectangle(
                Pens.Black,
                x, y,
                ADD_SECTION_BUTTON_SIZE, ADD_SECTION_BUTTON_SIZE);

            using (var pen = new Pen(Color.Green, 3))
            {
                g.DrawLine(pen,
                    x + 4, y + ADD_SECTION_BUTTON_SIZE / 2,
                    x + ADD_SECTION_BUTTON_SIZE - 3, y + ADD_SECTION_BUTTON_SIZE / 2);
                g.DrawLine(pen,
                    x + ADD_SECTION_BUTTON_SIZE / 2, y + 4,
                    x + ADD_SECTION_BUTTON_SIZE / 2, y + ADD_SECTION_BUTTON_SIZE - 3);
            }
        }


        public float DrawnYMax
        {
            get
            {
                return this.trackSegments[this.trackSegments.Count - 1].layoutRect.yMax;
            }
        }
    }
}
