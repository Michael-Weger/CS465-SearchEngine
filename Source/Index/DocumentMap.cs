﻿using CS465_SearchEngine.Source.Index.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CS465_SearchEngine.Source.Index
{
    public class DocumentMap
    {
		private string FilePath;
        private Dictionary<int, Document> Map;
        private int NextDocumentId;

        public DocumentMap(string filePath)
        {
            try
            {
				this.FilePath = filePath;
				this.Map = new Dictionary<int, Document>();
				this.ResolveFromFile(filePath);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                Console.WriteLine("[WARNING]: " + e.Message);
            }
        }

		public Document GetDocument(int documentId)
        {
			return Map.GetValueOrDefault(documentId);
		}

		public bool DocumentExists(int documentId)
        {
			Document document = Map.GetValueOrDefault(documentId);

			return document != default && File.Exists(document.FilePath);
        }

		public void WriteToFile()
		{
			if (!File.Exists(FilePath))
			{
				File.Create(FilePath);
			}

			StreamWriter writer = new StreamWriter(FilePath);

			try
			{
				foreach (Document document in Map.Values)
                {
					writer.WriteLine(document.DocumentId + "," + document.FilePath + "," + document.DistinctWords + "," + document.TotalWords);
                }
			}
			catch(IOException)
            {
				throw new IOException("Failed to write to the document map file.");
			}

			writer.Close();
        }

		private void ResolveFromFile(String filePath)
		{
			if (!File.Exists(filePath))
			{
				throw new FileNotFoundException("Document Map file does not exist.");
			}

			StreamReader reader = new StreamReader(filePath);

			try
			{
				string line;
				while ((line = reader.ReadLine()) != null)
				{
					string[] documentSplit = line.Split(","); // 0 - DocId, 1 - FilePath, 2 - UniqueWords, 3 - TotalWords

					try
                    {
						int documentId  = Convert.ToInt32(documentSplit[0]);

						if (NextDocumentId < documentId)
							NextDocumentId = documentId + 1;

						string path     = documentSplit[1];
						int uniqueWords = Convert.ToInt32(documentSplit[2]);
						int totalWords  = Convert.ToInt32(documentSplit[3]);

						Document document = new Document(documentId, path, uniqueWords, totalWords);
						Map.Add(documentId, document);
					}
					catch(IndexOutOfRangeException)
					{
						Console.WriteLine("Failed to resolve " + line + " as a Document. Not enough comma delimited fields.");
						continue;
					}
					catch(FormatException)
                    {
						Console.WriteLine("Failed to resolve numeric fields of " + line + ".");
						continue;
					}
				}
			}
			catch (IOException)
			{
				throw new IOException("Failed to read the inverted index file.");
			}

			reader.Close();
		}
	}
}