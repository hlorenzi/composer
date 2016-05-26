using System.Collections.Generic;


namespace Composer.Util
{
    // FIXME: Unoptimized trivial implementation.
    public class TimeSortedList<T> : IList<T>
    {
        List<T> internalList;
        System.Func<T, float> getTimeFunc;


        public TimeSortedList(System.Func<T, float> getTimeFunc)
        {
            this.internalList = new List<T>();
            this.getTimeFunc = getTimeFunc;
        }


        public void Sort()
        {
            this.internalList.Sort((a, b) =>
            {
                var order = this.getTimeFunc(a) - getTimeFunc(b);
                if (order < 0) return -1;
                if (order > 0) return 1;
                return 0;
            });
        }


        public TimeSortedList<T> Clone()
        {
            var clone = new TimeSortedList<T>(this.getTimeFunc);
            clone.AddRange(this);
            return clone;
        }


        public IEnumerable<T> EnumerateOverlappingRange(Util.TimeRange timeRange)
        {
            foreach (var item in this.internalList)
            {
                if (timeRange.Overlaps(this.getTimeFunc(item)))
                    yield return item;
            }
        }


        public IEnumerable<T> EnumerateAffectingRange(Util.TimeRange timeRange)
        {
            T lastItem = default(T);
            var gotFirst = false;
            var yieldedAny = false;

            foreach (var item in this.internalList)
            {
                var itemTime = this.getTimeFunc(item);
                if (timeRange.Overlaps(itemTime))
                {
                    if (!gotFirst)
                    {
                        gotFirst = true;
                        if (itemTime > timeRange.Start)
                            yield return lastItem;
                    }
                    yieldedAny = true;
                    yield return item;
                }
                lastItem = item;
            }

            if (!yieldedAny)
                yield return lastItem;
        }


        public IEnumerable<T> EnumerateBefore(float time)
        {
            foreach (var item in this.internalList)
            {
                if (this.getTimeFunc(item) <= time)
                    yield return item;
            }
        }


        public IEnumerable<T> EnumerateAtAndAfter(float time)
        {
            foreach (var item in this.internalList)
            {
                if (this.getTimeFunc(item) >= time)
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
            internalList.RemoveAll(item => timeRange.Overlaps(this.getTimeFunc(item)));
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
