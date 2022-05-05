using System;

namespace CS465_SearchEngine.Source.DataStructures.Nodes
{
    /// <summary>
    /// A class for a SkipPointerNode to be used in a LinkedList implementation with skip pointers.
    /// </summary>
    /// <typeparam name="T">The type of data to store in the node.</typeparam>
    public class SkipPointerLinkedListNode<T> where T : IComparable
    {
        public SkipPointerLinkedListNode<T> Next;     // A reference to the next node.
        public SkipPointerLinkedListNode<T> Previous; // A reference to the previous node.
        public SkipPointerLinkedListNode<T> Skip;     // A reference to the node to skip to.
        public T Value; // The value assigned to this node.

        /// <summary>
        /// Initializes a new SkipPointerNode containing the specified value.
        /// </summary>
        /// <param name="value">The value assigned to this node.</param>
        public SkipPointerLinkedListNode(T value)
        {
            this.Value = value;
        }

        /// <summary>
        /// Advances to the next applicable node (less than or equal to the comparison value) making use of skip pointers as much as possible.
        /// </summary>
        /// <param name="compareTo">The value to compare against.</param>
        /// <returns>The next applicable node remaining less than the provided value.</returns>
        public SkipPointerLinkedListNode<T> AdvanceNode(IComparable compareTo)
        {
            // Check for skips
            if (this.Skip != null && this.Skip.Value.CompareTo(compareTo) <= 0)
            {
                SkipPointerLinkedListNode<T> nextSkip = this.Skip;
                // Advance as many skips as possible keeping node B < A
                while (nextSkip != null && nextSkip.Skip != null && nextSkip.Skip.Value.CompareTo(compareTo) <= 0)
                {
                    nextSkip = nextSkip.Skip;
                }

                return nextSkip;
            }
            else
            {
                return this.Next;
            }
        }
    }
}
