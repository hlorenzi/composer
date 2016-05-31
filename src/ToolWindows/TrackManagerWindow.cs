using System.Linq;
using System.Collections.Generic;
using System.Windows.Forms;


namespace Composer.ToolWindows
{
    class TrackManagerWindow : Control
    {
        FormMain owner;

        TableLayoutPanel mainLayout;
        ListBox list;

        public TrackManagerWindow(FormMain owner)
        {
            this.owner = owner;
            this.SuspendLayout();

            this.mainLayout = new TableLayoutPanel();
            this.mainLayout.Dock = DockStyle.Fill;
            this.mainLayout.ColumnCount = 1;
            this.mainLayout.ColumnStyles.Add(new ColumnStyle());
            this.mainLayout.RowCount = 3;
            this.mainLayout.RowStyles.Add(new RowStyle());
            this.mainLayout.RowStyles.Add(new RowStyle());
            this.mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var cb1 = new CheckBox();
            cb1.AutoSize = true;
            cb1.Dock = DockStyle.Fill;
            cb1.Anchor = AnchorStyles.Left;
            cb1.Text = "Section View";
            cb1.Checked = true;
            cb1.Enabled = false;
            this.mainLayout.Controls.Add(cb1, 0, 0);

            var toolStrip = new ToolStrip();
            this.mainLayout.Controls.Add(toolStrip, 0, 1);

            var btTrackAdd = toolStrip.Items.Add("+");
            btTrackAdd.ToolTipText = "Add Track";
            btTrackAdd.Click += (sender, args) => AddPitchedNoteTrack();

            var btTrackRemove = toolStrip.Items.Add("x");
            btTrackRemove.ToolTipText = "Remove Selected Tracks";
            btTrackRemove.Click += (sender, args) => RemoveSelectedTracks();

            toolStrip.Items.Add("-");

            var btTrackShow = toolStrip.Items.Add("Show");
            btTrackShow.ToolTipText = "Show Selected Tracks";
            btTrackShow.Click += (sender, args) => SetVisibilityOfSelectedTracks(true);

            var btTrackHide = toolStrip.Items.Add("Hide");
            btTrackHide.ToolTipText = "Hide Selected Tracks";
            btTrackHide.Click += (sender, args) => SetVisibilityOfSelectedTracks(false);

            var btTrackMoveUp = toolStrip.Items.Add("^");
            btTrackMoveUp.ToolTipText = "Move Selected Tracks Up in List";
            btTrackMoveUp.Click += (sender, args) => MoveSelectedTracks(true);

            var btTrackMoveDown = toolStrip.Items.Add("v");
            btTrackMoveDown.ToolTipText = "Move Selected Tracks Down in List";
            btTrackMoveDown.Click += (sender, args) => MoveSelectedTracks(false);


            this.list = new ListBox();
            this.list.Dock = DockStyle.Fill;
            this.list.SelectionMode = SelectionMode.MultiExtended;
            this.list.DoubleClick += (sender, args) => OnDoubleClickTrackVisibilityToggle();
            this.mainLayout.Controls.Add(this.list, 0, 2);

            this.Controls.Add(this.mainLayout);
            this.ResumeLayout(true);
        }


        public void RefreshTracks()
        {
            var selectedIndices = new List<int>();
            for (var i = 0; i < this.list.SelectedIndices.Count; i++)
                selectedIndices.Add(this.list.SelectedIndices[i]);

            this.list.BeginUpdate();

            // Add new items before removing old items to prevent
            // scrollbar from moving.
            var originalItemCount = this.list.Items.Count;

            for (var i = 0; i < this.owner.currentProject.tracks.Count; i++)
            {
                var track = this.owner.currentProject.tracks[i];
                this.list.Items.Add(track.name + (track.visible ? "" : " (hidden)"));
            }

            for (var i = 0; i < originalItemCount; i++)
                this.list.Items.RemoveAt(0);

            for (var i = 0; i < selectedIndices.Count; i++)
                this.list.SetSelected(selectedIndices[i], true);

            this.list.EndUpdate();
        }


        public void SelectTrack(int index)
        {
            this.list.ClearSelected();
            this.list.SelectedIndex = index;
        }


        public void OnDoubleClickTrackVisibilityToggle()
        {
            if (this.list.SelectedIndex >= 0)
            {
                var track = this.owner.currentProject.tracks[this.list.SelectedIndex];
                track.visible = !track.visible;

                this.owner.editor.Rebuild();
                this.RefreshTracks();
                this.owner.Refresh();
            }
        }


        private void AddPitchedNoteTrack()
        {
            var newIndex = this.owner.currentProject.tracks.Count;
            var newTrack = new Project.TrackPitchedNotes("Track " + (newIndex + 1));
            this.owner.currentProject.tracks.Add(newTrack);

            this.owner.editor.Rebuild();
            this.RefreshTracks();
            this.SelectTrack(newIndex);
            this.owner.Refresh();
        }


        private void RemoveSelectedTracks()
        {
            for (var i = this.list.SelectedIndices.Count - 1; i >= 0; i--)
                this.owner.currentProject.tracks.RemoveAt(this.list.SelectedIndices[i]);

            this.list.ClearSelected();
            this.owner.editor.Rebuild();
            this.RefreshTracks();
            this.owner.Refresh();
        }


        private void SetVisibilityOfSelectedTracks(bool visible)
        {
            for (var i = 0; i < this.list.SelectedIndices.Count; i++)
            {
                var track = this.owner.currentProject.tracks[this.list.SelectedIndices[i]];
                track.visible = visible;
            }

            this.owner.editor.Rebuild();
            this.RefreshTracks();
            this.owner.Refresh();
        }


        private void MoveSelectedTracks(bool up)
        {
            var selectedIndices = new List<int>();
            for (var i = 0; i < this.list.SelectedIndices.Count; i++)
                selectedIndices.Add(this.list.SelectedIndices[i]);

            this.list.ClearSelected();

            if (up)
            {
                for (var i = 0; i < selectedIndices.Count; i++)
                {
                    var index = selectedIndices[i];
                    if (index <= 0)
                        continue;

                    var track = this.owner.currentProject.tracks[index];
                    this.owner.currentProject.tracks.RemoveAt(index);
                    this.owner.currentProject.tracks.Insert(index - 1, track);
                }

                for (var i = 0; i < selectedIndices.Count; i++)
                {
                    var index = selectedIndices[i];
                    if (index <= 0)
                        continue;

                    this.list.SetSelected(index - 1, true);
                }
            }
            else
            {
                for (var i = selectedIndices.Count - 1; i >= 0; i--)
                {
                    var index = selectedIndices[i];
                    if (index >= this.owner.currentProject.tracks.Count - 1)
                        continue;

                    var track = this.owner.currentProject.tracks[index];
                    this.owner.currentProject.tracks.RemoveAt(index);
                    this.owner.currentProject.tracks.Insert(index + 1, track);
                }

                for (var i = 0; i < selectedIndices.Count; i++)
                {
                    var index = selectedIndices[i];
                    if (index >= this.owner.currentProject.tracks.Count - 1)
                        continue;

                    this.list.SetSelected(index + 1, true);
                }
            }


            this.owner.editor.Rebuild();
            this.RefreshTracks();
            this.owner.Refresh();
        }
    }
}
