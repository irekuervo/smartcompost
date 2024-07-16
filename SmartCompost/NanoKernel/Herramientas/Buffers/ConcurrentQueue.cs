using System;
using System.Collections;


namespace NanoKernel.Ayudantes
{
    public class ConcurrentQueue
    {
        private readonly ArrayList _items;
        private readonly object _lockObject = new object();
        private readonly int _maxSize;

        public ConcurrentQueue(int maxSize)
        {
            _items = new ArrayList();
            _maxSize = maxSize;
        }

        public int Count()
        { 
            return _items.Count;
        }

        public object Enqueue(object item)
        {
            lock (_lockObject)
            {
                object discardedItem = null;
                if (_items.Count >= _maxSize)
                {
                    discardedItem = _items[0];
                    _items.RemoveAt(0);
                }
                _items.Add(item);
                return discardedItem;
            }
        }

        public object Dequeue()
        {
            lock (_lockObject)
            {
                if (_items.Count == 0)
                {
                    return null;
                }

                var item = _items[0];
                _items.RemoveAt(0);
                return item;
            }
        }

        public bool IsEmpty()
        {
            lock (_lockObject)
            {
                return _items.Count == 0;
            }
        }

        public void Clear()
        {
            lock (_lockObject)
            {
                _items.Clear();
            }
        }
    }
}
