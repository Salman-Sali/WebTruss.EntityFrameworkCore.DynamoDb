using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace WebTruss.EntityFrameworkCore.DynamoDb.BaseFunctions
{
    public static partial class Get
    {
        public static async Task<QueryResponse> GetPagedList(GetPagedListRequest getPageListRequest)
        {
            var request = new QueryRequest
            {
                TableName = getPageListRequest.TableName,
                ScanIndexForward = getPageListRequest.ScanIndexForward,
                KeyConditionExpression = $"{getPageListRequest.PkName} = :v_Id",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    {":v_Id", getPageListRequest.PkValue}
                },
                Limit = getPageListRequest.Limit,
                ExclusiveStartKey = getPageListRequest.ExclusiveStartKey
            };
            return await getPageListRequest.Client.QueryAsync(request);
        }
    }

    public class GetPagedListRequest
    {
        public string PkName { get; set; } = null!;
        public AttributeValue PkValue { get; set; } = null!;
        public int Limit { get; set; }
        public string TableName { get; set; } = null!;
        public bool ScanIndexForward { get; set; }
        public AmazonDynamoDBClient Client { get; set; } = null!;
        public Dictionary<string, AttributeValue> ExclusiveStartKey { get; set; } = null!;
    }
}
