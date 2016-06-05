using System.Collections.Generic;
using System.Drawing;


namespace Composer.Editor
{
    class TrackSegmentKeyChanges : TrackSegment
    {
        public List<Project.KeyChange> affectingKeyChanges;


        public TrackSegmentKeyChanges(
            ViewManager manager,
            Row row)
            : base(manager, row)
        {
            this.affectingKeyChanges = new List<Project.KeyChange>();
        }


        public override void RefreshLayout(float x, float y)
        {
            this.affectingKeyChanges.Clear();

            foreach (var keyChange in this.manager.project.keyChanges.EnumerateAffectingRange(this.row.timeRange))
                this.affectingKeyChanges.Add(keyChange);

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
