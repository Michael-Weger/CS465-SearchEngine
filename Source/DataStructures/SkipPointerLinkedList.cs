using CS465_SearchEngine.Source.DataStructures.Nodes;
using System;
using System.Collections;
using System.Collections.Generic;

namespace CS465_SearchEngine.Source.DataStructures
{
    /// <summary>
    /// A stripped down LinkedList implementation with skip pointers for use with an InvertedIndex.
    /// Elements stored in ascending order.
    /// </summary>
    /// <typeparam name="T">The type of data to store in each node.</typeparam>
    public class SkipPointerLinkedList<T> : IEnumerable<T>, IEnumerable where T : IComparable
    {
        public  SkipPointerLinkedListNode<T> First;
        private SkipPointerLinkedListNode<T> Last;
        private int _Count;

        /// <summary>
        /// Creates an empty SkipPointerLinkedList instance.
        /// </summary>
        public SkipPointerLinkedList()
        {
            
        }

        /// <summary>
        /// Creates a SkipPointerLinkedList instance with the elements from collection in ascending order.
        /// </summary>
        /// <param name="collection">The collection of items to initalize using.</param>
        public SkipPointerLinkedList(ICollection<T> collection)
        {
            foreach (T item in collection)
            {
                this.Add(item);
            }
        }

        public int Count
        {
            get { return this._Count; }
        }

        /// <summary>
        /// Adds the specified element to the collection in ascending order.
        /// </summary>
        /// <param name="item">The element to add to the data structure.</param>
        public void Add(T item)
        {
            SkipPointerLinkedListNode<T> node = new SkipPointerLinkedListNode<T>(item);

            if(this.First == null)
            {
                this.First = node;
                this.Last = node;
            }
            else if (this.Last != null)
            {
                int comparison = this.Last.Value.CompareTo(item);

                // Prevent duplicates
                if (comparison == 0)
                {
                    return;
                }
                // The new item has a greater Id than the current last one
                else if (comparison < 0)
                {
                    this.Last.Next = node;
                    node.Previous = this.Last;
                    this.Last = node;
                }
                // If the InvertedIndex implementation is used properly this should never be called... but an internal order should be maintained as best as possible.
                else
                {
                    this.InsertOrdered(node);
                }
            }

            this._Count++;

            // Update skip pointers to keep them evenly spaced
            // I debated not adding this as, for the purposes of this project, there shouldn't be any reason to insert doucments with younger Ids, but decided to do so anyway
            // as in a real system a document may have been updated with new terms or a term may appear in a new position.
            // This still shouldn't get called in the project or its demo but its here anyway... just in case.
            updateSkipPointers();
        }

        public T Get(IComparable compareBy)
        {
            if (this.First == null)
                return default;

            SkipPointerLinkedListNode<T> currentNode = this.First;
            while (currentNode != null)
            {
                int comparison = currentNode.Value.CompareTo(compareBy);

                // Found a hit, check positional data if applicable.
                if (comparison == 0)
                {
                    return currentNode.Value;
                }
                // Miss, move forward
                else if (comparison < 0)
                {
                    currentNode = currentNode.AdvanceNode(compareBy);
                }
                // Value does not exist
                else
                {
                    return default;
                }
            }

            return default;
        }

        /// <summary>
        /// Inserts an element at the appropriate spot to keep the data strcture sorted in ascending order.
        /// </summary>
        /// <param name="newNode">The node to add to the data strcture.</param>
        private void InsertOrdered(SkipPointerLinkedListNode<T> newNode)
        {
            SkipPointerLinkedListNode<T> currentNode = this.First;

            while (currentNode != null)
            {
                int comparison = currentNode.Value.CompareTo(newNode.Value);

                if (comparison == 0)
                {
                    Console.WriteLine("Exit");
                    return; // Duplicate value quit early
                }
                // The node to be inserted has not been a duplicate and is less than the current value. Insert behind the current node.
                else if(comparison > 0)
                {
                    Console.WriteLine(currentNode.Value + " | " + newNode.Value);

                    if(currentNode.Previous != null)
                    {
                        currentNode.Previous.Next = newNode;
                        newNode.Previous = currentNode.Previous;
                    }

                    currentNode.Previous = newNode;
                    newNode.Next = currentNode;
                }
                else
                {
                    currentNode = currentNode.Next;
                }
            }
        }

        /// <summary>
        /// Updates the placement of all skip pointers.
        /// </summary>
        private void updateSkipPointers()
        {
            // Not enough nodes for there to be more than 1 skip node.
            if (this._Count < 4)
                return;

            int frequency = (int) Math.Sqrt(this._Count);
            int nodesIterated = 0;

            SkipPointerLinkedListNode<T> lastSkipNode = this.First;
            SkipPointerLinkedListNode<T> currentNode  = this.First;

            while(currentNode != null)
            {
                currentNode.Skip = null;

                if(nodesIterated % frequency == 0)
                {
                    lastSkipNode.Skip = currentNode;
                    lastSkipNode = currentNode;
                }

                nodesIterated++;
                currentNode = currentNode.Next;
            }
        }
        /*
         * Is formatted to show skip pointers and have brackets but due to generics ToString is needed for writing to disk without implmenting a new interface.
        public override string ToString()
        {
            string str = "{ ";
            SkipPointerLinkedListNode<T> currentNode = this.First;

            while (currentNode != null)
            {
                str += currentNode.Value.ToString();

                if (currentNode.Skip != null)
                    str += "*";

                str += " ";

                currentNode = currentNode.Next;
            }

            str += "}";

            return str;
        }*/

        public override string ToString()
        {
            string str = "";
            SkipPointerLinkedListNode<T> currentNode = this.First;

            while (currentNode != null)
            {
                str += currentNode.Value.ToString();

                if (currentNode.Next != null)
                    str += ",";
                else
                    str += " ";

                currentNode = currentNode.Next;
            }

            return str;
        }

        public List<T> asList()
        {
            List<T> results = new List<T>(this._Count);

            SkipPointerLinkedListNode<T> currentNode = First;
            while (currentNode != null)
            {
                results.Add(currentNode.Value);
                currentNode = currentNode.Next;
            }

            return results;
        }

        public IEnumerator<T> GetEnumerator()
        {
            SkipPointerLinkedListNode<T> currentNode = First;
            while(currentNode != null)
            {
                yield return currentNode.Value;
                currentNode = currentNode.Next;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
