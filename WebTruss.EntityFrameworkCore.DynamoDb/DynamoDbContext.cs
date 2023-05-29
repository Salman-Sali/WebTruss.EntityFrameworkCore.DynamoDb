using Amazon.DynamoDBv2;
using System.Reflection;

namespace WebTruss.EntityFrameworkCore.DynamoDb
{
    public class DynamoDbContext : IDynoSour
    {
        public AmazonDynamoDBClient Client { get; set; } = null!;
        public Dictionary<Type, string> Tables { get; set; } = null!;
        public string? MetaDataTable { get; set; }

        public async Task SaveChangesAsync()
        {
            var dynamoSets = this.GetType()
                .GetProperties()
                .Where(x => x.PropertyType.IsGenericType &&
                           x.PropertyType.GetGenericTypeDefinition() == typeof(DynamoSet<>))
                .ToList();

            foreach (var dynamoSet in dynamoSets)
            {
                var  value = dynamoSet.GetValue(this);
                MethodInfo method = typeof(IDynoSour).GetMethod("SaveChangesAsync")!;
                await (Task)method.Invoke(value, null)!;
            }
        }
    }
}
