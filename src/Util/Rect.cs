namespace Composer.Util
{
    public class Rect
    {
        public float xMin, yMin, xMax, yMax;


        public Rect(float xMin, float yMin, float xMax, float yMax)
        {
            this.xMin = xMin;
            this.yMin = yMin;
            this.xMax = xMax;
            this.yMax = yMax;
        }


        public float xSize
        {
            get { return this.xMax - this.xMin; }
        }


        public float ySize
        {
            get { return this.yMax - this.yMin; }
        }


        public bool Contains(float x, float y)
        {
            return x >= this.xMin && y >= this.yMin && x <= this.xMax && y <= this.yMax;
        }


        public bool ContainsX(float x)
        {
            return x >= this.xMin && x <= this.xMax;
        }


        public bool ContainsY(float y)
        {
            return y >= this.yMin && y <= this.yMax;
        }


        public Rect Expand(float amount)
        {
            return new Rect(
                xMin - amount,
                yMin - amount,
                xMax + amount,
                yMax + amount);
        }


        public Rect Include(Rect other)
        {
            return new Rect(
                System.Math.Min(this.xMin, other.xMin),
                System.Math.Min(this.yMin, other.yMin),
                System.Math.Max(this.xMax, other.xMax),
                System.Math.Max(this.yMax, other.yMax));
        }
    }
}
