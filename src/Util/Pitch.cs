namespace Composer.Util
{
    public struct Pitch
    {
        float midiPitch;
         

        public static Pitch FromMidiPitch(float midiPitch)
        {
            return new Pitch { midiPitch = midiPitch };
        }


        public float MidiPitch
        {
            get { return this.midiPitch; }
        }


        public Pitch OffsetMidiPitchBy(float amount)
        {
            return FromMidiPitch(this.midiPitch + amount);
        }
    }
}
