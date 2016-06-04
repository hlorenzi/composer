using System;
using System.Windows.Forms;
using System.Collections.Generic;


namespace Composer
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var project = new Project.Project(960 * 4);
            project.tracks.Add(new Project.TrackPitchedNotes("Track 1"));

            project.InsertKeyChange(new Project.KeyChange(0, new Util.Key(Util.RelativePitch.C, Util.Scale.Major)));
            project.InsertKeyChange(new Project.KeyChange(960 * 3, new Util.Key(Util.RelativePitch.Fs, Util.Scale.Mixolydian)));
            project.InsertMeterChange(new Project.MeterChange(0, new Util.Meter(4, 4)));
            project.InsertMeterChange(new Project.MeterChange(960, new Util.Meter(3, 4)));
            project.InsertMeterChange(new Project.MeterChange(960 * 2, new Util.Meter(6, 8)));

            project.InsertPitchedNote(0, new Project.PitchedNote { pitch = Util.Pitch.FromMidiPitch(60), timeRange = new Util.TimeRange(960 / 4 * 0, 960 / 4 * 1) });
            project.InsertPitchedNote(0, new Project.PitchedNote { pitch = Util.Pitch.FromMidiPitch(61), timeRange = new Util.TimeRange(960 / 4 * 1, 960 / 4 * 2) });
            project.InsertPitchedNote(0, new Project.PitchedNote { pitch = Util.Pitch.FromMidiPitch(62), timeRange = new Util.TimeRange(960 / 4 * 2, 960 / 4 * 3) });
            project.InsertPitchedNote(0, new Project.PitchedNote { pitch = Util.Pitch.FromMidiPitch(63), timeRange = new Util.TimeRange(960 / 4 * 3, 960 / 4 * 4) });
            project.InsertPitchedNote(0, new Project.PitchedNote { pitch = Util.Pitch.FromMidiPitch(64), timeRange = new Util.TimeRange(960 / 4 * 4, 960 / 4 * 5) });

            Application.Run(new FormMain(project));
        }
    }
}
