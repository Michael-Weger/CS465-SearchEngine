using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CS465_SearchEngine.Source.Web
{
    public class QueryModel
    {
        [Required]
        [StringLength(100,ErrorMessage = "Query is too long.")]
        public string RawQuery { get; set; }

        [Required]
        public SearchType Type { get; set; } = SearchType.Or;

        public enum SearchType
        {
            Or,
            And,
            Positional
        }
    }
}
