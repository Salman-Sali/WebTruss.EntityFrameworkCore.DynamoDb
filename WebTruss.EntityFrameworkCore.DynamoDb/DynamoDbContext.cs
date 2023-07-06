﻿using Amazon.DynamoDBv2;
using Amazon.Runtime.Internal.Transform;
using System.Reflection;
using WebTruss.EntityFrameworkCore.DynamoDb.DynamoSetFunctions;

namespace WebTruss.EntityFrameworkCore.DynamoDb
{
    public class DynamoDbContext
    {
        public AmazonDynamoDBClient Client { get; set; } = null!;

        internal string GetTableName(Type type)
        {
            foreach (var property in this.GetType().GetProperties().Where(x => x.PropertyType == type))
            {
                if (property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(DynamoSet<>))
                {
                    Type[] genericArguments = property.PropertyType.GetGenericArguments();
                    MethodInfo method = property.PropertyType.GetMethod("GetTableName")!;
                    return (string)method.Invoke(property.GetValue(this), null)!;
                }
            }
            throw new Exception($"Entity {type.Name} not found in dynamo context.");
        }
    }
}
