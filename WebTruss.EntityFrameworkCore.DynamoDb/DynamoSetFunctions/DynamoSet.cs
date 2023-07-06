using Amazon.DynamoDBv2.Model;
using Amazon.Runtime.Internal.Transform;
using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace WebTruss.EntityFrameworkCore.DynamoDb.DynamoSetFunctions
{
    public partial class DynamoSet<T> : IDynoQueryableFunctions<T> where T : class
    {
        private readonly DynamoDbContext context;
        private EntityInfo entityInfo;

        private static List<Type> numericTypes = new List<Type> { typeof(int), typeof(long), typeof(double), typeof(float) };

        public DynamoSet(DynamoDbContext context, string tableName)
        {
            this.context = context;
            entityInfo = new EntityInfo(typeof(T), tableName, context);
        }

        public string GetTableName()
        {
            return entityInfo.TableName;
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

        private Dictionary<string, AttributeValue> KeyDictionary(T entity)
        {
            var keyDictionary = PkDictionary(entityInfo.Pk().Property.GetValue(entity));
            var sk = entityInfo.Sk();
            if (sk != null)
            {
                var skDictionary = SkDictionary(sk.Property.GetValue(entity));
                keyDictionary.Add(skDictionary.First().Key, skDictionary.First().Value);
            }
            return keyDictionary;
        }

        private Dictionary<string, AttributeValue> GetKeyDictionary<V>(DynamoPropertyInfo dynamoProperty, V value)
        {
            if (numericTypes.Contains(dynamoProperty.Property.PropertyType))
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

        private Dictionary<string, AttributeValue> GetPropertyDictionary(DynamoPropertyInfo dynamoProperty, object? value)
        {
            if (value == null)
            {
                return new Dictionary<string, AttributeValue>();
            }

            var propertyType = dynamoProperty.Property.PropertyType;
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
                var propertyDictionary = GetPropertyDictionary(property, property.Property.GetValue(entity));
                if (propertyDictionary.Count != 0)
                {
                    dictionary.Add(propertyDictionary.First().Key, propertyDictionary.First().Value);
                }
            }
            return dictionary;
        }

        private object? ExtractValueFromAttributeValue(DynamoPropertyInfo propertyInfo, AttributeValue attributeValue)
        {
            var type = propertyInfo.Property.PropertyType;
            if (type == typeof(string))
            {
                return attributeValue.S;
            }
            else if (type == typeof(int))
            {
                return int.Parse(attributeValue.N);
            }
            else if (type == typeof(bool))
            {
                return attributeValue.BOOL;
            }
            else if (type == typeof(List<string>))
            {
                return attributeValue.SS;
            }
            else if (type == typeof(DateTime))
            {
                return DateTime.Parse(attributeValue.S);
            }
            else if (type == typeof(DateOnly))
            {
                return DateOnly.Parse(attributeValue.S);
            }
            else if (type == typeof(TimeOnly))
            {
                return TimeOnly.Parse(attributeValue.S);
            }
            else
            {
                var converter = TypeDescriptor.GetConverter(type);
                return converter.ConvertFromString(attributeValue.S);
            }
        }

        private T DictionaryToEntity(Dictionary<string, AttributeValue> dictionary)
        {
            var data = (T)FormatterServices.GetUninitializedObject(typeof(T));
            foreach (var item in dictionary)
            {
                var property = entityInfo.Properties.Where(x => x.Name == item.Key).FirstOrDefault();
                if (property == null)
                {
                    continue;
                }

                var value = ExtractValueFromAttributeValue(property, item.Value);
                property.Property.SetValue(data, value);
            }
            return data;
        }
    }
}
