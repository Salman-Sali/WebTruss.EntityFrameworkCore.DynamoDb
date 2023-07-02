using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

namespace WebTruss.EntityFrameworkCore.DynamoDb.BaseFunctions
{
    public static partial class Put
    {
        public static async Task<TransactWriteItemsResponse> BatchPutAsync(List<Dictionary<string, AttributeValue>> items, string tableName, AmazonDynamoDBClient client, CancellationToken cancellationToken = default)
        {
            var transactItems = new List<TransactWriteItem>();
            foreach (var item in items)
            {
                transactItems.Add(new TransactWriteItem
                {
                    Put = new Amazon.DynamoDBv2.Model.Put
                    {
                        TableName = tableName,
                        Item = item
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
