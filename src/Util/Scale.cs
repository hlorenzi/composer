using static Composer.Util.RelativePitch;


namespace Composer.Util
{
    public enum Scale
    {
        Major,
        Dorian,
        Phrygian,
        Lydian,
        Mixolydian,
        NaturalMinor,
        Locrian,
        MelodicMinor
    }


    public static class ScaleData
    {
        private static readonly string[] names = new string[]
        {
            "Major",
            "Dorian",
            "Phrygian",
            "Lydian",
            "Mixolydian",
            "Natural Minor",
            "Locrian",
            "Melodic Minor"
        };


        private static readonly RelativePitch[][] relativePitches = new RelativePitch[][]
        {
            new RelativePitch[] { C,  D,  E,  F,  G,  A,  B  }, /* Major */ 
            new RelativePitch[] { C,  D,  Ds, F,  G,  A,  As }, /* Dorian */ 
            new RelativePitch[] { C,  Cs, Ds, F,  G,  Gs, As }, /* Phrygian */ 
            new RelativePitch[] { C,  D,  E,  Fs, G,  A,  B  }, /* Lydian */ 
            new RelativePitch[] { C,  D,  E,  F,  G,  A,  As }, /* Mixolydian */ 
            new RelativePitch[] { C,  D,  Ds, F,  G,  Gs, As }, /* NaturalMinor */ 
            new RelativePitch[] { C,  Cs, Ds, F,  G,  Gs, As }, /* Locrian */ 
            new RelativePitch[] { C,  D,  Ds, F,  G,  Gs, B  }, /* MelodicMinor */ 
        };


        private static int[][] relativePitchesToDegree;


        static ScaleData()
        {
            relativePitchesToDegree = new int[relativePitches.Length][];
            for (var i = 0; i < relativePitchesToDegree.Length; i++)
            {
                relativePitchesToDegree[i] = new int[12]
                    { -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 };

                for (var j = 0; j < relativePitches[i].Length; j++)
                    relativePitchesToDegree[i][(int)relativePitches[i][j]] = j;
            }
        }


        public static RelativePitch[] GetRelativePitches(this Scale scale)
        {
            return relativePitches[(int)scale];
        }


        public static string GetName(this Scale scale)
        {
            return names[(int)scale];
        }


        public static bool HasRelativePitch(this Scale scale, RelativePitch relativePitch)
        {
            return relativePitchesToDegree[(int)scale][(int)relativePitch] >= 0;
        }
    }
}
