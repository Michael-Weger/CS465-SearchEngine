using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CS465_SearchEngine.Source.DataStructures;
using CS465_SearchEngine.Source.Index;
using CS465_SearchEngine.Source.Index.Utility;

namespace CS465_SearchEngine.Source.Web
{
    public class QueryService
    {
        private InvertedIndex InvertedIndex;
        private DocumentMap   DocumentMap;

        public void Initialize(InvertedIndex invertedIndex, DocumentMap documentMap)
        {
            InvertedIndex = invertedIndex;
            DocumentMap = documentMap;
        }

        public Task<List<Document>> OrSearch(string rawQuery)
        {
            return Task.Run( () =>
            {
                List<string> processedQuery = PreprocessQuery(rawQuery);
                List<Posting> postings = InvertedIndex.OrSearch(processedQuery);
                List<Document> documents = new List<Document>(postings.Count);

                foreach (Posting posting in postings)
                {
                    if (DocumentMap.DocumentExists(posting.DocumentId))
                        documents.Add(DocumentMap.GetDocument(posting.DocumentId));
                }

                return documents;
            });
        }

        public Task<List<Document>> AndSearch(string rawQuery)
        {
            return Task.Run(() =>
            {
                List<string> processedQuery = PreprocessQuery(rawQuery);
                SkipPointerLinkedList<Posting> postings = InvertedIndex.AndSearch(processedQuery);
                List<Document> documents = new List<Document>(postings.Count);

                foreach (Posting posting in postings)
                {
                    if (DocumentMap.DocumentExists(posting.DocumentId))
                        documents.Add(DocumentMap.GetDocument(posting.DocumentId));
                }

                return documents;
            });
        }

        public Task<List<Tuple<Document, string>>> PositionalSearch(string rawQuery)
        {
            return Task.Run(() =>
            {
                Tuple<List<string>, List<int>> processedQuery = PreprocessPositionalQuery(rawQuery);
                List<List<Tuple<int, int, int>>> searchResults = InvertedIndex.PositionalSearch(processedQuery.Item1, processedQuery.Item2);
                List<Tuple<Document, string>> results = new List<Tuple<Document, string>>(searchResults.Count);

                int previousDocumentId = -1;
                string previousPositions = "";
                foreach(List<Tuple<int, int, int>> path in searchResults)
                {
                    if(previousDocumentId != path.First().Item1)
                    {
                        if(previousDocumentId != -1 && DocumentMap.DocumentExists(previousDocumentId))
                        {
                            results.Add(new Tuple<Document, string>(DocumentMap.GetDocument(previousDocumentId), previousPositions));
                        }

                        previousDocumentId = path.First().Item1;
                        previousPositions = "";
                    }

                    for (int i = 0; i < path.Count; i++)
                    {
                        Tuple<int, int, int> hit = path[i];

                        previousPositions += hit.Item2 + ", ";

                        if (i == path.Count - 1)
                            previousPositions += hit.Item3 + "; ";
                    }
                }

                if (previousDocumentId != -1 && DocumentMap.DocumentExists(previousDocumentId))
                {
                    results.Add(new Tuple<Document, string>(DocumentMap.GetDocument(previousDocumentId), previousPositions));
                }

                return results;
            });
        }

        private List<string> PreprocessQuery(string rawQuery)
        {
            rawQuery = rawQuery.ToLower();
            rawQuery = string.Join("", from character in rawQuery where char.IsLetterOrDigit(character) || char.IsWhiteSpace(character) select character);
            return new List<string>(rawQuery.Split(' '));
        }

        private Tuple<List<string>, List<int>> PreprocessPositionalQuery(string rawQuery)
        {
            rawQuery = rawQuery.ToLower();
            rawQuery = string.Join("", from character in rawQuery where character == '\\' || char.IsLetterOrDigit(character) || char.IsWhiteSpace(character) select character);

            List<string> queryStr = new List<string>();
            List<int> queryDistances = new List<int>();

            int currTerm = 0;
            bool nextTokenTerm = true;

            foreach (string token in rawQuery.Split(' '))
            {
                if(token.Length > 1 && token[0] == '\\')
                {
                    // No term token on this pass. Add default distance.
                    if (nextTokenTerm)
                        continue;

                    int distance;

                    try
                    {
                        distance = Convert.ToInt32(token.Substring(1));
                    }
                    catch(FormatException)
                    {
                        distance = 1;
                    }

                    queryDistances.Add(distance);

                    nextTokenTerm = true;
                }
                else
                {
                    // No distance token on this pass. Add default distance.
                    if (!nextTokenTerm)
                    {
                        queryDistances.Add(1);
                        nextTokenTerm = true;
                    }
                    else if(currTerm > 0)
                    {
                        nextTokenTerm = false;
                    }

                    queryStr.Add(token);
                }

                currTerm++;
            }

            if (queryStr.Count - 1 > queryDistances.Count)
                queryDistances.Add(1);

            return new Tuple<List<string>, List<int>>(queryStr, queryDistances);
        }
    }
}
