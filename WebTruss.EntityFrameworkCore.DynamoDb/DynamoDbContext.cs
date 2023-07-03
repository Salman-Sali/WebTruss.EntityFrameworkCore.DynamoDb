using Amazon.DynamoDBv2;
using System.Reflection;

namespace WebTruss.EntityFrameworkCore.DynamoDb
{
    public class DynamoDbContext
    {
        public AmazonDynamoDBClient Client { get; set; } = null!;
    }
}
