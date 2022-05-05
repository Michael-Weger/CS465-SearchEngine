using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CS465_SearchEngine.Source.InvertedIndex;
using CS465_SearchEngine.Source.InvertedIndex.Utility;


namespace Tokenizer
{
    class Tokenizer
    {
        static (int, int, string[], string) tokenMaker(string wordDoc)
        {
            string text = System.IO.File.ReadAllText(wordDoc);   //Converts input documents to string
            Regex rgx = new Regex("[^a-zA-z0-9 ]");              //Whitelists all alphanumeric characters and whitespace

            text = text.ToLowerInvariant();                      //Makes each word lowercase
            text = text.Replace("\r","").Replace("\n"," ");      //Removes all carriage return values in txt documents (for windows)
            text = text.Replace("&", "and").Replace("@", "at");
            text = rgx.Replace(text, "");                                     
            text = text.Replace("[", "").Replace("]", "").Replace("^", "");   //Replaces '[' , ']' and '^' as they are not specified in regex

            //Tokenizes string based on certain delimiters
            string[] words = text.Split(new char[] {' ','.','?','!','_','-'}, StringSplitOptions.RemoveEmptyEntries);

            int uniqueTerms = 0;                                 //Amount of non-duplicate words in document
            int wordTotal = words.Length;                        //Amount of all words in document

            Array.Sort(words, StringComparer.InvariantCulture);  //Puts words in alphabetical order

            string[] noDups = words.Distinct().ToArray();        //Removes all duplicates
            uniqueTerms = noDups.Length;

            //**TESTING** Prints total words and unique words for current document
            //System.Console.WriteLine("WordTotal: " + wordTotal + " Unique: " + uniqueTerms);


            int docID = 1;       //Current document ID  --- Turn into global variable???
            int wordPos = 0;     //Term position in document
            string posID = "";   //Stores (docID,Position) delimited by spaces ; parallel to words array
            string temp;         //Temp value for posID

            /* Creates a string that includes document and position
             * coordinates that run parallel to the term array.  */
            foreach (string word in words)
            {
                wordPos += 1;            

                temp = docID + "," + wordPos + " ";
                posID += temp;
            }


            //string docString;      //Test value

            //**TESTING** Prints each word in array
            /*
            foreach (string word in words)
            {
                System.Console.WriteLine("Term: " + word + " DocID: " + wordDoc);
                docString = "Term: " + word + " DocID: " + docID;
                System.Console.WriteLine(docString);
            }
            */
            return(wordTotal, uniqueTerms, words, posID);
        }

        //Used to test Tokenizer
        public static void Main(string[] args)
        {
            //Adds each documnet (text file) located in a folder to an array
            //string[] fileArr = Directory.GetFiles(@"C:\Users\micha\Documents\School\Spring2022\CS-465\Project_1\tokenizer\Text"); //***Change***

            //(int, int, string[], string) ans;
            //int wrdCnt = 0;
            //int unWrdCnt = 0;
            //string posID = "";

            //Goes through each file and finds the # of words and unique words
            /*
            for(int i = 0; i < fileArr.Length; i++)
            {
                ans = tokenMaker(fileArr[i]);

                wrdCnt = ans.Item1 + wrdCnt;
                unWrdCnt = ans.Item2 + unWrdCnt;
                posID = posID + ans.Item4 + "\n\n";
                
            }
            */
            //////////////////////////////////////////////////////////////////////////

            //THIS IS WHAT I BELIEVE ResolveFromFile() IN InvertedIndex SHOULD BE CHANGED TO

            //Adds each documnet (text file) located in a folder to an array
            string[] fileArr = Directory.GetFiles(@"C:\Users\micha\Documents\School\Spring2022\CS-465\Project_1\tokenizer\Text"); //***Change***

            (int, int, string[], string) tokenAns;
            int wrdCnt = 0;
            int unWrdCnt = 0;
            string[] termArr;
            string posIdStr = "";

            //Iterates through each document
            for(int docNum = 0; docNum < fileArr.Length; i++)
            {
                tokenMaker(fileArr[i]);

                wrdCnt = tokenAns.Item1;
                unWrdCnt = tokenAns.Item2;
                termArr = tokenAns.Item3;
                posIdStr = tokenAns.Item4;

				string[] documentSplit = posIdStr.Split(" "); // Splitting whitespace splits the term and documents. Term is index 0.
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
						positions.Add(Convert.ToInt32(postingSplit[j]));      //added AddPosition to Postings.cs
                    }

				    postings.Add(new Posting(documentId, positions));
                }

				Dictionary.Insert(new Term(documentSplit[0], postings));
            }
            //////////////////////////////////////////////////////////////////////////

            //**TESTING** Test outputs
            //System.Console.WriteLine("Word Total: " + wrdCnt);
            //System.Console.WriteLine("Unique Word Total: " + unWrdCnt);
            //System.Console.WriteLine("posID String:\n" + posID);
        }
    }
}