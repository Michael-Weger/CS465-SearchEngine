using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CS465_SearchEngine.Source.Index.Utility;
using System.Linq;

// Michael Weger, Michael Young, Zachary Pass
// CS465, S22, Project #1

namespace CS465_SearchEngine.Source.Index
{
    public class ParserInverter
    {
        private InvertedIndex Index; // The inverted index to build on
        private DocumentMap DocumentMap; // The document mapping to add onto

        private string InputDirectory; // Document input directory
        private string OutputDirectory; // Document output directory

        private bool DoCaseFolding; // Whether or not to perform case folding.
        public readonly bool DoStemming;   // Whether or not to perform stemming. Exposed to allow for the positional search to normalize its strings in a slightly different way...
        private List<string> StopWords; // Stop words to exclude from the index.

        public ParserInverter(InvertedIndex index, DocumentMap documentMap, string inputDirectory, string outputDirectory, bool doCaseFolding, bool doStemming) : this(index, documentMap, inputDirectory, outputDirectory, doCaseFolding, doStemming, "") { }

        public ParserInverter(InvertedIndex index, DocumentMap documentMap, string inputDirectory, string outputDirectory, bool doCaseFolding, bool doStemming, string stopWordsPath)
        {
            this.Index = index;
            this.DocumentMap = documentMap;

            this.InputDirectory = inputDirectory;
            this.OutputDirectory = outputDirectory;

            this.DoCaseFolding = doCaseFolding || doStemming;
            this.DoStemming = doStemming;
            this.StopWords = this.ReadStopWords(stopWordsPath);

            this.ProcessDocuments(); // Begin processing documents once initialized.
        }

        /// <summary>
        /// Reads the stop words from the specified file.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private List<string> ReadStopWords(string filePath)
        {
            List<string> stopWords = new List<string>();

            if (filePath == "")
            {
                return stopWords;
            }
            else if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("Requested stop words file at " + filePath + " does not exist.");
            }
            else
            {
                try
                {
                    StreamReader reader = new StreamReader(filePath);

                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        // Process stop words in the same manner as other text to ensure it is normalized the same way to exclude them.
                        line = this.ProcessString(line);
                        stopWords.Add(line);
                    }

                    reader.Close();
                }
                catch (IOException)
                {
                    Console.WriteLine("Failed to completely read from stopwords file.");
                }
            }

            return stopWords;
        }


        /// <summary>
        /// Normalizes the provided string.
        /// </summary>
        /// <param name="str">String to normalize.</param>
        /// <returns>Normalized string.</returns>
        public string ProcessString(string str)
        {
            return ProcessString(str, false);
        }

        /// <summary>
        /// Normalizes the provided string.
        /// </summary>
        /// <param name="str">String to normalize.</param>
        /// <param name="whitelistPositional">Whether or not to whitelist \ for processing positional queries</param>
        /// <returns>Normalized string.</returns>
        public string ProcessString(string str, bool whitelistPositional)
        {
            string output = str;

            Regex regex = whitelistPositional ? new Regex("^[a-zA-Z0-9\\\\ ]*$") : new Regex("[^a-zA-z0-9 ]"); // Whitelists all alphanumeric characters and whitespace

            if (DoCaseFolding)
                output = output.ToLowerInvariant(); //Makes each word lowercase

            output = output.Replace("\r", "").Replace("\n", " "); // Removes all carriage return values in txt documents (for windows)
            output = output.Replace("&", "and").Replace("@", "at");
            output = regex.Replace(output, "");
            output = output.Replace("[", "").Replace("]", "").Replace("^", ""); // Replaces '[' , ']' and '^' as they are not specified in regex

            return output;
        }

        /// <summary>
        /// Standardises splitting strings.
        /// </summary>
        /// <param name="toSplit">String to split.</param>
        /// <returns>List of tokens from the string.</returns>
        public List<string> SplitString(string toSplit)
        {
            return toSplit.Split(new char[] { ' ', '.', '?', '!', '_', '-' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        }

        public List<string> ProcessWords(List<string> words)
        {
            this.RemoveStopWords(words);

            if (DoStemming)
                words = PorterStemmer.StemWords(words);

            return words;
        }

        /// <summary>
        /// Removes all stop words recgonized by this instance from the provided list of words.
        /// </summary>
        /// <param name="words">List of words to remove stop words from.</param>
        /// <returns>List of words with stop words removed.</returns>
        public List<string> RemoveStopWords(List<string> words)
        {
            words.RemoveAll(word => this.StopWords.Contains(word)); // Remove Stopwords
            return words;
        }

        /// <summary>
        /// Processes the doucments present in the InputDirectory and builds the index.
        /// </summary>
        /// <returns></returns>
        private void ProcessDocuments()
        {
            if(!Directory.Exists(InputDirectory))
            {
                Directory.CreateDirectory(InputDirectory);
            }

            string[] files = Directory.GetFiles(InputDirectory, "", SearchOption.AllDirectories);

            // Iterate and process each file.
            foreach(string filePath in files)
            {
                Console.WriteLine("Processing document: " + filePath);

                int documentId = DocumentMap.GetNextDocumentId();

                try
                {
                    (int, int) results = ProcessDocument(filePath, documentId);

                    // Move the document to the output path and save the location in the mapping.
                    string outputPath = Path.Combine(OutputDirectory, Path.GetFileName(filePath)); 
                    File.Move(filePath, outputPath);

                    Document document = new Document(documentId, outputPath, results.Item1, results.Item2);
                    DocumentMap.AddDocument(document);
                }
                catch(Exception)
                {
                    continue; // Failed to read the file, try again later.
                }
            }

            // Save the index and mapping to disk.
            Index.WriteToFile();
            DocumentMap.WriteToFile();
        }

        /// <summary>
        /// Tokenizes and normalizes a document. Uses results to build the index.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="documentId"></param>
        /// <returns></returns>
        private (int, int) ProcessDocument(string filePath, int documentId)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("Requested doucment to tokenize at " + filePath + " does not exist.");
            }

            int totalWords = 0; // Total words encountered in this document (for statistics)
            int currPosition = 1; // Current position in the document (for positional searches)

            StreamReader reader = new StreamReader(filePath);
            List<string> distinctList = new List<string>(); // List of distinct words (for statistics)

            string line;
            while ((line = reader.ReadLine()) != null)
            {
                // Process before delimiting
                line = this.ProcessString(line);

                // Tokenizes line based on certain delimiters
                List<string> words = this.SplitString(line);

                // Statistics stuff
                distinctList.AddRange(words); // Adds all words to the distinct list and removes all nondistinct values.
                distinctList = distinctList.Distinct().ToList();
                totalWords += words.Count; // Update total words for statistics

                // Process individual tokens
                words = this.ProcessWords(words);

                // Process each token
                foreach (string word in words)
                {
                    Term term = this.Index.GetTerm(word);

                    // New term
                    if (term == default)
                    {
                        term = this.Index.AddTerm(word);
                        Posting posting = new Posting(documentId);

                        term.AddPosting(posting);
                        posting.AddPosition(currPosition);
                    }
                    // Eixstant term, possibly a new posting.
                    else
                    {
                        Posting posting;
                        try
                        {
                            posting = term.Postings.Get(documentId);
                        }
                        catch(NullReferenceException)
                        {
                            posting = default;
                        }
                        
                        // New posting
                        if(posting == default)
                        {
                            posting = new Posting(documentId);
                            term.AddPosting(posting);
                        }

                        posting.AddPosition(currPosition);
                    }

                    currPosition++;
                }
            }

            reader.Close();
            int distinctWords = distinctList.Count; // Use the count on the distinct list for distinct words in the file. Down here to be in scope.

            return (distinctWords, totalWords);
        }
    }
}