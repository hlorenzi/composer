using System.Collections.Generic;


namespace Composer.Util
{
    // FIXME: Unoptimized trivial implementation.
    public class TimeRangeSortedList<T> : IList<T>
    {
        List<T> internalList;
        System.Func<T, TimeRange> getTimeRangeFunc;


        public TimeRangeSortedList(System.Func<T, TimeRange> getTimeRangeFunc)
        {
            this.internalList = new List<T>();
            this.getTimeRangeFunc = getTimeRangeFunc;
        }


        public IEnumerable<T> EnumerateInsideRange(Util.TimeRange timeRange)
        {
            foreach (var item in this.internalList)
            {
                if (this.getTimeRangeFunc(item).Overlaps(timeRange))
                    yield return item;
            }
        }


        void IList<T>.Insert(int index, T item)
        { 
            this.internalList.Insert(index, item);
            this.internalList.Sort((a, b) =>
            {
                var order = this.getTimeRangeFunc(a).Start - getTimeRangeFunc(b).Start;
                if (order < 0) return -1;
                if (order > 0) return 1;
                return 0;
            });
        }


        void IList<T>.RemoveAt(int index)
        {
            this.internalList.RemoveAt(index);
        }


        int IList<T>.IndexOf(T item)
        {
            return this.internalList.IndexOf(item);
        }


        T IList<T>.this[int index]
        {
            get { return this.internalList[index]; }
            set { this.internalList[index] = value; }
        }


        void ICollection<T>.Add(T item)
        {
            this.internalList.Add(item);
            this.internalList.Sort((a, b) =>
            {
                var order = this.getTimeRangeFunc(a).Start - getTimeRangeFunc(b).Start;
                if (order < 0) return -1;
                if (order > 0) return 1;
                return 0;
            });
        }


        bool ICollection<T>.Remove(T item)
        {
            return this.internalList.Remove(item);
        }


        int ICollection<T>.Count
        {
            get { return this.internalList.Count; }
        }


        void ICollection<T>.Clear()
        {
            this.internalList.Clear();
        }


        bool ICollection<T>.Contains(T item)
        {
            return this.internalList.Contains(item);
        }


        bool ICollection<T>.IsReadOnly
        {
            get { return false; }
        }


        void ICollection<T>.CopyTo(T[] array, int arrayIndex)
        {
            this.internalList.CopyTo(array, arrayIndex);
        }


        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return this.internalList.GetEnumerator();
        }


        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.internalList.GetEnumerator();
        }
    }
}
