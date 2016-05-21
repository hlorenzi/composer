namespace Composer.Editor
{
    class InteractableRegion
    {
        public enum Kind
        {
            Select,
            MoveAll,
            StretchHorizontal
        }


        public Kind kind;
        public Util.Rect rect;


        public InteractableRegion(Kind kind, Util.Rect boundingBox)
        {
            this.kind = kind;
            this.rect = boundingBox;
        }
    }
}
