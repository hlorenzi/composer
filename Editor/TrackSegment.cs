using System.Drawing;


namespace Composer.Editor
{
    abstract class TrackSegment
    {
        public ViewManager manager;
        public Row row;
        public Util.Rect layoutRect;


        public TrackSegment(ViewManager manager, Row row)
        {
            this.manager = manager;
            this.row = row;
        }


        public abstract void Rebuild(float x, float y);

        public abstract float GetTimeAtPosition(float x, float y);

        public abstract Util.Pitch GetPitchAtPosition(float x, float y);

        public abstract void Draw(Graphics g);
    }
}
