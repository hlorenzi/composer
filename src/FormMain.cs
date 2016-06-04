using System.Windows.Forms;


namespace Composer
{
    class FormMain : Form
    {
        public Project.Project currentProject;
        public Editor.ControlEditor editorControl;
        public Editor.ViewManager editor;


        public FormMain(Project.Project project)
        {
            this.currentProject = project;

            SuspendLayout();

            var menuStrip = new MenuStrip();
            menuStrip.Items.Add("File");
            menuStrip.Items.Add("Edit");
            menuStrip.Items.Add("View");

            var toolStrip = new ToolStrip();
            toolStrip.Items.Add("Insert Key Change");
            toolStrip.Items.Add("Insert Meter Change");

            var split = new SplitContainer();
            split.Dock = DockStyle.Fill;

            var trackManager = new ToolWindows.TrackManagerWindow(this);
            trackManager.Dock = DockStyle.Fill;
            split.Panel1.Controls.Add(trackManager);

            this.editorControl = new Editor.ControlEditor(this);
            this.editorControl.Dock = DockStyle.Fill;
            split.Panel2.Controls.Add(this.editorControl);
            this.editor = this.editorControl.viewManager;

            trackManager.RefreshTracks();

            this.Controls.Add(split);
            this.Controls.Add(toolStrip);
            this.Controls.Add(menuStrip);
            this.MainMenuStrip = menuStrip;

            this.Width = 900;
            this.Height = 600;

            ResumeLayout(false);
            PerformLayout();

            split.SplitterDistance = 200;
            editor.Rebuild();
            Refresh();
        }


        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            var ctrlKey = (keyData & Keys.Control) != 0;
            var shiftKey = (keyData & Keys.Shift) != 0;

            keyData = (keyData & ~(Keys.Control | Keys.Shift));

            if (keyData == Keys.Z ||
                keyData == Keys.X || keyData == Keys.C ||
                keyData == Keys.V || keyData == Keys.B || keyData == Keys.N ||
                keyData == Keys.M ||
                keyData == Keys.S || keyData == Keys.D ||
                keyData == Keys.G || keyData == Keys.H || keyData == Keys.J)
            {
                int trackIndex;
                float time;
                if (this.editor.GetInsertionPosition(out trackIndex, out time))
                {
                    var relativePitch = Util.RelativePitch.C;
                    if (keyData == Keys.Z) relativePitch = Util.RelativePitch.C;
                    if (keyData == Keys.S) relativePitch = Util.RelativePitch.Cs;
                    if (keyData == Keys.X) relativePitch = Util.RelativePitch.D;
                    if (keyData == Keys.D) relativePitch = Util.RelativePitch.Ds;
                    if (keyData == Keys.C) relativePitch = Util.RelativePitch.E;
                    if (keyData == Keys.V) relativePitch = Util.RelativePitch.F;
                    if (keyData == Keys.G) relativePitch = Util.RelativePitch.Fs;
                    if (keyData == Keys.B) relativePitch = Util.RelativePitch.G;
                    if (keyData == Keys.H) relativePitch = Util.RelativePitch.Gs;
                    if (keyData == Keys.N) relativePitch = Util.RelativePitch.A;
                    if (keyData == Keys.J) relativePitch = Util.RelativePitch.As;
                    if (keyData == Keys.M) relativePitch = Util.RelativePitch.B;

                    var pitch = Util.RelativePitchData.GetPitch(relativePitch, 5);
                    this.InsertPitchedNote(trackIndex, time, 960 / 4, pitch);
                }

                return true;
            }

            else if (keyData == Keys.Up) { this.editor.OnPressUp(ctrlKey, shiftKey); return true; }
            else if (keyData == Keys.Down) { this.editor.OnPressDown(ctrlKey, shiftKey); return true; }
            else if (keyData == Keys.Right)
            {
                this.editor.OnPressRight(ctrlKey, shiftKey);
                var currentNote = this.editor.GetNoteInsertionModeNote();
                if (currentNote != null)
                {
                    this.editor.SetCursorTimeRange(currentNote.timeRange.End, currentNote.timeRange.End);
                    this.editorControl.Refresh();
                }
                return true;
            }
            else if (keyData == Keys.Left)
            {
                this.editor.OnPressLeft(ctrlKey, shiftKey);
                var currentNote = this.editor.GetNoteInsertionModeNote();
                if (currentNote != null)
                {
                    this.editor.SetCursorTimeRange(currentNote.timeRange.End, currentNote.timeRange.End);
                    this.editorControl.Refresh();
                }
                return true;
            }

            else if (keyData == Keys.Return)
            {
                this.editor.UnselectAll();
                this.editorControl.Refresh();
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }


        private void InsertPitchedNote(int trackIndex, float time, float duration, Util.Pitch pitch)
        {
            this.editor.UnselectAll();

            var note = new Project.PitchedNote
            {
                pitch = pitch,
                timeRange = new Util.TimeRange(time, time + duration)
            };

            this.currentProject.InsertPitchedNote(trackIndex, note);

            this.editor.Rebuild();
            this.editor.SetPitchedNoteSelection(trackIndex, note, true);
            this.editor.SetNoteInsertionMode(true);
            this.editor.SetCursorTimeRange(time + duration, time + duration);
            this.editor.SetCursorVisible(true);

            this.Refresh();
        }
    }
}
