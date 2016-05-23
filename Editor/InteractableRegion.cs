namespace Composer.Editor
{
    class InteractableRegion
    {
        public enum CursorKind
        {
            Select,
            MoveAll,
            MoveHorizontal
        }


        public CursorKind cursorKind;
        public Util.Rect rect;
        public bool isolated;
        public System.Action<InteractableRegion> dragStartFunc, dragFunc, dragEndFunc;


        public InteractableRegion(CursorKind cursorKind, Util.Rect boundingBox)
        {
            this.cursorKind = cursorKind;
            this.rect = boundingBox;
            this.isolated = false;
        }


        public void SetIsolated(
            System.Action<InteractableRegion> dragStartFunc,
            System.Action<InteractableRegion> dragFunc,
            System.Action<InteractableRegion> dragEndFunc)
        {
            this.isolated = true;
            this.dragStartFunc = dragStartFunc;
            this.dragFunc = dragFunc;
            this.dragEndFunc = dragEndFunc;
        }
    }
}
