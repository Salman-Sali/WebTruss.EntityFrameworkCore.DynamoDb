using WebTruss.EntityFrameworkCore.DynamoDb;

namespace ExampleWebApi
{
    public class ApplicationDbContext : DynamoDbContext
    {
        public ApplicationDbContext() 
        {
            Client = new Amazon.DynamoDBv2.AmazonDynamoDBClient();
            Tables = new Dictionary<Type, string>
            {
                { typeof(Employee), "Employees" }
            };
        }

        public DynamoSet<Employee> Employees => new DynamoSet<Employee>(this);
    }
}
