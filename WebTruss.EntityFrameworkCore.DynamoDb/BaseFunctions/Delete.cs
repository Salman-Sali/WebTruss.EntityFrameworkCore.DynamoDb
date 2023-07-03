using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

namespace WebTruss.EntityFrameworkCore.DynamoDb.BaseFunctions
{
    public partial class Delete
    {
        public static async Task<DeleteItemResponse> DeleteAsync(Dictionary<string, AttributeValue> keys, string tableName, AmazonDynamoDBClient client, CancellationToken cancellationToken = default)
        {
            return await client.DeleteItemAsync(tableName, keys, cancellationToken);
        }

        public static async Task<TransactWriteItemsResponse> BatchDeleteAsync(List<Dictionary<string, AttributeValue>> items, string tableName, AmazonDynamoDBClient client, CancellationToken cancellationToken = default)
        {
            var transactItems = new List<TransactWriteItem>();
            foreach (var item in items)
            {
                transactItems.Add(new TransactWriteItem
                {
                    Delete = new Amazon.DynamoDBv2.Model.Delete
                    {
                        TableName = tableName,
                        Key = item
                    }
                });
            }
            var request = new TransactWriteItemsRequest
            {
                TransactItems = transactItems,
            };

            return await client.TransactWriteItemsAsync(request, cancellationToken);
        }
    }
}
