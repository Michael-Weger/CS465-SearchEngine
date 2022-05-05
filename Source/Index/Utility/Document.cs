using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// Michael Weger
// CS465, S22, Project #1

namespace CS465_SearchEngine.Source.Index.Utility
{
    /// <summary>
    /// Data encapsulating class to represent a Document.
    /// </summary>
    public class Document
    {
        public readonly int DocumentId;    // Numeric ID of the document
        public readonly string FilePath;   // File path at which the actual document resides
        public readonly int DistinctWords; // Number of distinct words in this document (for statistics)
        public readonly int TotalWords;    // Total number of words in this document (for statistics)

        public Document(int documentId, string filePath, int distinctWords, int totalWords)
        {
            this.DocumentId = documentId;
            this.FilePath = filePath;
            this.DistinctWords = distinctWords;
            this.TotalWords = totalWords;
        }
    }
}
