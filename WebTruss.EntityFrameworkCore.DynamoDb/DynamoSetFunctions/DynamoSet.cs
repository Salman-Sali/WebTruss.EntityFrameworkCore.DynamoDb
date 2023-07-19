using Amazon.DynamoDBv2.Model;
using Amazon.Runtime.Internal.Transform;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.Serialization;

namespace WebTruss.EntityFrameworkCore.DynamoDb.DynamoSetFunctions
{
    public partial class DynamoSet<T> : IDynoQueryableFunctions<T> where T : class
    {
        private readonly DynamoDbContext context;
        private EntityInfo entityInfo;

        private static List<Type> numericTypes = new List<Type> { typeof(int), typeof(long), typeof(double), typeof(float) };
        private static List<Type> stringTypes = new List<Type> { typeof(string), typeof(Enum), typeof(Guid), typeof(DateTime), typeof(TimeOnly), typeof(DateOnly) };

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
            return new Dictionary<string, AttributeValue>
            {
                { dynamoProperty.Name, GetAttributeValue(dynamoProperty.Property.PropertyType, value) }
            };
        }

        private AttributeValue GetAttributeValue(Type type, object value)
        {
            if (numericTypes.Contains(type))
            {
                return new AttributeValue { N = value!.ToString() };
            }
            else if (type == typeof(bool))
            {
                return new AttributeValue { BOOL = (bool)value };
            }
            else if (type == typeof(List<string>))
            {
                return new AttributeValue { SS = (List<string>)value };
            }
            else if (stringTypes.Contains(type))
            {
                return new AttributeValue { S = value!.ToString() };
            }
            else if (type.IsClass)
            {
                if (type.Namespace == typeof(List<>).Namespace)
                {
                    var list = new List<AttributeValue>();
                    var listType = type.GetGenericArguments()[0];
                    MethodInfo toArrayMethod = type.GetMethod("ToArray");
                    Array array = (Array)toArrayMethod.Invoke(value, null);
                    foreach (object item in array)
                    {
                        list.Add(GetAttributeValue(listType, item));
                    }
                    return new AttributeValue { L = list };
                }
                else
                {
                    var mappings = new Dictionary<string, AttributeValue>();
                    foreach (var prop in type.GetProperties())
                    {
                        mappings.Add(prop.Name, GetAttributeValue(prop.PropertyType, prop.GetValue(value)));
                    }
                    return new AttributeValue { M = mappings };
                }
            }
            else
            {
                return new AttributeValue { S = value!.ToString() };
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

        private object? ExtractValueFromAttributeValue(Type type, AttributeValue attributeValue)
        {
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
            else if (type.IsClass)
            {
                if (type.Namespace == typeof(List<>).Namespace)
                {
                    var listType = type.GetGenericArguments()[0];
                    var result = Activator.CreateInstance(type);
                    foreach (var attribute in attributeValue.L)
                    {
                        var value = ExtractValueFromAttributeValue(listType, attribute);
                        MethodInfo? addMethod = type.GetMethod("Add");
                        if (addMethod != null)
                        {
                            addMethod.Invoke(result, new object[] { value });
                        }
                    }
                    return result;
                }
                else
                {
                    var result = Activator.CreateInstance(type);
                    foreach (var attributeDictionary in attributeValue.M)
                    {
                        var property = type.GetProperty(attributeDictionary.Key);
                        if (property != null)
                        {
                            property.SetValue(result, ExtractValueFromAttributeValue(property.PropertyType, attributeDictionary.Value));
                        }
                    }
                    return result;
                }
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

                var value = ExtractValueFromAttributeValue(property.Property.PropertyType, item.Value);
                property.Property.SetValue(data, value);
            }
            return data;
        }

        private T AppendValues(T entity, Dictionary<string, AttributeValue> values, List<DynamoPropertyInfo> properties)
        {
            foreach (var value in values)
            {
                var property = properties.Where(x => x.Name == value.Key).FirstOrDefault();
                if (property == null)
                {
                    continue;
                }

                var attributeValue = ExtractValueFromAttributeValue(property.Property.PropertyType, value.Value);
                property.Property.SetValue(entity, attributeValue);
            }
            return entity;
        }
    }
}
