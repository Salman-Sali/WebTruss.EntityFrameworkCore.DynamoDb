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
        public PaginationToken(string? fs, bool fwd, string? s)
        {
            FirstSk = fs;
            Forward = fwd;
            Sk = s;
        }

        public static PaginationToken? FromString(string base64String)
        {
            if (string.IsNullOrEmpty(base64String))
            {
                return null;
            }
            var decompressedBytes = Decompress(Convert.FromBase64String(base64String));
            var decompressedJsonString = System.Text.Encoding.UTF8.GetString(decompressedBytes);


            return JsonConvert.DeserializeObject<PaginationToken>(decompressedJsonString);
        }

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
            var compressedBytes = Compress(bytes);
            var base64String = Convert.ToBase64String(compressedBytes);
            return base64String;
        }

        public static byte[] Compress(byte[] bytes)
        {
            using (var compressedStream = new MemoryStream())
            {
                using (var zipStream = new GZipStream(compressedStream, CompressionMode.Compress))
                {
                    zipStream.Write(bytes, 0, bytes.Length);
                }
                return compressedStream.ToArray();
            }
        }

        public static byte[] Decompress(byte[] bytes)
        {
            using (var compressedStream = new MemoryStream(bytes))
            using (var zipStream = new GZipStream(compressedStream, CompressionMode.Decompress))
            using (var resultStream = new MemoryStream())
            {
                zipStream.CopyTo(resultStream);
                return resultStream.ToArray();
            }
        }

    }
}
