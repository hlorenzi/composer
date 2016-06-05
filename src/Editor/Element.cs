using System.Collections.Generic;
using System.Drawing;


namespace Composer.Editor
{
    abstract class Element
    {
        public ViewManager manager;
        public List<InteractableRegion> interactableRegions;

        public bool selected;


        public Element(ViewManager manager)
        {
            this.manager = manager;
        }


        public virtual void RefreshLayout()
        {

        }


        public virtual void BeginModify()
        {

        }


        public virtual void EndModify()
        {

        }


        public virtual void OnPressUp(bool ctrlKey, bool shiftKey)
        {

        }


        public virtual void OnPressDown(bool ctrlKey, bool shiftKey)
        {

        }


        public virtual void OnPressRight(bool ctrlKey, bool shiftKey)
        {

        }


        public virtual void OnPressLeft(bool ctrlKey, bool shiftKey)
        {

        }


        public virtual void Drag()
        {

        }


        public virtual void Draw(Graphics g, bool hovering, bool selected)
        {

        }
    }
}
