namespace Composer.Util
{
    public enum RelativePitch
    {
        C, Cs, D, Ds, E, F, Fs, G, Gs, A, As, B, None
    }


    public static class RelativePitchData
    { 
        public static RelativePitch MakeFromPitch(Util.Pitch pitch)
        {
            var midiPitch = ((pitch.MidiPitch % 12f) + 12) % 12f;
            if (midiPitch % 1f != 0)
                return RelativePitch.None;

            return (RelativePitch)(int)midiPitch;
        }


        public static RelativePitch DisplaceBy(this RelativePitch relativePitch, RelativePitch otherPitch)
        {
            return (RelativePitch)(((int)relativePitch + 12 - (int)otherPitch) % 12);
        }


        public static string GetSimpleName(this RelativePitch relativePitch)
        {
            switch (relativePitch)
            {
                case RelativePitch.C:  return "C";
                case RelativePitch.Cs: return "C#";
                case RelativePitch.D:  return "D";
                case RelativePitch.Ds: return "D#";
                case RelativePitch.E:  return "E";
                case RelativePitch.F:  return "F";
                case RelativePitch.Fs: return "F#";
                case RelativePitch.G:  return "G";
                case RelativePitch.Gs: return "G#";
                case RelativePitch.A:  return "A";
                case RelativePitch.As: return "A#";
                case RelativePitch.B:  return "B";
                default: return "?";
            }
        }
    }
}
