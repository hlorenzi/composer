using System.Collections.Generic;
using System.Drawing;


namespace Composer.Editor
{
    class ElementMeterChange : Element
    {
        Project.MeterChange projectMeterChange;

        Row row;
        float time;
        float timeDragStart;


        public const int HANDLE_WIDTH = 10;
        public const int HANDLE_HEIGHT = 16;


        public ElementMeterChange(
            ViewManager manager,
            Project.MeterChange projectMeterChange)
            : base(manager)
        {
            this.projectMeterChange = projectMeterChange;
            this.interactableRegions = new List<InteractableRegion>();
            this.time = projectMeterChange.time;
        }


        public override void Rebuild()
        {
            this.interactableRegions.Clear();

            var tMult = this.manager.TimeToPixelsMultiplier;

            this.row = this.manager.GetRowOverlapping(this.time);
            if (this.row != null)
            {
                var track = this.row.trackSegmentMeterChanges;
                var timeMinusTrackStart = this.time - this.row.timeRange.Start;

                var handleRect = new Util.Rect(
                    track.layoutRect.xMin + tMult * timeMinusTrackStart - HANDLE_WIDTH / 2,
                    track.layoutRect.yMin,
                    track.layoutRect.xMin + tMult * timeMinusTrackStart + HANDLE_WIDTH / 2,
                    track.layoutRect.yMax);

                this.interactableRegions.Add(
                    new InteractableRegion(InteractableRegion.CursorKind.MoveHorizontal, handleRect));
            }
        }


        public override void DragStart()
        {
            this.timeDragStart = this.projectMeterChange.time;
        }


        public override void Drag()
        {
            this.time =
                System.Math.Max(0,
                System.Math.Min(this.manager.project.Length,
                this.timeDragStart + this.manager.DragTimeOffsetClampedToRow));
        }


        public override void DragEnd()
        {
            this.manager.project.MoveMeterChange(this.projectMeterChange, this.time);
        }


        public override void Draw(Graphics g, bool hovering, bool selected)
        {
            if (this.row == null)
                return;

            var x = (int)(this.row.layoutRect.xMin +
                (this.time - this.row.timeRange.Start) * this.manager.TimeToPixelsMultiplier);

            using (var pen = new Pen(
                selected ? Color.DarkCyan :
                hovering ? Color.Aquamarine : Color.MediumAquamarine,
                3))
            {
                g.DrawLine(pen,
                    x, (int)this.row.trackSegmentMeterChanges.contentRect.yMin,
                    x, (int)this.row.contentRect.yMax);

                g.FillRectangle(
                    selected ? Brushes.DarkCyan :
                    hovering ? Brushes.Aquamarine : Brushes.MediumAquamarine,
                    x - HANDLE_WIDTH / 2, (int)this.row.trackSegmentMeterChanges.contentRect.yMin,
                    HANDLE_WIDTH, HANDLE_HEIGHT);
            }

            using (var font = new Font("Verdana", HANDLE_HEIGHT / 2))
            {
                g.DrawString(
                    this.projectMeterChange.GetDisplayString(),
                    font,
                    Brushes.MediumAquamarine,
                    x + HANDLE_HEIGHT / 2, (int)this.row.trackSegmentMeterChanges.contentRect.yMin);
            }
        }
    }
}
