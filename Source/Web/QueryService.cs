using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CS465_SearchEngine.Source.DataStructures;
using CS465_SearchEngine.Source.Index;
using CS465_SearchEngine.Source.Index.Utility;

// Michael Weger
// CS465, S22, Project #1

namespace CS465_SearchEngine.Source.Web
{
    /// <summary>
    /// Web API service. Users send queries to the service which interfaces them with the Index.
    /// </summary>
    public class QueryService
    {
        private InvertedIndex  InvertedIndex;  // The index
        private DocumentMap    DocumentMap;    // The documents mapping used to verify that documents still exist before sending them to the user.
        private ParserInverter ParserInverter; // Provides standardised string manipulation between index creation and user queries

        public void Initialize(InvertedIndex invertedIndex, DocumentMap documentMap, ParserInverter parserInverter)
        {
            InvertedIndex = invertedIndex;
            DocumentMap = documentMap;
            ParserInverter = parserInverter;
        }

        /// <summary>
        /// Interfaces a user query with an OR search.
        /// </summary>
        /// <param name="rawQuery">Unprocessed user query.</param>
        /// <returns>OR search results.</returns>
        public Task<List<Document>> OrSearch(string rawQuery)
        {
            return Task.Run( () =>
            {
                List<string> processedQuery = PreprocessQuery(rawQuery);
                List<Posting> postings = InvertedIndex.OrSearch(processedQuery);
                List<Document> documents = new List<Document>(postings.Count);

                // Ensure all documents still exist on disk. Exclude any documents no longer present.
                foreach (Posting posting in postings)
                {
                    if (DocumentMap.DocumentExists(posting.DocumentId))
                        documents.Add(DocumentMap.GetDocument(posting.DocumentId));
                }

                return documents;
            });
        }

        /// <summary>
        /// Interfaces a user query with an AND search.
        /// </summary>
        /// <param name="rawQuery">Unprocessed user query.</param>
        /// <returns>AND search results.</returns>
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

        /// <summary>
        /// Interfaces a user query with an POSITIONAL search.
        /// </summary>
        /// <param name="rawQuery">Unprocessed user query.</param>
        /// <returns>POSITONAL search results of Documents and the positions the user's search terms appeared in the document.</returns>
        public Task<List<Tuple<Document, string>>> PositionalSearch(string rawQuery)
        {
            return Task.Run(() =>
            {
                Tuple<List<string>, List<int>> processedQuery = PreprocessPositionalQuery(rawQuery);
                List<List<Tuple<int, int, int>>> searchResults = InvertedIndex.PositionalSearch(processedQuery.Item1, processedQuery.Item2);
                List<Tuple<Document, string>> results = new List<Tuple<Document, string>>(searchResults.Count);

                int previousDocumentId = -1; // Default to -1 as doucment Ids are >= 0
                string previousPositions = "";

                // Extract the term positions out of each pairing.
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

                        // The last hit must take the right position (item3) from the pairing.
                        if (i == path.Count - 1)
                            previousPositions += hit.Item3 + "; ";
                    }
                }

                // Add the last result in if it exists
                if (previousDocumentId != -1 && DocumentMap.DocumentExists(previousDocumentId))
                {
                    results.Add(new Tuple<Document, string>(DocumentMap.GetDocument(previousDocumentId), previousPositions));
                }

                return results;
            });
        }

        /// <summary>
        /// Processes the user query returning a normalized query.
        /// </summary>
        /// <param name="rawQuery">The raw user query.</param>
        /// <returns>A normalized user query.</returns>
        private List<string> PreprocessQuery(string rawQuery)
        {
            string processedQuery = ParserInverter.ProcessString(rawQuery);
            List<string> splitQuery = ParserInverter.SplitString(processedQuery);
            splitQuery = ParserInverter.ProcessWords(splitQuery);

            return splitQuery;
        }

        /// <summary>
        /// Processes a positional query to extract distances from the user query.
        /// </summary>
        /// <param name="rawQuery">The raw user query.</param>
        /// <returns>A list of processed terms and corresponding distances.</returns>
        private Tuple<List<string>, List<int>> PreprocessPositionalQuery(string rawQuery)
        {
            rawQuery = rawQuery.ToLower();
            // Processing here is differnt as the \ character is saved.
            rawQuery = string.Join("", from character in rawQuery where character == '\\' || char.IsLetterOrDigit(character) || char.IsWhiteSpace(character) select character);

            List<string> queryStr = new List<string>();
            List<int> queryDistances = new List<int>();

            int currTerm = 0;
            bool nextTokenTerm = true;

            foreach (string token in rawQuery.Split(' '))
            {
                if (token.Length > 1 && token[0] == '\\')
                {
                    // No term token on this pass. Add default distance.
                    if (nextTokenTerm)
                        continue;

                    int distance;

                    try
                    {
                        distance = Convert.ToInt32(token.Substring(1));
                    }
                    catch (FormatException)
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
                    else if (currTerm > 0)
                    {
                        nextTokenTerm = false;
                    }

                    queryStr.Add(token);
                }

                currTerm++;
            }

            // Check if the last distance is present in the case the user ended the query on a term to default to 1
            if (queryStr.Count - 1 > queryDistances.Count)
                queryDistances.Add(1);

            if(ParserInverter.DoStemming)
                queryStr = PorterStemmer.StemWords(queryStr);

            return new Tuple<List<string>, List<int>>(queryStr, queryDistances);
        }
    }
}
