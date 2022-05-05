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
        private string InputDirectory;
        private string OutputDirectory;

        private DocumentMap DocumentMap;
        private List<string> StopWords;

        private bool DoCaseFolding;

        public ParserInverter(string inputDirectory, string outputDirectory, DocumentMap documentMap, bool doCaseFolding) : this(inputDirectory, outputDirectory, documentMap, doCaseFolding, "") { }

        public ParserInverter(string inputDirectory, string outputDirectory, DocumentMap documentMap, bool doCaseFolding, string stopWordsPath)
        {
            this.InputDirectory = inputDirectory;
            this.OutputDirectory = outputDirectory;

            this.DocumentMap = documentMap;
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
                        Console.WriteLine(line);
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
                Console.WriteLine(filePath);
                int documentId = DocumentMap.GetNextDocumentId();
                (InvertedIndex, int, int) results = ProcessDocument(filePath, documentId);

                File.Move(filePath, Path.Combine(OutputDirectory, Path.GetFileName(filePath)));

                InvertedIndex index = results.Item1;
                index.traverse();
                //finalIndex.Merge(index);

                Document document = new Document(documentId, filePath, results.Item2, results.Item3);
                DocumentMap.AddDocument(document);
            }

            return finalIndex;
        }

        private (InvertedIndex, int, int) ProcessDocument(string filePath, int documentId)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("Requested doucment to tokenize at " + filePath + " does not exist.");
            }

            InvertedIndex index = new InvertedIndex();

            int totalWords = 0;
            int currPosition = 1;

            StreamReader reader = new StreamReader(filePath);
            string line;
            Regex rgx = new Regex("[^a-zA-z0-9 ]"); // Whitelists all alphanumeric characters and whitespace
            while ((line = reader.ReadLine()) != null)
            {
                line = this.ProcessString(line);
                // Tokenizes line based on certain delimiters
                List<string> words = line.Split(new char[] { ' ', '.', '?', '!', '_', '-' }, StringSplitOptions.RemoveEmptyEntries).ToList();

                // Update total words for statistics
                totalWords += words.Count;

                words.RemoveAll(word => this.StopWords.Contains(word)); // Remove Stopwords

                foreach(string word in words)
                {
                    Term term = index.GetTerm(word);

                    if (term == default)
                        term = index.AddTerm(word, documentId);

                    term.AddPosition(documentId, currPosition);

                    currPosition++;
                }
            }

            reader.Close();

            int distinctWords = index.Count;

            return (index, distinctWords, totalWords);
        }
    }
}