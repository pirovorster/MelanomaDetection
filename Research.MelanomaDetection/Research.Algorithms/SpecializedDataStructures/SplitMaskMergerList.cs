
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Research.Algorithms.SpecializedDataStructures
{
    internal sealed class SplitMaskMergerList
    {
        private int _capacity;
        private int _capacityIncrement;
        private int _current = 0;
        private int[] _values;

        internal int HighestKey
        {
            get
            {
                return _current;
            }
        }

        internal SplitMaskMergerList(int capacity, int capacityIncrement)
        {
            _capacity = capacity;
            _capacityIncrement = capacityIncrement;
            _values = new int[capacity];
        }

        internal int GetValue(int key)
        {
            // CG: No exception checking for speed reasons, but it is assumed that key is <= _current and key >= 1
            unsafe
            {
                fixed (int* valuesPointer = _values)
                {
                    int value = *(valuesPointer + key - 1);
                    if (value == key)
                    {
                        return value;
                    }
                    List<int> keysToUpdate = new List<int>();
                    while (value != key)
                    {
                        keysToUpdate.Add(key);
                        key = value;
                        value = *(valuesPointer + key - 1);
                    }

                    foreach (int keyToUpdate in keysToUpdate)
                    {
                        *(valuesPointer + keyToUpdate - 1) = value;
                    }

                    return value;
                }
            }
        }

        internal int GetValueRawAccess(int key)
        {
            // CG: No exception checking for speed reasons, but it is assumed that key is <= _current and key >= 1
            return _values[key - 1];
        }

        internal void SetValue(int key, int value)
        {
            // CG: No exception checking for speed reasons, but it is assumed that key is <= _current and key >= 1 and value <= key and value >= 1
            _values[key - 1] = value;
        }

        internal int GetNext()
        {
            if (_current == _capacity)
            {
                this.Resize();
            }

            unsafe
            {
                fixed (int* valuesPointer = _values)
                {
                    _current += 1;
                    *(valuesPointer + _current - 1) = _current;
                }
            }

            return _current;
        }

        private void Resize()
        {
            int newCapacity = checked(_capacity + _capacityIncrement);
            int[] newValues = new int[newCapacity];
            Array.Copy(_values, newValues, _current);
            _capacity = newCapacity;
            _values = newValues;
        }
    }
}
