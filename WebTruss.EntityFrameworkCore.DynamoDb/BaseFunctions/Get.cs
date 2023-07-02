using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using System.Reflection;

namespace WebTruss.EntityFrameworkCore.DynamoDb.BaseFunctions
{
    public static partial class Get
    {
        public static async Task<Dictionary<string, AttributeValue>?> GetByKeysAsync(Dictionary<string, AttributeValue> keys, string projection, string tableName, AmazonDynamoDBClient client)
        {
            var request = new GetItemRequest
            {
                TableName = tableName,
                Key = keys,
                ProjectionExpression = projection
            };

            var response = await client.GetItemAsync(request);
            if (response.Item.Count == 0)
            {
                return null;
            }

            return response.Item;
        }
    }
}
