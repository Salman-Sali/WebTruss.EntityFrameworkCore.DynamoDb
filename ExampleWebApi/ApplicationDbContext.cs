﻿using Amazon;
using ExampleWebApi.Configurations;
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

            Employees = new DynamoSet<Employee>(this, dynamoDbTablesConfiguration.Employees);
            SingleTable = new DynamoSet<SingleTable>(this, dynamoDbTablesConfiguration.SingleTable);
        }

        public DynamoSet<Employee> Employees { get; set; }
        public DynamoSet<SingleTable> SingleTable { get; set; }
    }
}