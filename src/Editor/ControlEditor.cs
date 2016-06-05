using System;
using System.Collections.Generic;
using System.Windows.Forms;


namespace Composer.Editor
{
    class ControlEditor : Control
    {
        FormMain ownerFormMain;
        public ViewManager viewManager;


        public ControlEditor(FormMain owner)
        {
            this.ownerFormMain = owner;
            this.viewManager = new ViewManager(this, owner.currentProject);

            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
        }


        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            this.viewManager.SetSize(this.Width, this.Height);
            this.viewManager.Rebuild();
        }


        public void Rebuild()
        {
            this.viewManager.Rebuild();
            this.Refresh();
        }


        protected override void OnMouseMove(MouseEventArgs e)
        {
            this.viewManager.OnMouseMove(e.X, e.Y);
            this.Refresh();
        }


        protected override void OnMouseDown(MouseEventArgs e)
        {
            this.viewManager.OnMouseDown(
                e.Button != MouseButtons.Left,
                e.X, e.Y,
                ModifierKeys == Keys.Control, ModifierKeys == Keys.Shift);
            this.Refresh();
        }


        protected override void OnMouseUp(MouseEventArgs e)
        {
            this.viewManager.OnMouseUp(e.X, e.Y);
            this.Refresh();
        }


        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.FillRectangle(System.Drawing.Brushes.White, 0, 0, Width, Height);
            this.viewManager.Draw(e.Graphics);
            base.OnPaint(e);
        }
    }
}
