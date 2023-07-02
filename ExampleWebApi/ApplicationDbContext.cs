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
                { typeof(Employee), dynamoDbTablesConfiguration.Employees },
                { typeof(SingleTable), dynamoDbTablesConfiguration.SingleTable }
            };

            Employees = new OldDynamoSet<Employee>(this);
            SingleTable = new OldDynamoSet<SingleTable>(this);
        }

        public OldDynamoSet<Employee> Employees { get; set; }
        public OldDynamoSet<SingleTable> SingleTable { get; set; }
    }
}