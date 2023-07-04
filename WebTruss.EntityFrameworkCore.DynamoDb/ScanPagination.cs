using Newtonsoft.Json;

namespace WebTruss.EntityFrameworkCore.DynamoDb
{
    public class ScanPagination
    {
        public static List<string> ListFromToken(string? token)
        {
            List<string> tokens = new List<string>();

            if (!string.IsNullOrEmpty(token))
            {
                var jsonString = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(token));
                tokens = JsonConvert.DeserializeObject<List<string>>(jsonString)!;
            }
            return tokens;
        }

        public static string? TokenFromList(List<string> list)
        {
            if(list == null || list.Count == 0)
            {
                return null;
            }

            var a = JsonConvert.SerializeObject(list);
            var bytes = System.Text.Encoding.UTF8.GetBytes(a);
            var base64String = Convert.ToBase64String(bytes);
            return base64String;
        }
    }
}
