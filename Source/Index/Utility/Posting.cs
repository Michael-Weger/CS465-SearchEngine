using CS465_SearchEngine.Source.DataStructures;
using System;
using System.Collections.Generic;

// Michael Weger
// CS465, S22, Project #1

namespace CS465_SearchEngine.Source.Index.Utility
{
    /// <summary>
    /// Class representing a single posting for a term. Contains the positions a given term appeared at in the document.
    /// </summary>
    public class Posting : IComparable
    {
        public readonly int DocumentId; // Document this posting is for
        public readonly SkipPointerLinkedList<int> Positions; // Positions at which the term this posting is for appeared at

        public Posting(int documentId)
        {
            this.DocumentId = documentId;
            this.Positions = new SkipPointerLinkedList<int>();
        }

        public Posting(int documentId, ICollection<int> positions)
        {
            this.DocumentId = documentId;
            this.Positions = new SkipPointerLinkedList<int>(positions);
        }

        /// <summary>
        /// Number of times the term appeared in this document
        /// </summary>
        public int Frequency
        {
            get { return Positions.Count; }
        }

        /// <summary>
        /// Appends the given position to this posting.
        /// </summary>
        /// <param name="position">The position to add.</param>
        public void AddPosition(int position)
        {
            this.Positions.Add(position);
        }

        /// <summary>
        /// Compares a posting to another posting or integer key representing the document ID.
        /// </summary>
        /// <param name="obj">Object to compare against.</param>
        /// <returns>Comparison result.</returns>
        public int CompareTo(object obj)
        {
            if (obj is int)
            {
                int documentId = (int) obj;
                return DocumentId.CompareTo(documentId);
            }
            else if (obj is Posting)
            {
                Posting posting = (Posting) obj;
                return DocumentId.CompareTo(posting.DocumentId);
            }
            else
            {
                throw new ArgumentException("Postings must be compared to integers or other postings.");
            }
        }

        /// <summary>
        /// Returns this posting as a string representation.
        /// </summary>
        /// <returns>String representation of this posting.</returns>
        public override String ToString()
        {
            return DocumentId + "," + Positions;
        }
    }
}
