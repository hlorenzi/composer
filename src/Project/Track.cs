namespace Composer.Project
{
    public abstract class Track
    {
        public string name;
        public bool visible = true;


        public abstract void InsertEmptySpace(float startTime, float duration);

        public abstract void CutRange(Util.TimeRange timeRange);
    }
}
