using CS465_SearchEngine.Source.DataStructures;
using System;
using System.Collections.Generic;

// Michael Weger
// CS465, S22, Project #1

namespace CS465_SearchEngine.Source.Index.Utility
{
    /// <summary>
    /// Class representing a term. Contains posting list for the term.
    /// </summary>
    public class Term : IComparable
    {
        public readonly String Key; // The term's string key
        public readonly SkipPointerLinkedList<Posting> Postings; // Posting list

        public Term(String key)
        {
            this.Key = key;
            this.Postings = new SkipPointerLinkedList<Posting>();
        }

        public Term(String key, ICollection<Posting> postings)
        {
            this.Key = key;
            this.Postings = new SkipPointerLinkedList<Posting>(postings);
        }

        /// <summary>
        /// Number of documents the term appeared in.
        /// </summary>
        public int Frequency
        {
            get { return Postings.Count; }
        }

        /// <summary>
        /// Appends a posting to the posting list.
        /// </summary>
        /// <param name="posting">The posting to add.</param>
        public void AddPosting(Posting posting)
        {
            this.Postings.Add(posting);
        }

        /// <summary>
        /// Compares this term to another term or string representing the term key.
        /// </summary>
        /// <param name="obj">The term or string to compare against.</param>
        /// <returns>The comparison result.</returns>
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

        /// <summary>
        /// Returns a string representation of this Term.
        /// </summary>
        /// <returns>A string representation of this term.</returns>
        public override String ToString()
        {
            string postingsStr = "";
            foreach (Posting posting in Postings)
                postingsStr += posting.ToString();

            return Key + " " + postingsStr;
        }

        /// <summary>
        /// Returns a string listing of document Ids.
        /// </summary>
        /// <returns>A string representation of document Ids.</returns>
        public string DocumentIds()
        {
            string postingsStr = "";
            foreach (Posting posting in Postings)
                postingsStr += posting.DocumentId + ", ";

            if (postingsStr.EndsWith(", "))
                postingsStr = postingsStr.Substring(0, postingsStr.LastIndexOf(", "));

            return "Postings: " + postingsStr;
        }
    }
}
