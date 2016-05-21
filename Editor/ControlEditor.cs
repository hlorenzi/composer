using System.Collections.Generic;
using System.Windows.Forms;


namespace Composer.Editor
{
    class ControlEditor : Control
    {
        FormMain ownerFormMain;
        ViewManager viewManager;


        public ControlEditor(FormMain owner)
        {
            this.ownerFormMain = owner;
            this.viewManager = new ViewManager(this, owner.currentProject);

            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
        }


        public void Rebuild()
        {
            this.viewManager.RebuildTracks();
            this.viewManager.RefreshTracks();
            this.viewManager.RebuildElements();
            this.viewManager.RefreshElements();
        }


        protected override void OnMouseMove(MouseEventArgs e)
        {
            this.viewManager.OnMouseMove(e.X, e.Y);
            base.OnMouseMove(e);
        }


        protected override void OnMouseDown(MouseEventArgs e)
        {
            this.viewManager.OnMouseDown(e.X, e.Y, ModifierKeys == Keys.Control, ModifierKeys == Keys.Shift);
            base.OnMouseDown(e);
        }


        protected override void OnMouseUp(MouseEventArgs e)
        {
            this.viewManager.OnMouseUp(e.X, e.Y);
            base.OnMouseDown(e);
        }


        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.FillRectangle(System.Drawing.Brushes.White, 0, 0, Width, Height);
            this.viewManager.Draw(e.Graphics);
            base.OnPaint(e);
        }
    }
}
