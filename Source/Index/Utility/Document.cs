using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CS465_SearchEngine.Source.Index.Utility
{
    public class Document
    {
        public readonly int DocumentId;
        public readonly string FilePath;
        public readonly int DistinctWords;
        public readonly int TotalWords;

        public Document(int documentId, string filePath, int distinctWords, int totalWords)
        {
            this.DocumentId = documentId;
            this.FilePath = filePath;
            this.DistinctWords = distinctWords;
            this.TotalWords = totalWords;
        }
    }
}
