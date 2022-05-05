using CS465_SearchEngine.Source.DataStructures;
using System;
using System.Collections.Generic;

namespace CS465_SearchEngine.Source.Index.Utility
{
    public class Posting : IComparable
    {
        public readonly int DocumentId;
        public readonly SkipPointerLinkedList<int> Positions;

        public Posting(int documentId, ICollection<int> positions)
        {
            this.DocumentId = documentId;
            this.Positions = new SkipPointerLinkedList<int>(positions);
        }

        public int Frequency
        {
            get { return Positions.Count; }
        }

        public void AddPosition(int position)
        {
            this.Positions.Add(position);
        }

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

        public override String ToString()
        {
            return DocumentId + ":" + Positions;
        }
    }
}
