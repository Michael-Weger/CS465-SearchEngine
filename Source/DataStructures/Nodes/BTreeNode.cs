using System;
using System.Collections.Generic;

namespace CS465_SearchEngine.Source.DataStructures.Nodes
{
    /// <summary>
    /// Class representing a node on a BTree. Can store up to order number of children and order-1 values.
    /// </summary>
    /// <typeparam name="Term">The type of data held by the node.</typeparam>
    class BTreeNode<T> where T : IComparable
    {
        public List<T> Keys;
        public List<BTreeNode<T>> Children;
        public int Degree; // Minimum number of children

        /// <summary>
        /// Instantiates an instance of a BTreeNode with the specified order.
        /// </summary>
        /// <param name="degree"></param>
        public BTreeNode(int degree)
        {
            this.Degree = degree;
            this.Keys = new List<T>(degree - 1);
            this.Children = new List<BTreeNode<T>>(degree);
        }

        public bool IsFull
        {
            get { return Keys.Count == this.Degree*2 - 1; }
        }

        public bool IsLeaf
        {
            get { return Children.Count == 0; }
        }

        /// <summary>
        /// Traverses the subtree of this node.
        /// </summary>
        public void traverse()
        {
            int index = 0;
            for (index = 0; index < this.Keys.Count; index++)
            {
                if (!this.IsLeaf)
                    Children[index].traverse();

                Console.WriteLine(this.Keys[index].ToString());
            }

            if (!this.IsLeaf)
                this.Children[index].traverse();
        }

        /// <summary>
        /// Finds the item corresponding to the specified key in the tree if it exists.
        /// </summary>
        /// <param name="key">The key to use to find the corresponding item.</param>
        /// <returns>The item corresponding to the specified key.</returns>
        public T Search(IComparable key)
        {
            // Iterate through the list to find the appropriate subtree or result.
            int index = 0;
            while (index < this.Keys.Count && this.Keys[index].CompareTo(key) < 0)
                index++;

            // Found the value
            if (index < this.Keys.Count && this.Keys[index].CompareTo(key) == 0)
                return Keys[index];

            // Leaf node with no children, item is not in the subtree
            if (this.IsLeaf)
                return default;

            // Move to the subtree to the right of the index
            return this.Children[index].Search(key);
        }

        /// <summary>
        /// Inserts a new node at the appropriate location sorted ascending left to right.
        /// </summary>
        /// <param name="item">The item to add.</param>
        /// <returns>Whether or not the item already existed.</returns>
        public bool Insert(T item)
        {
            // Iterate through the list to find the appropriate subtree or result.
            int index = 0;
            while (index < this.Keys.Count && item.CompareTo(this.Keys[index]) > 0)
                index++;

            // Key already exists
            if (index < this.Keys.Count && item.CompareTo(this.Keys[index]) == 0)
            {
                return false;
            }
            else if (this.IsLeaf)
            {
                this.Keys.Insert(index, item);
                return true;
            }
            else
            {
                // Move to the child right of the key
                if (this.Children[index].IsFull)
                {
                    this.SplitMergeChild(index);

                    // With the child now split into two new child nodes determine which node the new item goes to. 
                    if (item.CompareTo(Keys[index]) > 0)
                        index++;
                }

                return this.Children[index].Insert(item);
            }
        }

        /// <summary>
        /// Splits the node at its median and pushes up values to the right. Assumes this node is full.
        /// </summary>
        /// <param name="childIndex">The index of the child to split.</param>
        public void SplitMergeChild(int childIndex)
        {
            BTreeNode<T> child = this.Children[childIndex];
            BTreeNode<T> leftNode = new BTreeNode<T>(this.Degree);

            this.Keys.Insert(childIndex, child.Keys[this.Degree - 1]);
            this.Children.Insert(childIndex + 1, leftNode);

            leftNode.Keys.AddRange(child.Keys.GetRange(this.Degree, this.Degree - 1));
            child.Keys.RemoveRange(this.Degree - 1, this.Degree ); // -1 to remove the median index

            if (!child.IsLeaf)
            {
                leftNode.Children.AddRange(child.Children.GetRange(this.Degree, this.Degree));
                child.Children.RemoveRange(this.Degree, this.Degree);
            }
        }
    }
}
