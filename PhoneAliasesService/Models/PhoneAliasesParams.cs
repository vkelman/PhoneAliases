using System.Collections.Generic;
using Newtonsoft.Json;

namespace PhoneAliasesService.Models
{
    public class PhoneAliasesParam
    {
        [JsonProperty(PropertyName = "phone")]
        public string Phone { get; set; }
        [JsonProperty(PropertyName = "page")]
        public string Page { get; set; }
        [JsonProperty(PropertyName = "pageSize")]
        public string PageSize { get; set; }
    }

    public class PhoneAliasesResult
    {
        [JsonProperty(PropertyName = "digitsOnlyPhone")]
        public string DigitsOnlyPhone { get; set; }

        [JsonProperty(PropertyName = "totalAliasesNumber")]
        public int TotalAliasesNumber { get; set; }

        [JsonProperty(PropertyName = "numberOfPages")]
        public int NumberOfPages { get; set; }

        [JsonProperty(PropertyName = "currentPageNumber")]
        public int CurrentPageNumber { get; set; }

        [JsonProperty(PropertyName = "aliases")]
        public List<string> Aliases { get; set; }
    }
}