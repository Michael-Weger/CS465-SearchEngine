using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

// Michael Weger
// CS465, S22, Project #1

namespace CS465_SearchEngine.Source.Web
{
    /// <summary>
    /// Query Model for the Query submission form on the webpage.
    /// </summary>
    public class QueryModel
    {
        /// <summary>
        /// The raw string inputted by the user for this query.
        /// </summary>
        [Required] // Annotates that this field is required to submit the form.
        [StringLength(100,ErrorMessage = "Query is too long.")] // Query maximum length
        public string RawQuery { get; set; }

        /// <summary>
        /// The search type of this query. defaults to OR
        /// </summary>
        [Required]
        public SearchType Type { get; set; } = SearchType.Or;

        /// <summary>
        /// Enum of search types.
        /// </summary>
        public enum SearchType
        {
            Or,
            And,
            Positional
        }
    }
}
