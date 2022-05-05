using CS465_SearchEngine.Source.DataStructures.Nodes;
using System;

namespace CS465_SearchEngine.Source.DataStructures
{
    public class BTree<T> where T : IComparable
    {
        private BTreeNode<T> Root;
        private int Degree;
        private int _Count;

        /// <summary>
        /// Intantiates a BTree with a default order of 2 making it equivalent to a binary tree.
        /// </summary>
        public BTree() : this(2) { }

        /// <summary>
        /// Instantiates a BTree with the provided order.
        /// </summary>
        /// <param name="degree">The order of the BTree.</param>
        public BTree(int order)
        {
            if(order < 3)
            {
                throw new ArgumentException("BTree order must be at least 3.");
            }

            this.Degree = (int) Math.Ceiling(order / 2d); // Store the minimum degree instead of order to avoid calculating a median repeatedly.
            this.Root = new BTreeNode<T>(Degree);
        }

        public int Count
        {
            get { return _Count; }
        }

        /// <summary>
        /// Traverses the BTree from the left.
        /// </summary>
        public void traverse()
        {
            if (this.Root != null)
                this.Root.traverse();
            else
                Console.WriteLine("BTree is empty.");
        }

        /// <summary>
        /// Searches the BTree for the item corresponding to the specified key.
        /// </summary>
        /// <param name="key">The key to search by.</param>
        /// <returns>The item corresponding to the key if it exists.</returns>
        public T Search(IComparable key)
        {
            return this.Root.Search(key);
        }

        /// <summary>
        /// Inserts the specified item into the BTree if it does not already exist.
        /// </summary>
        /// <param name="item">The item to insert.</param>
        /// <returns>Whether or not the operation was successful.</returns>
        public bool Insert(T item)
        {
            if (this.Root.IsFull)
            {
                BTreeNode<T> oldRoot = this.Root; 
                this.Root = new BTreeNode<T>(Degree);
                this.Root.Children.Add(oldRoot);
                this.Root.SplitMergeChild(0);
            }

            bool result = this.Root.Insert(item);

            if (result)
                this._Count++;

            return result;
        }
    }
}