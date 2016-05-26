using System.Collections.Generic;


namespace Composer.Util
{
    // FIXME: Unoptimized trivial implementation.
    public class SortedList<T> : IList<T>
    {
        List<T> internalList;
        System.Func<T, T, int> comparerFunc;


        public SortedList(System.Func<T, T, int> comparerFunc)
        {
            this.internalList = new List<T>();
            this.comparerFunc = comparerFunc;
        }


        public void Sort()
        {
            this.internalList.Sort((a, b) => this.comparerFunc(a, b));
        }


        public SortedList<T> Clone()
        {
            var clone = new SortedList<T>(this.comparerFunc);
            clone.AddRange(this);
            return clone;
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
