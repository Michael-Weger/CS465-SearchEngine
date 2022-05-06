# CS465-SearchEngine

Midterm project for CS465 Information Retrieval and Data Mining at Kettering University.

## Instructions
Design and implement a simple IR system.

The system should:
* Create the inverted index (the dictionary and postings lists) for your collection of documents
* Parse and execute simple queries
* Perform simple tokenization and normalization of the text such as removing digits, punctuation
marks, etc.
* Statistics:
  * Report the number of distinct words observed in each document, and the total number of
words encountered.
  * Report the number of distinct words observed in the whole collection of documents, and the
total number of words encountered.
  * Report the total number of times each word is seen (term frequency) and the document IDs
where the word occurs (Output the posting list for a term).
  * Report the top 100th, 500th, and 1000th most-frequent word and their frequencies of
occurrence.
  * Create postings and assign a term frequency to every document in the postings list.
  * Provide a simple GUI to test the system.

## Member Contributions:
  
  *	Michael Weger
    * Data structures
    * Search queries
    *	Stemming
    *	GUI
   *	Statistics
  *	Michael Young
    *	Document parsing
  *	Zachary Pass
    *	Index construction

## Running the Application:
  1.	Clone the repository from https://github.com/Michael-Weger/CS465-SearchEngine/commits/main to your desired directory. If you downloaded as zip unzip the archived repository.
  2.	Put any documents you want to process in the ./DocumentInput directory
  3.	Open a terminal window and navigate to the directory the repository is in.
  4.	Run the command dotnet run.
  5.	The program will start processing all files in the ./DocumentInput directory moving them to the Documents directory once complete.
  6.	As documents are processed the index is constructed along with a mapping of document IDs to file paths. These files can be found in the ./Environment directory.
  7.	After processing documents the program will print the index, the documents mapping, and statistics to the terminal. These outputs have been provided in the Statistics directory for your convenience.
  8.	Your default web browser will open the search page.
  9.	Users can make queries by inputting a query to the text field, selecting a search type, and hitting the submit button. Positional queries must have the syntax of term term \$distance term \$distance term … where every \$distance is the maximum distance between the previous two terms. Not specifying a distance will default to 1 for adjacent terms.
  10.	The program can be closed from the terminal by sending the interrupt signal (LControl + C)

## References:
  *	Positional Intersect from the textbook.
  *	SPIMI algorithm from the textbook.
  *	Various Blazor documentation and tutorials from Microsoft. 
    *	https://dotnet.microsoft.com/en-us/learn/aspnet/blazor-tutorial/create
    *	https://docs.microsoft.com/en-us/aspnet/core/blazor/forms-validation?view=aspnetcore-5.0
  *	Referenced when creating the BTree
    *	 https://en.wikipedia.org/wiki/B-tree
    *	https://www.geeksforgeeks.org/insert-operation-in-b-tree/
  *	Referenced to implement Porter Stemming
    *	https://tartarus.org/martin/PorterStemmer/def.txt
  *	Stopwords taken from 
    *	https://www.ranks.nl/stopwords
