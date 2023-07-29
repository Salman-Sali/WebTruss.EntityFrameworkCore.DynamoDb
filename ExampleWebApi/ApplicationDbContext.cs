using Amazon;
using ExampleWebApi.Configurations;
using ExampleWebApi.Models;
using WebTruss.EntityFrameworkCore.DynamoDb;
using WebTruss.EntityFrameworkCore.DynamoDb.DynamoSetFunctions;

namespace ExampleWebApi
{
    public class ApplicationDbContext : DynamoDbContext
    {
        public ApplicationDbContext(AWSConfiguration aWSConfiguration, DynamoDbTablesConfiguration dynamoDbTablesConfiguration) 
        {
            base.Client = new Amazon.DynamoDBv2.AmazonDynamoDBClient(aWSConfiguration.AccessKeyId, 
                aWSConfiguration.SecretKey, 
                RegionEndpoint.GetBySystemName(aWSConfiguration.Region));

            Authors = new DynamoSet<Author>(this, dynamoDbTablesConfiguration.Authors);
            Books = new DynamoSet<Book>(this, dynamoDbTablesConfiguration.Books);
            Employees = new DynamoSet<Employee>(this, dynamoDbTablesConfiguration.Employees);
            SingleTable = new DynamoSet<SingleTable>(this, dynamoDbTablesConfiguration.SingleTable);
        }

        public DynamoSet<Employee> Employees { get; set; }
        public DynamoSet<SingleTable> SingleTable { get; set; }
        public DynamoSet<Author> Authors { get; set; }
        public DynamoSet<Book> Books { get; set; }
    }
}