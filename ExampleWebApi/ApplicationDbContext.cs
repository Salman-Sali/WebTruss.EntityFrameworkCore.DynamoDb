using Amazon;
using ExampleWebApi.Configurations;
using WebTruss.EntityFrameworkCore.DynamoDb;

namespace ExampleWebApi
{
    public class ApplicationDbContext : DynamoDbContext
    {
        public ApplicationDbContext(AWSConfiguration aWSConfiguration, DynamoDbTablesConfiguration dynamoDbTablesConfiguration) 
        {
            base.Client = new Amazon.DynamoDBv2.AmazonDynamoDBClient(aWSConfiguration.AccessKeyId, 
                aWSConfiguration.SecretKey, 
                RegionEndpoint.GetBySystemName(aWSConfiguration.Region));

            base.Tables = new Dictionary<Type, string>
            {
                { typeof(Employee), dynamoDbTablesConfiguration.Employees }
            };

            Employees = new DynamoSet<Employee>(this);
        }

        public DynamoSet<Employee> Employees { get; set; }
    }
}