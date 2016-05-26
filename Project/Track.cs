namespace Composer.Project
{
    public abstract class Track
    {
        public abstract void InsertEmptySpace(float startTime, float duration);

        public abstract void CutRange(Util.TimeRange timeRange);
    }
}
