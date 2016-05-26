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

            var project = new Project.Project();
            var track = new Project.TrackPitchedNotes();
            project.tracks.Add(track);

            var noteList = (IList<Project.PitchedNote>)track.notes;
            noteList.Add(new Project.PitchedNote { pitch = Util.Pitch.FromMidiPitch(0), timeRange = new Util.TimeRange(0, project.WholeNoteDuration / 4) });
            noteList.Add(new Project.PitchedNote { pitch = Util.Pitch.FromMidiPitch(1), timeRange = new Util.TimeRange(project.WholeNoteDuration / 4, project.WholeNoteDuration / 4 * 2) });
            noteList.Add(new Project.PitchedNote { pitch = Util.Pitch.FromMidiPitch(2), timeRange = new Util.TimeRange(project.WholeNoteDuration / 4 * 2, project.WholeNoteDuration / 4 * 3) });
            noteList.Add(new Project.PitchedNote { pitch = Util.Pitch.FromMidiPitch(3), timeRange = new Util.TimeRange(project.WholeNoteDuration / 4 * 3, project.WholeNoteDuration / 4 * 4) });
            noteList.Add(new Project.PitchedNote { pitch = Util.Pitch.FromMidiPitch(3), timeRange = new Util.TimeRange(project.WholeNoteDuration / 4 * 4, project.WholeNoteDuration * 9) });

            Application.Run(new FormMain(project));
        }
    }
}
