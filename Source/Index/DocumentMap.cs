using CS465_SearchEngine.Source.Index.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

// Michael Weger
// CS465, S22, Project #1

namespace CS465_SearchEngine.Source.Index
{
	/// <summary>
	/// Class representing and managing a DocumentId - Document mapping.
	/// </summary>
    public class DocumentMap
    {
		private string FilePath; // Path at which the mapping is saved.
        private Dictionary<int, Document> Map; // The documentId - Document mapping in memory
        private int NextDocumentId; // The next documentId to use.

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

		/// <summary>
		/// Returns the next document ID. Pre-increments to prepare for the next Id to use.
		/// </summary>
		/// <returns>The next document Id to use.</returns>
		public int GetNextDocumentId()
        {
			return ++NextDocumentId; // Pre increment to move to next Id
        }

		/// <summary>
		/// Returns the object representation of a document mapped to the provided document Id.
		/// </summary>
		/// <param name="documentId">The documentId to find the corresponding document to.</param>
		/// <returns>The document mapped to the documentId.</returns>
		public Document GetDocument(int documentId)
        {
			return Map.GetValueOrDefault(documentId);
		}

		/// <summary>
		/// Adds the provided document to the mapping.
		/// </summary>
		/// <param name="document">The document to add to the mapping.</param>
		public void AddDocument(Document document)
		{
			Map.Add(NextDocumentId, document);
		}

		/// <summary>
		/// Whether or not the provided document exists in the mapping and on disk.
		/// </summary>
		/// <param name="documentId">The document to check if it exists.</param>
		/// <returns>Whether or not the provided document exists in the mapping and on disk.</returns>
		public bool DocumentExists(int documentId)
        {
			Document document = Map.GetValueOrDefault(documentId);

			return document != default && File.Exists(document.FilePath);
		}

		/// <summary>
		/// Prints the document mapping to console.
		/// </summary>
		public void Print()
		{
			Console.WriteLine("Mapped Documents");
			foreach (int key in Map.Keys)
			{
				Document document = Map.GetValueOrDefault(key);
				Console.WriteLine(document.DocumentId + ", " + document.FilePath + ", " + document.DistinctWords + ", " + document.TotalWords);
			}
		}

		/// <summary>
		/// Prints document mapping statistics to console.
		/// </summary>
		public void PrintStatistics()
		{
			Console.WriteLine("\nDistinct and Total words per document: ");
			int totalWordsCollection = 0;
			foreach (int key in Map.Keys)
			{
				Document document = Map.GetValueOrDefault(key);
				Console.WriteLine("\nDocumentId: " + document.DocumentId + "\nFilePath: " + document.FilePath + "\nDistinct Words: " + document.DistinctWords + "\nTotal Words: " + document.TotalWords);
				totalWordsCollection += document.TotalWords;
			}

			Console.WriteLine("\nTotal Words Collection: " + totalWordsCollection);
		}

		/// <summary>
		/// Writes the document mapping to file.
		/// </summary>
		public void WriteToFile()
		{
			if (!File.Exists(FilePath))
			{
				File.Create(FilePath);
			}

			StreamWriter writer = new StreamWriter(FilePath);
			writer.AutoFlush = true;

			try
			{
				foreach (int key in Map.Keys)
                {
					Document document = Map.GetValueOrDefault(key);
					writer.WriteLine(document.DocumentId + "," + document.FilePath + "," + document.DistinctWords + "," + document.TotalWords);
				}

				writer.Close();
			}
			catch (IOException)
			{
				writer.Close();
				throw new IOException("Failed to write to the document map file.");
			}
		}

		/// <summary>
		/// Resolves the documents mapping from file.
		/// </summary>
		/// <param name="filePath"></param>
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
					string[] documentSplit = line.Split(",", StringSplitOptions.RemoveEmptyEntries); // 0 - DocId, 1 - FilePath, 2 - UniqueWords, 3 - TotalWords

					try
                    {
						int documentId  = Convert.ToInt32(documentSplit[0]);
						
						// Advance the document based on the last highest document id read from disk.
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
