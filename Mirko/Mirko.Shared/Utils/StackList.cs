using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Mirko.Utils
{
    public class StackList<T> where T: class
    {
        private List<T> items = new List<T>();

        public void Push(T item)
        {
            items.Add(item);
        }

        public T Pop()
        {
            if (items.Count > 0)
            {
                T temp = items[items.Count - 1];
                items.RemoveAt(items.Count - 1);
                return temp;
            }
            else
                return default(T);
        }

        public T Peek()
        {
            if (items.Count > 0)
                return items[items.Count - 1];
            else
                return default(T);
        }

        public T First(T item)
        {
            return items.FirstOrDefault(x => x == item);
        }

        public int Count()
        {
            return items.Count;
        }

        public void Remove(T item)
        {
            items.Remove(item);
        }

        public void Remove(int itemAtPosition)
        {
            items.RemoveAt(itemAtPosition);
        }
    }
}
