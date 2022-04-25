using System.Collections.Generic;

namespace ChatProject_v2
{
    public class ThreadSafeList<T>
    {
        private object _locker = new object();

        private List<T> _list = new List<T>();

        public int Count
        {
            get
            {
                lock(_locker)
                {
                    return _list.Count;
                }
            }
        }

        public void RemoveAt(int index)
        {
            lock (_locker)
            {
                _list.RemoveAt(index);
            }
        }

        public void Add(T item)
        {
            lock (_locker)
            {
                _list.Add(item);
            }
        }
    }
}
