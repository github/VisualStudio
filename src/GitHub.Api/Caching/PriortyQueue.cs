/*
Copyright 2012, 2013, 2017 Adam Carter (http://adam-carter.com)

This file is part of FileCache (http://github.com/acarteas/FileCache).

FileCache is distributed under the Apache License 2.0.
Consult "LICENSE.txt" included in this package for the Apache License 2.0.
*/
using System.Collections.Generic;

namespace System.Runtime.Caching
{
    /// <summary>
    /// A basic min priorty queue (min heap)
    /// </summary>
    /// <typeparam name="T">Data type to store</typeparam>
    public class PriortyQueue<T> where T : IComparable<T>
    {

        private List<T> _items;
        private IComparer<T> _comparer;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="comparer">The comparer to use.  The default comparer will make the smallest item the root of the heap.  
        /// 
        /// </param>
        public PriortyQueue(IComparer<T> comparer = null)
        {
            _items = new List<T>();
            if (comparer == null)
            {
                _comparer = new GenericComparer<T>();
            }
        }

        /// <summary>
        /// Constructor that will convert an existing list into a min heap
        /// </summary>
        /// <param name="unsorted">The unsorted list of items</param>
        /// <param name="comparer">The comparer to use.  The default comparer will make the smallest item the root of the heap.</param>
        public PriortyQueue(List<T> unsorted, IComparer<T> comparer = null)
            : this(comparer)
        {
            for (int i = 0; i < unsorted.Count; i++)
            {
                _items.Add(unsorted[i]);
            }
            BuildHeap();
        }

        private void BuildHeap()
        {
            for (int i = _items.Count / 2; i >= 0; i--)
            {
                adjustHeap(i);
            }
        }

        //Percolates the item specified at by index down into its proper location within a heap.  Used
        //for dequeue operations and array to heap conversions
        private void adjustHeap(int index)
        {
            //cannot percolate empty list
            if (_items.Count == 0)
            {
                return;
            }

            //GOAL: get value at index, make sure this value is less than children
            // IF NOT: swap with smaller of two
            // (continue to do so until we can't swap)
            T item = _items[index];

            //helps us figure out if a given index has children
            int end_location = _items.Count;

            //keeps track of smallest index
            int smallest_index = index;

            //while we're not the last thing in the heap
            while (index < end_location)
            {
                //get left child index
                int left_child_index = (2 * index) + 1;
                int right_child_index = left_child_index + 1;

                //Three cases:
                // 1. left index is out of range
                // 2. right index is out or range
                // 3. both indices are valid
                if (left_child_index < end_location)
                {
                    //CASE 1 is FALSE
                    //remember that left index is the smallest
                    smallest_index = left_child_index;

                    if (right_child_index < end_location)
                    {
                        //CASE 2 is FALSE (CASE 3 is true)
                        //TODO: find value of smallest index
                        smallest_index = (_comparer.Compare(_items[left_child_index], _items[right_child_index]) < 0)
                            ? left_child_index
                            : right_child_index;
                    }
                }

                //we have two things: original index and (potentially) a child index
                if (_comparer.Compare(_items[index], _items[smallest_index]) > 0)
                {
                    //move parent down (it was too big)
                    T temp = _items[index];
                    _items[index] = _items[smallest_index];
                    _items[smallest_index] = temp;

                    //update index
                    index = smallest_index;
                }
                else
                {
                    //no swap necessary
                    break;
                }
            }
        }

        public bool isEmpty()
        {
            return _items.Count == 0;
        }

        public int GetSize()
        {
            return _items.Count;
        }


        public void Enqueue(T item)
        {
            //calculate positions
            int current_position = _items.Count;
            int parent_position = (current_position - 1) / 2;

            //insert element (note: may get erased if we hit the WHILE loop)
            _items.Add(item);

            //find parent, but be careful if we are an empty queue
            T parent = default(T);
            if (parent_position >= 0)
            {
                //find parent
                parent = _items[parent_position];

                //bubble up until we're done
                while (_comparer.Compare(parent, item) > 0 && current_position > 0)
                {
                    //move parent down
                    _items[current_position] = parent;

                    //recalculate position
                    current_position = parent_position;
                    parent_position = (current_position - 1) / 2;

                    //make sure that we have a valid index
                    if (parent_position >= 0)
                    {
                        //find parent
                        parent = _items[parent_position];
                    }
                }
            } //end check for nullptr

            //after WHILE loop, current_position will point to the place that
            //variable "item" needs to go
            _items[current_position] = item;

        }

        public T GetFirst()
        {
            return _items[0];
        }

        public T Dequeue()
        {
            int last_position = _items.Count - 1;
            T last_item = _items[last_position];
            T top = _items[0];
            _items[0] = last_item;
            _items.RemoveAt(_items.Count - 1);

            //percolate down
            adjustHeap(0);
            return top;
        }


        private class GenericComparer<TInner> : IComparer<TInner> where TInner : IComparable<TInner>
        {
            public int Compare(TInner x, TInner y)
            {
                return x.CompareTo(y);
            }
        }
    }
}