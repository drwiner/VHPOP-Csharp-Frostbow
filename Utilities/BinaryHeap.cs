using System;
using System.Collections;
using System.Collections.Generic;


namespace BoltFreezer.Utilities
{
    public enum HeapType
    {
        MinHeap,
        MaxHeap
    }

    public class Heap<T> where T : IComparable<T>
    {
        List<T> items;

        public HeapType MinOrMax { get; private set; }

        public T Root
        {
            get { return items[0]; }
        }

        public Heap(HeapType type)
        {
            items = new List<T>();
            this.MinOrMax = type;
        }

        public Heap(HeapType type, List<T> existingList)
        {
            items = existingList;
            this.MinOrMax = type;
        }

        public List<T> ToList()
        {
            return items;
        }

        public bool IsEmpty()
        {
            if (items.Count > 0)
                return false;
            return true;
        }

        public int Count
        {
            get { return items.Count; }
        }

        public void Insert(T item)
        {
            items.Add(item);

            int i = items.Count - 1;

            bool flag = true;
            if (MinOrMax == HeapType.MaxHeap)
                flag = false;

            while (i > 0)
            {
                if ((items[i].CompareTo(items[(i - 1) / 2]) > 0) ^ flag)
                {
                    T temp = items[i];
                    items[i] = items[(i - 1) / 2];
                    items[(i - 1) / 2] = temp;
                    i = (i - 1) / 2;
                }
                else
                    break;
            }
        }

        public void DeleteRoot()
        {
            int i = items.Count - 1;

            items[0] = items[i];
            items.RemoveAt(i);

            i = 0;

            bool flag = true;
            if (MinOrMax == HeapType.MaxHeap)
                flag = false;

            while (true)
            {
                int leftInd = 2 * i + 1;
                int rightInd = 2 * i + 2;
                int largest = i;

                if (leftInd < items.Count)
                {
                    if ((items[leftInd].CompareTo(items[largest]) > 0) ^ flag)
                        largest = leftInd;
                }

                if (rightInd < items.Count)
                {
                    if ((items[rightInd].CompareTo(items[largest]) > 0) ^ flag)
                        largest = rightInd;
                }

                if (largest != i)
                {
                    T temp = items[largest];
                    items[largest] = items[i];
                    items[i] = temp;
                    i = largest;
                }
                else
                    break;
            }
        }

        public T PopRoot()
        {
            T result = items[0];

            DeleteRoot();

            return result;
        }
    }
}