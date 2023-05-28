using Amazon.DynamoDBv2;

namespace WebTruss.EntityFrameworkCore.DynamoDb
{
    public class DynamoDbContext : IDynoSour
    {
        public AmazonDynamoDBClient Client { get; set; }
        public Dictionary<Type, string> Tables { get; set; }
        public string? MetaDataTable { get; set; }

        public void SaveChangesAsync()
        {
            var dynamoSets = this.GetType()
                .GetProperties()
                .Where(x => x.PropertyType.IsGenericType &&
                           x.PropertyType.GetGenericTypeDefinition() == typeof(DynamoSet<>))
                .ToList();

            foreach (var dynamoSet in dynamoSets)
            {
                var  value = dynamoSet.GetValue(this);
                var method = typeof(IDynoSour).GetMethod("SaveChangesAsync");
                method.Invoke(value, null);
            }
        }
    }
}
