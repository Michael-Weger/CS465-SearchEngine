using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CS465_SearchEngine.Source.Index.Utility;
using System.Linq;

namespace CS465_SearchEngine.Source.Index
{
    public class ParserInverter
    {
        private InvertedIndex Index;
        private DocumentMap DocumentMap;

        private string InputDirectory;
        private string OutputDirectory;

        private bool DoCaseFolding;
        private List<string> StopWords;

        public ParserInverter(InvertedIndex index, DocumentMap documentMap, string inputDirectory, string outputDirectory, bool doCaseFolding) : this(index, documentMap, inputDirectory, outputDirectory, doCaseFolding, "") { }

        public ParserInverter(InvertedIndex index, DocumentMap documentMap, string inputDirectory, string outputDirectory, bool doCaseFolding, string stopWordsPath)
        {
            this.Index = index;
            this.DocumentMap = documentMap;

            this.InputDirectory = inputDirectory;
            this.OutputDirectory = outputDirectory;

            this.DoCaseFolding = doCaseFolding;
            this.StopWords = this.ReadStopWords(stopWordsPath);

            this.ProcessDocuments();
        }

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

        private string ProcessString(string str)
        {
            string output = str;

            Regex rgx = new Regex("[^a-zA-z0-9 ]"); // Whitelists all alphanumeric characters and whitespace

            if (DoCaseFolding)
                output = output.ToLowerInvariant(); //Makes each word lowercase

            output = output.Replace("\r", "").Replace("\n", " "); // Removes all carriage return values in txt documents (for windows)
            output = output.Replace("&", "and").Replace("@", "at");
            output = rgx.Replace(output, "");
            output = output.Replace("[", "").Replace("]", "").Replace("^", ""); // Replaces '[' , ']' and '^' as they are not specified in regex

            return output;
        }

        private InvertedIndex ProcessDocuments()
        {
            if(!Directory.Exists(InputDirectory))
            {
                Directory.CreateDirectory(InputDirectory);
            }

            string[] files = Directory.GetFiles(InputDirectory, "", SearchOption.AllDirectories);

            InvertedIndex finalIndex = new InvertedIndex();

            foreach(string filePath in files)
            {
                Console.WriteLine("Processing document: " + filePath);

                int documentId = DocumentMap.GetNextDocumentId();

                try
                {
                    (int, int) results = ProcessDocument(filePath, documentId);
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

            Index.WriteToFile();
            DocumentMap.WriteToFile();

            return finalIndex;
        }

        private (int, int) ProcessDocument(string filePath, int documentId)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("Requested doucment to tokenize at " + filePath + " does not exist.");
            }

            int totalWords = 0;
            int currPosition = 1;

            StreamReader reader = new StreamReader(filePath);
            Regex rgx = new Regex("[^a-zA-z0-9 ]"); // Whitelists all alphanumeric characters and whitespace
            List<string> distinctList = new List<string>();

            string line;
            while ((line = reader.ReadLine()) != null)
            {
                line = this.ProcessString(line);
                // Tokenizes line based on certain delimiters
                List<string> words = line.Split(new char[] { ' ', '.', '?', '!', '_', '-' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                
                distinctList.AddRange(words);
                distinctList = distinctList.Distinct().ToList();

                // Update total words for statistics
                totalWords += words.Count;

                words.RemoveAll(word => this.StopWords.Contains(word)); // Remove Stopwords

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
                            posting = default;//Posting posting = term.Postings.Get(documentId);
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

            int distinctWords = distinctList.Count;

            return (distinctWords, totalWords);
        }
    }
}