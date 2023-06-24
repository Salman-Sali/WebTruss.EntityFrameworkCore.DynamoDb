using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO.Compression;
using System.Web;

namespace WebTruss.EntityFrameworkCore.DynamoDb
{
    public class QueryPagination
    {
        public QueryPagination(PaginationToken? prv, PaginationToken? nxt)
        {
            Previous = prv;
            Next = nxt;
        }

        public PaginationToken? Previous { get; set; }
        public PaginationToken? Next { get; set; }
    }

    public class PaginationToken
    {
        public PaginationToken(string? fs, bool fwd, string? s, int c)
        {
            FirstSk = fs;
            Forward = fwd;
            Sk = s;
            CurrentPage = c;
        }

        public static PaginationToken? FromString(string? base64String)
        {
            if (string.IsNullOrEmpty(base64String))
            {
                return null;
            }
            var decompressedJsonString = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(base64String));


            return JsonConvert.DeserializeObject<PaginationToken>(decompressedJsonString);
        }

        [JsonProperty("c")]
        public int CurrentPage { get; set; }

        [JsonProperty("fs")]
        public string? FirstSk { get; set; }

        [JsonProperty("fwd")]
        public bool Forward { get; set; }

        [JsonProperty("s")]
        public string? Sk { get; set; }

        public override string ToString()
        {
            var a =JsonConvert.SerializeObject(this);
            var bytes = System.Text.Encoding.UTF8.GetBytes(a);
            var base64String = Convert.ToBase64String(bytes);
            return base64String;
        }
    }
}
