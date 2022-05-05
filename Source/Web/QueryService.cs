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
        private cInvertedIndex Index;

        public void Initialize(string indexPath)
        {
            Index = new cInvertedIndex(indexPath);
        }

        public Task<List<int>> OrSearch(string rawQuery)
        {
            return Task.Run( () =>
            {
                List<string> processedQuery = PreprocessQuery(rawQuery);
                List<Posting> postings = Index.OrSearch(processedQuery);
                List<int> documentIds = new List<int>(postings.Count);

                foreach (Posting posting in postings)
                    documentIds.Add(posting.DocumentId);

                return documentIds;
            });
        }

        public Task<List<int>> AndSearch(string rawQuery)
        {
            return Task.Run(() =>
            {
                List<string> processedQuery = PreprocessQuery(rawQuery);
                SkipPointerLinkedList<Posting> postings = Index.AndSearch(processedQuery);
                List<int> documentIds = new List<int>(postings.Count);

                foreach (Posting posting in postings)
                    documentIds.Add(posting.DocumentId);

                return documentIds;
            });
        }

        public Task<List<Tuple<int, string>>> PositionalSearch(string rawQuery)
        {
            return Task.Run(() =>
            {
                Tuple<List<string>, List<int>> processedQuery = PreprocessPositionalQuery(rawQuery);
                List<List<Tuple<int, int, int>>> searchResults = Index.PositionalSearch(processedQuery.Item1, processedQuery.Item2);
                List<Tuple<int, string>> results = new List<Tuple<int, string>>(searchResults.Count);

                int previousDocumentId = -1;
                string previousPositions = "";
                foreach(List<Tuple<int, int, int>> path in searchResults)
                {
                    if(previousDocumentId != path.First().Item1)
                    {
                        if(previousDocumentId != -1)
                        {
                            results.Add(new Tuple<int, string>(previousDocumentId, previousPositions));
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

                if (previousDocumentId != -1)
                {
                    results.Add(new Tuple<int, string>(previousDocumentId, previousPositions));
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
