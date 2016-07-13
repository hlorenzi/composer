namespace Composer.AudioOut
{
    public class JobNotePreview : Job
    {
        float frequency = 440;
        float phase = 0;
        int timer = 0;


        public JobNotePreview(float frequency)
        {
            this.frequency = frequency;
        }


        public override int GetNextSamples(float[] sampleBuffer)
        {
            for (var i = 0; i < sampleBuffer.Length; i++)
            {
                timer++;
                phase += (float)(1f / 44100f * 2f * System.Math.PI * frequency);

                var volume = 1 - (timer / 11000f);
                if (volume <= 0)
                    return i;

                var wave = (float)System.Math.Sin(phase);
                sampleBuffer[i] += (wave > 0.25f) ? volume : -volume;
            }

            return sampleBuffer.Length;
        }
    }
}
