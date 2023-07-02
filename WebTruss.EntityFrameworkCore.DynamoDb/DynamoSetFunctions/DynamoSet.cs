using Amazon.DynamoDBv2.Model;
using Amazon.Runtime.Internal.Transform;
using System;

namespace WebTruss.EntityFrameworkCore.DynamoDb.DynamoSetFunctions
{
    public partial class DynamoSet<T> : IDynoFunctions<T> where T : class
    {
        private readonly DynamoDbContext context;
        private EntityInfo entityInfo;

        private static List<Type> numericTypes = new List<Type> { typeof(int), typeof(long), typeof(double), typeof(float) };

        public DynamoSet(DynamoDbContext context, string tableName)
        {
            this.context = context;
            entityInfo = new EntityInfo(typeof(T), tableName);
        }

        private Dictionary<string, AttributeValue> PkDictionary<Pid>(Pid pk)
        {
            return GetKeyDictionary(entityInfo.Pk(), pk);
        }

        private Dictionary<string, AttributeValue> SkDictionary<Sid>(Sid sk)
        {
            var skProperty = entityInfo.Sk();
            if (skProperty == null)
            {
                throw new Exception("The dynamo entity does not have a property mapped with sk attribute.");
            }

            return GetKeyDictionary(skProperty, sk);
        }

        private Dictionary<string, AttributeValue> GetKeyDictionary<V>(DynamoPropertyInfo dynamoProperty, V value)
        {
            if (numericTypes.Contains(dynamoProperty.Property.GetType()))
            {
                return new Dictionary<string, AttributeValue>
                {
                    { dynamoProperty.Name, new AttributeValue { N = value!.ToString() } }
                };
            }
            else
            {
                return new Dictionary<string, AttributeValue>
                {
                    { dynamoProperty.Name,  new AttributeValue { S = value!.ToString() } }
                };
            }
        }

        private Dictionary<string, AttributeValue> GetPropertyDictionary(DynamoPropertyInfo dynamoProperty, object value)
        {
            if (value == null)
            {
                return new Dictionary<string, AttributeValue>();
            }

            var propertyType = dynamoProperty.Property.GetType();
            if (numericTypes.Contains(propertyType))
            {
                return new Dictionary<string, AttributeValue>
                {
                    { dynamoProperty.Name, new AttributeValue { N = value!.ToString() } }
                };
            }
            else if (propertyType == typeof(bool))
            {
                return new Dictionary<string, AttributeValue>
                {
                    { dynamoProperty.Name, new AttributeValue { BOOL = (bool)value } }
                };
            }
            else if (propertyType == typeof(List<string>))
            {
                return new Dictionary<string, AttributeValue>
                {
                    { dynamoProperty.Name, new AttributeValue { SS = (List<string>)value } }
                };
            }
            else
            {
                return new Dictionary<string, AttributeValue>
                {
                    { dynamoProperty.Name, new AttributeValue { S = value!.ToString() } }
                };
            }
        }

        private Dictionary<string, AttributeValue> EntityToDictionary(T entity)
        {
            var dictionary = new Dictionary<string, AttributeValue>();
            foreach (var property in entityInfo.Properties)
            {
                dictionary.Concat(GetPropertyDictionary(property, property.Property.GetValue(entity) ?? new AttributeValue()));
            }
            return dictionary;
        }

        private T DictionaryToEntity(Dictionary<string, AttributeValue> dictionary)
        {
            var data = (T)Activator.CreateInstance(typeof(T));
        }
    }
}
