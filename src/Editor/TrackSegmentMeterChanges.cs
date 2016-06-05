using System.Collections.Generic;
using System.Drawing;


namespace Composer.Editor
{
    class TrackSegmentMeterChanges : TrackSegment
    {
        public List<Project.MeterChange> affectingMeterChanges;


        public TrackSegmentMeterChanges(
            ViewManager manager,
            Row row)
            : base(manager, row)
        {
            this.affectingMeterChanges = new List<Project.MeterChange>();
        }


        public override void RefreshLayout(float x, float y)
        {
            this.affectingMeterChanges.Clear();

            foreach (var meterChange in this.manager.project.meterChanges.EnumerateAffectingRange(this.row.timeRange))
                this.affectingMeterChanges.Add(meterChange);

            this.layoutRect = new Util.Rect(
                x,
                y,
                x + this.row.timeRange.Duration * this.manager.TimeToPixelsMultiplier,
                y + ElementMeterChange.HANDLE_HEIGHT + 2);

            this.contentRect = this.layoutRect.Clone();
        }


        public override float GetTimeAtPosition(float x)
        {
            return this.row.timeRange.Start + (x - this.layoutRect.xMin) / this.manager.TimeToPixelsMultiplier;
        }


        public override Util.Pitch GetPitchAtPosition(float y)
        {
            return Util.Pitch.FromMidiPitch(0);
        }


        public override void Draw(Graphics g)
        {

        }
    }
}
