using System.Collections.Generic;
using System.Drawing;


namespace Composer.Editor
{
    class Row
    {
        ViewManager manager;
        public Util.TimeRange timeRange;
        public float resizeEndTime;
        public List<TrackSegment> trackSegments;
        public Util.Rect layoutRect;
        public List<InteractableRegion> interactableRegions;


        public Row(ViewManager manager, Util.TimeRange timeRange)
        {
            this.manager = manager;
            this.timeRange = timeRange;
            this.resizeEndTime = timeRange.End;
            this.trackSegments = new List<TrackSegment>();
            this.interactableRegions = new List<InteractableRegion>();
        }


        public void Rebuild(float x, float y)
        {
            this.resizeEndTime = timeRange.End;

            this.layoutRect = new Util.Rect(x, y, x, y + 16);

            foreach (var track in this.trackSegments)
            {
                track.Rebuild(x, this.layoutRect.yMax);
                this.layoutRect = this.layoutRect.Include(track.layoutRect);
            }

            this.interactableRegions.Clear();

            var handle = new InteractableRegion(
                InteractableRegion.CursorKind.MoveHorizontal,
                new Util.Rect(
                    this.layoutRect.xMax - 5, this.layoutRect.yMin,
                    this.layoutRect.xMax + 5, this.layoutRect.yMin + 10));
            handle.SetIsolated(this.DragStart, this.Drag, this.DragEnd);
            this.interactableRegions.Add(handle);
        }


        public void DragStart(InteractableRegion region)
        {

        }


        public void Drag(InteractableRegion region)
        {
            this.resizeEndTime = System.Math.Max(
                this.timeRange.Start,
                this.timeRange.End + this.manager.DragTimeOverflowOffset);
        }


        public void DragEnd(InteractableRegion region)
        {
            this.manager.project.SetLength(this.resizeEndTime);
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
                    (int)(this.layoutRect.xMin + overlayStartTime * this.manager.TimeToPixelsMultiplier);

                var overlayEndX =
                    (int)(this.layoutRect.xMin + overlayEndTime * this.manager.TimeToPixelsMultiplier);

                using (var brush = new SolidBrush(Color.FromArgb(180, Color.White)))
                {
                    g.FillRectangle(brush,
                        overlayStartX + 1, (int)this.layoutRect.yMin + 16,
                        overlayEndX - overlayStartX, (int)this.layoutRect.ySize);
                }
            }

            var selected = this.manager.currentDraggingIsolatedRegion == this.interactableRegions[0];
            var hovering = this.manager.currentHoverRegion == this.interactableRegions[0];

            var endX = (int)(this.layoutRect.xMin +
                (this.resizeEndTime - this.timeRange.Start) * this.manager.TimeToPixelsMultiplier);

            using (var pen = new Pen(
                selected ? Color.Aquamarine : hovering ? Color.MediumAquamarine : Color.Black,
                3))
            {
                g.DrawLine(pen,
                    endX, (int)this.layoutRect.yMin + 6,
                    endX, (int)this.layoutRect.yMax);

                g.FillRectangle(selected ? Brushes.Aquamarine : hovering ? Brushes.MediumAquamarine : Brushes.Black,
                    endX - 5, (int)this.layoutRect.yMin,
                    11, 11);
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
