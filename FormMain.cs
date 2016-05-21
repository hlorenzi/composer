using System.Windows.Forms;


namespace Composer
{
    class FormMain : Form
    {
        public Project.Project currentProject;


        public FormMain(Project.Project project)
        {
            this.currentProject = project;

            SuspendLayout();

            var menuStrip = new MenuStrip();
            menuStrip.Items.Add("File");
            menuStrip.Items.Add("Edit");
            menuStrip.Items.Add("View");

            var editor = new Editor.ControlEditor(this);
            editor.Dock = DockStyle.Fill;

            this.Controls.Add(editor);
            this.Controls.Add(menuStrip);
            this.MainMenuStrip = menuStrip;

            this.Width = 900;
            this.Height = 600;

            ResumeLayout(false);
            PerformLayout();

            editor.Rebuild();
            Refresh();
        }
    }
}
