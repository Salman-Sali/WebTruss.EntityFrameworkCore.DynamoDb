﻿using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using System.Reflection;

namespace WebTruss.EntityFrameworkCore.DynamoDb.BaseFunctions
{
    public static partial class Get
    {
        public static async Task<Dictionary<string, AttributeValue>?> GetByKeysAsync(Dictionary<string, AttributeValue> keys, string projection, string tableName, AmazonDynamoDBClient client, Dictionary<string, string>? expressionAttributeNames = null)
        {
            var request = new GetItemRequest
            {
                TableName = tableName,
                Key = keys,
                ProjectionExpression = string.IsNullOrEmpty(projection) ? null : projection,
                ExpressionAttributeNames = expressionAttributeNames
            };

            var response = await client.GetItemAsync(request);
            if (response.Item.Count == 0)
            {
                return null;
            }

            return response.Item;
        }

        public static async Task<bool> AnyAsync(Dictionary<string, AttributeValue> keys, string tableName, AmazonDynamoDBClient client)
        {//TODO Replace with ConditionCheck
            var request = new GetItemRequest
            {
                TableName = tableName,
                Key = keys,
                ProjectionExpression = keys.First().Key
            };

            var response = await client.GetItemAsync(request);

            return response.Item.Count != 0;
        }
    }
}
