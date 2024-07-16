using System;
using System.Collections;


namespace NanoKernel.Ayudantes
{
    public class ConcurrentQueue
    {
        private readonly ArrayList _items;
        private readonly object _lockObject = new object();

        public ConcurrentQueue()
        {
            _items = new ArrayList();
        }

        public void Enqueue(object item)
        {
            lock (_lockObject)
            {
                _items.Add(item);
            }
        }

        public object Dequeue()
        {
            lock (_lockObject)
            {
                if (_items.Count == 0)
                {
                    throw new InvalidOperationException("Queue is empty.");
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
