# CS465-SearchEngine

CS 465 Project #1, Simple Information Retrieval System
Member Contributions:
  •	Michael Weger
    o	Data structures
    o	Search queries
    o	Stemming
    o	GUI
    o	Statistics
  •	Michael Young
    o	Document parsing
  •	Zachary Pass
    o	Index construction

Running the Application:
  1.	Clone the repository from https://github.com/Michael-Weger/CS465-SearchEngine/commits/main to your desired directory.
    a.	If you downloaded as zip unzip the archived repository.
  2.	Put any documents you want to process in the ./DocumentInput directory
  3.	Open a terminal window and navigate to the directory the repository is in.
  4.	Run the command dotnet run.
  5.	The program will start processing all files in the ./DocumentInput directory moving them to the Documents directory once complete.
  6.	As documents are processed the index is constructed along with a mapping of document IDs to file paths. These files can be found in the ./Environment directory.
  7.	After processing documents the program will print the index, the documents mapping, and statistics to the terminal. These outputs have been provided in the Statistics directory for your convenience.
  8.	Your default web browser will open the search page.
  9.	Users can make queries by inputting a query to the text field, selecting a search type, and hitting the submit button.
    a.	Positional queries must have the syntax of term term \$distance term \$distance term … where every \$distance is the maximum distance between the previous two terms. Not specifying a distance will default to 1 for adjacent terms.
  10.	The program can be closed from the terminal by sending the interrupt signal (LControl + C)

References:
  •	Positional Intersect from the textbook.
  •	SPIMI algorithm from the textbook.
  •	Various Blazor documentation and tutorials from Microsoft. 
    o	https://dotnet.microsoft.com/en-us/learn/aspnet/blazor-tutorial/create
    o	https://docs.microsoft.com/en-us/aspnet/core/blazor/forms-validation?view=aspnetcore-5.0
  •	Referenced when creating the BTree
    o	 https://en.wikipedia.org/wiki/B-tree
    o	https://www.geeksforgeeks.org/insert-operation-in-b-tree/
  •	Referenced to implement Porter Stemming
    o	https://tartarus.org/martin/PorterStemmer/def.txt
  •	Stopwords taken from 
    o	https://www.ranks.nl/stopwords
