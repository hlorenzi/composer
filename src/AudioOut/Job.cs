namespace Composer.AudioOut
{
    public abstract class Job
    {
        public abstract int GetNextSamples(float[] sampleBuffer);
    }
}
