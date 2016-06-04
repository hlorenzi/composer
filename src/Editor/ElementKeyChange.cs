using System.Collections.Generic;
using System.Drawing;


namespace Composer.Editor
{
    class ElementKeyChange : Element
    {
        Project.KeyChange projectKeyChange;

        Row row;
        float time;
        float timeDragStart;


        public const int HANDLE_WIDTH = 10;
        public const int HANDLE_HEIGHT = 16;


        public ElementKeyChange(
            ViewManager manager,
            Project.KeyChange projectKeyChange)
            : base(manager)
        {
            this.projectKeyChange = projectKeyChange;
            this.interactableRegions = new List<InteractableRegion>();
            this.time = projectKeyChange.time;
        }


        public override void Rebuild()
        {
            this.interactableRegions.Clear();

            var tMult = this.manager.TimeToPixelsMultiplier;

            this.row = this.manager.GetRowOverlapping(this.time);
            if (this.row != null)
            {
                var track = this.row.trackSegmentKeyChanges;
                var timeMinusTrackStart = this.time - this.row.timeRange.Start;

                var handleRect = new Util.Rect(
                    track.contentRect.xMin + tMult * timeMinusTrackStart - HANDLE_WIDTH / 2,
                    track.contentRect.yMin,
                    track.contentRect.xMin + tMult * timeMinusTrackStart + HANDLE_WIDTH / 2,
                    track.contentRect.yMax);

                this.interactableRegions.Add(
                    new InteractableRegion(InteractableRegion.CursorKind.MoveHorizontal, handleRect));
            }
        }


        public override void DragStart()
        {
            this.timeDragStart = this.projectKeyChange.time;
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
            this.manager.project.MoveKeyChange(this.projectKeyChange, this.time);
        }


        public override void Draw(Graphics g, bool hovering, bool selected)
        {
            if (this.row == null)
                return;

            var x = (int)(this.row.layoutRect.xMin +
                (this.time - this.row.timeRange.Start) * this.manager.TimeToPixelsMultiplier);

            using (var pen = new Pen(
                selected ? Color.DarkMagenta :
                hovering ? Color.Violet : Color.MediumVioletRed,
                3))
            {
                g.DrawLine(pen,
                    x, (int)this.row.trackSegmentKeyChanges.contentRect.yMin,
                    x, (int)this.row.contentRect.yMax);

                g.FillRectangle(
                    selected ? Brushes.DarkMagenta :
                    hovering ? Brushes.Violet : Brushes.MediumVioletRed,
                    x - HANDLE_WIDTH / 2, (int)this.row.trackSegmentKeyChanges.contentRect.yMin,
                    HANDLE_WIDTH, HANDLE_HEIGHT);
            }

            using (var font = new Font("Verdana", HANDLE_HEIGHT / 2))
            {
                g.DrawString(
                    this.projectKeyChange.GetDisplayString(),
                    font,
                    Brushes.MediumVioletRed,
                    x + HANDLE_HEIGHT / 2, (int)this.row.trackSegmentKeyChanges.contentRect.yMin);
            }
        }
    }
}
