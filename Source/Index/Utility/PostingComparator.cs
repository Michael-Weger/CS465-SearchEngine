using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CS465_SearchEngine.Source.Index.Utility
{
    /// <summary>
    /// A comparator class created in order to use the List#Union function on postings for Or searches to avoid O(n^2) runtimes.
    /// </summary>
    public class PostingComparator : IEqualityComparer<Posting>
    {
        public bool Equals(Posting postingA, Posting postingB)
        {
            return postingA.DocumentId == postingB.DocumentId;
        }

        public int GetHashCode(Posting posting)
        {
            return posting.DocumentId;
        }
    }
}
