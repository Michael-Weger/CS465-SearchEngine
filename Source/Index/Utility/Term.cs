using CS465_SearchEngine.Source.DataStructures;
using System;
using System.Collections.Generic;

namespace CS465_SearchEngine.Source.Index.Utility
{
    public class Term : IComparable
    {
        public readonly String Key;
        public readonly SkipPointerLinkedList<Posting> Postings;

        public Term(String key, int documentId)
        {
            this.Key = key;
            this.Postings = new SkipPointerLinkedList<Posting>();

            Posting posting = new Posting(documentId);
            this.Postings.Add(posting);
        }

        public Term(String key, ICollection<Posting> postings)
        {
            this.Key = key;
            this.Postings = new SkipPointerLinkedList<Posting>(postings);
        }

        public int Frequency
        {
            get { return Postings.Count; }
        }

        public void AddPosting(Posting posting)
        {
            this.Postings.Add(posting);
        }

        public void AddPosition(int documentId, int position)
        {
            Posting posting = Postings.Get(documentId);

            if (posting != default)
                posting.AddPosition(position);
        }

        public int CompareTo(object obj)
        {
            if(obj is string)
            {
                string key = (string) obj;
                return Key.CompareTo(key);
            }
            else if(obj is Term)
            {
                Term term = (Term) obj;
                return Key.CompareTo(term.Key);
            }
            else
            {
                throw new ArgumentException("Terms must be compared to strings or other terms.");
            }
        }

        public override String ToString()
        {
            return Key + ":" + Postings;
        }
    }
}
