using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using System.Threading;

namespace WebTruss.EntityFrameworkCore.DynamoDb.BaseFunctions
{
    public static partial class Put
    {
        public static async Task<PutItemResponse> PutAsync(Dictionary<string, AttributeValue> values, string tableName, AmazonDynamoDBClient client, CancellationToken cancellationToken = default)
        {
            var request = new PutItemRequest
            {
                TableName = tableName,
                Item = values
            };
            return await client.PutItemAsync(request, cancellationToken);
        }
    }
}
