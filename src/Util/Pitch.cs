﻿namespace Composer.Util
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
            set { this.midiPitch = value; }
        }


        public float Frequency
        {
            get { return (float)System.Math.Pow(2, (this.midiPitch - 69) / 12f) * 440f; }
        }


        public Pitch OffsetMidiPitchBy(float amount)
        {
            return FromMidiPitch(this.midiPitch + amount);
        }
    }
}
