using Amazon.DynamoDBv2;

namespace WebTruss.EntityFrameworkCore.DynamoDb
{
    public class DynamoDbContext
    {
        public AmazonDynamoDBClient Client { get; set; }
        public Dictionary<Type, string> Tables { get; set; }

        public void SaveChangesAsync()
        {

        }
    }
}
