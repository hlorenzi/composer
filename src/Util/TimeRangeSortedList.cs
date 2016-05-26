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


        public void Sort()
        {
            this.internalList.Sort((a, b) =>
            {
                var order = this.getTimeRangeFunc(a).Start - getTimeRangeFunc(b).Start;
                if (order < 0) return -1;
                if (order > 0) return 1;
                return 0;
            });
        }


        public IEnumerable<T> EnumerateOverlapping(float time)
        {
            foreach (var item in this.internalList)
            {
                if (this.getTimeRangeFunc(item).Overlaps(time))
                    yield return item;
            }
        }


        public IEnumerable<T> EnumerateOverlappingRange(Util.TimeRange timeRange)
        {
            foreach (var item in this.internalList)
            {
                if (this.getTimeRangeFunc(item).OverlapsRange(timeRange))
                    yield return item;
            }
        }


        public IEnumerable<T> EnumerateEntirelyBefore(float time)
        {
            foreach (var item in this.internalList)
            {
                if (this.getTimeRangeFunc(item).End <= time)
                    yield return item;
            }
        }


        public IEnumerable<T> EnumerateEntirelyAfter(float time)
        {
            foreach (var item in this.internalList)
            {
                if (this.getTimeRangeFunc(item).Start >= time)
                    yield return item;
            }
        }


        public void Insert(int index, T item)
        {
            this.internalList.Insert(index, item);
            this.Sort();
        }


        public void RemoveAt(int index)
        {
            this.internalList.RemoveAt(index);
        }


        public int RemoveAll(System.Predicate<T> predicate)
        {
            return internalList.RemoveAll(predicate);
        }


        public void RemoveOverlappingRange(Util.TimeRange timeRange)
        {
            internalList.RemoveAll(item => this.getTimeRangeFunc(item).OverlapsRange(timeRange));
        }


        public int IndexOf(T item)
        {
            return this.internalList.IndexOf(item);
        }


        public T this[int index]
        {
            get { return this.internalList[index]; }
            set { this.internalList[index] = value; }
        }


        public void Add(T item)
        {
            this.internalList.Add(item);
            this.Sort();
        }


        public void AddRange(IEnumerable<T> items)
        {
            this.internalList.AddRange(items);
            this.Sort();
        }


        public bool Remove(T item)
        {
            return this.internalList.Remove(item);
        }


        public int Count
        {
            get { return this.internalList.Count; }
        }


        public void Clear()
        {
            this.internalList.Clear();
        }


        public bool Contains(T item)
        {
            return this.internalList.Contains(item);
        }


        public bool IsReadOnly
        {
            get { return false; }
        }


        public void CopyTo(T[] array, int arrayIndex)
        {
            this.internalList.CopyTo(array, arrayIndex);
        }


        public IEnumerator<T> GetEnumerator()
        {
            return this.internalList.GetEnumerator();
        }


        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.internalList.GetEnumerator();
        }
    }
}
