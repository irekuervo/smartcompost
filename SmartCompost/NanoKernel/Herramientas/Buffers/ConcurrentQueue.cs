using System;
using System.Collections;


namespace NanoKernel.Ayudantes
{
    public class ConcurrentQueue
    {
        private readonly ArrayList items;
        private readonly object lockObject = new object();
        private readonly int maxSize;
        private readonly bool fifo = true; // false =FILO
        public ConcurrentQueue(int maxSize, bool fifo = true)
        {
            this.items = new ArrayList();
            this.maxSize = maxSize;
            this.fifo = fifo;
        }

        public int Size() => maxSize;
        public ArrayList GetItems() => items;

        public int Count()
        {
            lock (lockObject)
            {
                return items.Count; 
            }
        }

        /// <summary>
        /// Encola un objeto
        /// </summary>
        /// <param name="item"></param>
        /// <returns>Devuelve null si la cola no desborda, sino devuelve el objeto desbordado</returns>
        public object Enqueue(object item)
        {
            lock (lockObject)
            {
                object discardedItem = null;
                if (items.Count >= maxSize)
                {
                    discardedItem = items[0];
                    items.RemoveAt(0);
                }
                items.Add(item);
                return discardedItem;
            }
        }

        public object Dequeue()
        {
            lock (lockObject)
            {
                if (items.Count == 0)
                {
                    return null;
                }

                Object removedItem;

                if (fifo)
                {
                    removedItem = items[0];
                    items.RemoveAt(0);
                }
                else
                {
                    removedItem = items[items.Count - 1];
                    items.RemoveAt(items.Count - 1);
                }

                return removedItem;
            }
        }

        public bool IsFull()
        {
            lock (lockObject)
            {
                return items.Count == maxSize;
            }
        }

        public bool IsEmpty()
        {
            lock (lockObject)
            {
                return items.Count == 0;
            }
        }

        public void Clear()
        {
            lock (lockObject)
            {
                items.Clear();
            }
        }
    }
}
