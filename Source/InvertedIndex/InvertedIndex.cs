using System.Collections.Generic;
using System.IO;
using System;
using CS465_SearchEngine.Source.DataStructures;
using CS465_SearchEngine.Source.DataStructures.Nodes;
using CS465_SearchEngine.Source.InvertedIndex.Utility;
using System.Linq;

namespace CS465_SearchEngine.Source.InvertedIndex
{
	/// <summary>
	/// Class representing an inverted index for a simple information retrieval system.
	/// </summary>
	public class InvertedIndex
	{
		private BTree<Term> Dictionary;
		private List<bool> Documents;

		public InvertedIndex()
		{
			Dictionary = new BTree<Term>(6);
		}

		public InvertedIndex(String filePath)
		{ 
			Dictionary = new BTree<Term>(5);

			try
			{
				this.ResolveFromFile(filePath);
			}
			catch(Exception e)
			{
				Console.WriteLine(e.StackTrace);
				Console.WriteLine("[WARNING]: " + e.Message);
            }

			Dictionary.traverse();

			Console.WriteLine("Or Search <apple, banana, blueberry>");
			foreach (Posting posting in this.OrSearch(new List<string>(new string[] { "apple", "banana", "blueberry" })))
			{
				Console.Write(posting.DocumentId + " ");
			}

			Console.WriteLine("\nAnd Search <apple, banana, strawberry>");
			foreach (Posting posting in this.AndSearch(new List<string>(new string[] { "apple", "banana", "strawberry" })))
			{
				Console.Write(posting.DocumentId + " ");
			}

			Console.WriteLine("\nPositional(r=1) Search <apple, banana, strawberry>");
            foreach (List<Tuple<int, int, int>> path in this.PositionalSearch(new List<string>(new string[] { "apple", "banana", "strawberry" }), new List<int>(new int[] { 1, 1 })))
			{
				foreach (Tuple<int,int,int> data in path)
                {
					Console.Write("<" + data.Item1 + "Id, " + data.Item2 + "pp, " + data.Item3 + "pp> ");
                }

				Console.Write("\n");
			}
		}

		/// <summary>
		/// Gets proper Term instances from a user's search query
		/// </summary>
		/// <param name="query">A list of strings from the user's search query.</param>
		/// <returns>A list of terms corresponding to the user's query.</returns>
		private List<Term> getTerms(List<string> query)
        {
			List<Term> results = new List<Term>();

			foreach (string searchTerm in query)
			{
				Term term = Dictionary.Search(searchTerm);
				results.Add(term);
			}

			return results;
		}

		/// <summary>
		/// Performs an Or search returning any documents which contained a single search term.
		/// </summary>
		/// <param name="query">List of strings of the user query.</param>
		/// <returns>Any documents where a single search term appeared.</returns>
		private List<Posting> OrSearch(List<string> query)
		{
			List<Term> terms = getTerms(query);
			PostingComparator comparator = new PostingComparator();
			return terms.SelectMany(term => term.Postings).Distinct(comparator).ToList();
		}

		/// <summary>
		/// Performs an And search returning any documents which contain all search terms.
		/// </summary>
		/// <param name="query">List of strings of the user query.</param>
		/// <returns>Any documents where all search terms appeared.</returns>
		private SkipPointerLinkedList<Posting> AndSearch(List<string> query)
        {
			List<Term> terms = getTerms(query);
			return IntersectPostings(terms);
        }

		/// <summary>
		/// Performs a Positional And search returning any documents which contain all search terms within the specified proximitys.
		/// </summary>
		/// <param name="query">List of strings of the user query.</param>
		/// <param name="distances">List of distances of each term pair.</param>
		/// <returns>Any documents where all search terms appeared within the specified proximity of each pair.</returns>
		private List<List<Tuple<int, int, int>>> PositionalSearch(List<string> query, List<int> distances)
		{
			List<Term> terms = getTerms(query);
			List<List<Tuple<int, int, int>>> pairedIntersections = new List<List<Tuple<int, int, int>>>(terms.Count);

			for (int index = 0; index < terms.Count - 1; index++)
			{
				pairedIntersections.Add(PositionalIntersect(terms[index].Postings, terms[index + 1].Postings, distances[index]));
			}

			// Properly pair the positional data up to produce a "path" of the sequence of search terms for each document hit
			List<List<Tuple<int, int, int>>> results = new List<List<Tuple<int, int, int>>>();
			while (pairedIntersections[0].Count > 0)
			{
				List<Tuple<int, int, int>> path = new List<Tuple<int, int, int>>(pairedIntersections.Count);
				path.Add(pairedIntersections[0][0]);
				pairedIntersections[0].RemoveAt(0);
				
				for (int indexA = 0; indexA < pairedIntersections.Count - 1; indexA++)
				{
					Tuple<int, int, int> previousData = path[indexA];
					for (int indexB = 0; indexB < pairedIntersections[indexA + 1].Count; indexB++)
					{
						Tuple<int, int, int> data = pairedIntersections[indexA + 1][indexB];
						if (data.Item1 == previousData.Item1 && data.Item2 == previousData.Item3)
						{
							path.Add(data);
							break;
						}
					}
				}

				if (path.Count == pairedIntersections.Count)
					results.Add(path);
			}

			return results;
		}

		/// <summary>
		/// Intersects the Postings lists of all terms.
		/// </summary>
		/// <param name="terms">The terms to find the posting list intersection of.</param>
		/// <returns>The intersected list of postings.</returns>
		private SkipPointerLinkedList<Posting> IntersectPostings(List<Term> terms)
		{
			if (terms.Count == 0)
			{
				return new SkipPointerLinkedList<Posting>();
			}
			else if (terms.Count == 1)
			{
				return terms[0].Postings;
			}
			else
			{
				terms.OrderBy(term => term.Frequency);
				SkipPointerLinkedList<Posting> intersect = terms[0].Postings;

				for (int i = 1; i < terms.Count; i++)
				{
					Console.WriteLine(terms[i]);
					intersect = IntersectPostings(intersect, terms[i].Postings);
				}

				return intersect;
			}
		}

		/// <summary>
		/// Intersects the provided posting lists. Does not modify either posting list.
		/// </summary>
		/// <param name="listA">First posting list.</param>
		/// <param name="listB">Second posting list.</param>
		/// <returns>An intersection of both posting lists.</returns>
		private SkipPointerLinkedList<Posting> IntersectPostings(SkipPointerLinkedList<Posting> listA, SkipPointerLinkedList<Posting> listB)
        {
			List<Posting> intersection = new List<Posting>();

            SkipPointerLinkedListNode<Posting> currentNodeA = listA.First;
			SkipPointerLinkedListNode<Posting> currentNodeB = listB.First;

			while(currentNodeA != null && currentNodeB != null)
            {
				int comparison = currentNodeA.Value.CompareTo(currentNodeB.Value);

				// Found a hit, check positional data if applicable.
				if(comparison == 0)
                {
					intersection.Add(currentNodeA.Value);

					currentNodeA = currentNodeA.Next;
					currentNodeB = currentNodeB.Next;
                }
				// Miss, A > B so move B
				else if(comparison > 0)
                {
					currentNodeB = currentNodeB.AdvanceNode(currentNodeA.Value);
                }
				// Miss, B > A so move A
				else
				{
					currentNodeA = currentNodeA.AdvanceNode(currentNodeB.Value);
				}
            }

			return new SkipPointerLinkedList<Posting>(intersection);
		}


		/// <summary>
		/// Intersects the provided posting lists taking into account the distance between the specified pairing.
		/// </summary>
		/// <param name="listA">First posting list.</param>
		/// <param name="listB">Second posting list.</param>
		/// <param name="distance">The maximum allowed distance between individual posting positions.</param>
		/// <returns>An intersection of both posting postion lists which are within the specified distance of one another.</returns>
		private List<Tuple<int, int, int>> PositionalIntersect(SkipPointerLinkedList<Posting> listA, SkipPointerLinkedList<Posting> listB, int distance)
        {
			List<Tuple<int, int, int>> positionalIntersect = new List<Tuple<int, int, int>>();

			SkipPointerLinkedListNode<Posting> postingA = listA.First;
			SkipPointerLinkedListNode<Posting> postingB = listB.First;

			while (postingA != null && postingB != null)
			{
				int comparison = postingA.Value.CompareTo(postingB.Value);

				// Found a hit, check positional data if applicable.
				if (comparison == 0)
				{

					SkipPointerLinkedListNode<int> positionA = postingA.Value.Positions.First;
					SkipPointerLinkedListNode<int> positionB = postingB.Value.Positions.First;

					while (positionA != null)
					{
						List<int> I = new List<int>();
						while (positionB != null)
						{
							if (Math.Abs(positionA.Value - positionB.Value) <= distance)
							{
								I.Add(positionB.Value);
							}
							else if (positionB.Value > positionA.Value)
							{
								break;
							}

							positionB = positionB.AdvanceNode(positionA.Value - distance);
						}

						while (I.Count > 0 && Math.Abs(I[0] - positionA.Value) > distance)
						{
							I.RemoveAt(0);
						}

						foreach (int position in I)
						{
							positionalIntersect.Add(new Tuple<int, int, int>(postingA.Value.DocumentId, positionA.Value, position));
						}

						if (positionB != null)
							positionA = positionA.AdvanceNode(positionB.Value - distance);
						else
							break;
					}

					postingA = postingA.Next;
					postingB = postingB.Next;
				}
				// Miss, A > B so move B
				else if (comparison > 0)
				{
					postingB = postingB.AdvanceNode(postingA.Value);
				}
				// Miss, B > A so move A
				else
				{
					postingA = postingA.AdvanceNode(postingB.Value);
				}
			}

			return positionalIntersect;
		}

		private void WriteToFile()
        {

        }

		//Term Posting,pos,pos,pos Posting,pos,pos,pos...
		private void ResolveFromFile(String filePath)
		{
			if (!File.Exists(filePath))
			{
				throw new FileNotFoundException("Inverted Index file does not exist.");
			}

			StreamReader reader = new StreamReader(filePath);

			try
            {
				string line;
				while((line = reader.ReadLine()) != null)
                {
					string[] documentSplit = line.Split(" "); // Splitting whitespace splits the term and documents. Term is index 0.
					List<Posting> postings = new List<Posting>(documentSplit.Length - 1);

					// Resolve each document
					for(int i = 1; i < documentSplit.Length; i++)
                    {
						string[] postingSplit = documentSplit[i].Split(","); // Splitting commas separates the docId from the positional data

						int documentId = Convert.ToInt32(postingSplit[0]);
						List<int> positions = new List<int>(postingSplit.Length - 1);

						// Resolve each position
						for (int j = 1; j < postingSplit.Length; j++)
                        {
							positions.Add(Convert.ToInt32(postingSplit[j]));
                        }

						postings.Add(new Posting(documentId, positions));
                    }

					Dictionary.Insert(new Term(documentSplit[0], postings));
                }
            }
			catch (IOException e)
			{
				Console.WriteLine(e.StackTrace);
				throw new IOException("Failed to read the inverted index file.");
			}

			reader.Close();
		}
	}
}

