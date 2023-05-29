using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using WebTruss.EntityFrameworkCore.DynamoDb.Attributes;

namespace WebTruss.EntityFrameworkCore.DynamoDb
{
    public class DynamoSet<T> : IDynoSour where T : class
    {
        private readonly string tableName;
        private readonly DynamoDbContext context;
        public List<Tracking<T>> entities;
        private PropertyInfo? pkProperty;
        private PropertyInfo? skProperty;
        private Dictionary<PropertyInfo, string> propertyNames;

        public DynamoSet(DynamoDbContext context)
        {
            this.context = context;

            KeyValuePair<Type, string>? table = context.Tables.Where(x => x.Key == typeof(T)).FirstOrDefault();
            if (table == null)
            {
                throw new System.Exception($"No table name found for type of {nameof(T)}");
            }
            tableName = table.Value.Value;

            skProperty = typeof(T)
               .GetProperties()
               .FirstOrDefault(prop => Attribute.IsDefined(prop, typeof(Sk)));

            pkProperty = typeof(T)
               .GetProperties()
               .FirstOrDefault(prop => Attribute.IsDefined(prop, typeof(Pk)));
            if (pkProperty == null)
            {
                throw new Exception($"Property with Pk attribute not found for entity {typeof(T).Name}.");
            }

            propertyNames = new Dictionary<PropertyInfo, string>();
            foreach (var property in typeof(T).GetProperties())
            {
                var dynamoPropertyName = property.GetCustomAttribute<DynamoPropertyName>();
                if (dynamoPropertyName == null)
                {
                    propertyNames.Add(property, property.Name);
                }
                else
                {
                    propertyNames.Add(property, dynamoPropertyName.Name);
                }
            }

            entities = new List<Tracking<T>>();
        }

        public void Add(T entity)
        {
            entities.Add(new Tracking<T>(entity, EntityState.Added));
        }

        public void Update(T entity)
        {
            entities.Add(new Tracking<T>(entity, EntityState.Modified));
        }

        public void Delete(T entity)
        {
            entities.Add(new Tracking<T>(entity, EntityState.Deleted));
        }

        public void Delete<Tid>(Tid pkId)
        {
            CheckType(pkProperty, typeof(Tid), DynamoDbKey.Pk);
            var data = (T)Activator.CreateInstance(typeof(T));
            pkProperty.SetValue(data, pkId);
            entities.Add(new Tracking<T>(data, EntityState.Deleted));
        }

        public void Delete<Pid, Sid>(Pid pk, Sid sk)
        {
            CheckType(pkProperty, typeof(Pid), DynamoDbKey.Pk);
            CheckType(skProperty, typeof(Sid), DynamoDbKey.Sk);

            var data = (T)Activator.CreateInstance(typeof(T));
            pkProperty.SetValue(data, pk);
            skProperty.SetValue(data, sk);

            entities.Add(new Tracking<T>(data, EntityState.Deleted));
        }

        private void CheckType(PropertyInfo? property, Type t, DynamoDbKey key)
        {
            if (property == null)
            {
                throw new Exception($"{typeof(T).Name} does not contain a property with {key.ToString()}.");
            }

            if (property.PropertyType != t)
            {
                throw new Exception($"{typeof(T).Name} contains {key.ToString()} {property.Name} with type of {property.PropertyType.Name}. You have provided {t.Name}.");
            }
        }

        public async Task<T?> FirstOrDefaultAsync<Tid>(Tid pk)
        {
            CheckType(pkProperty, typeof(Tid), DynamoDbKey.Pk);
            var pkAttributeValue = new AttributeValue();
            if (typeof(Tid) == typeof(int))
            {
                pkAttributeValue.N = pk.ToString();
            }
            else if (typeof(Tid) == typeof(string))
            {
                pkAttributeValue.S = pk.ToString();
            }
            var keys = new Dictionary<string, AttributeValue>
            {
                { propertyNames.Where(a=>a.Key == pkProperty).First().Value, pkAttributeValue }
            };
            return await GetByKeysAsync(keys);
        }

        public async Task<T?> FirstOrDefaultAsync<Pid, Sid>(Pid pk, Sid sk)
        {
            CheckType(pkProperty, typeof(Pid), DynamoDbKey.Pk);
            CheckType(skProperty, typeof(Sid), DynamoDbKey.Sk);

            var pkAttributeValue = new AttributeValue();
            if (typeof(Pid) == typeof(int))
            {
                pkAttributeValue.N = pk.ToString();
            }
            else if (typeof(Pid) == typeof(string))
            {
                pkAttributeValue.S = pk.ToString();
            }

            var skAttributeValue = new AttributeValue();
            if (typeof(Sid) == typeof(int))
            {
                skAttributeValue.N = sk.ToString();
            }
            else if (typeof(Sid) == typeof(string))
            {
                skAttributeValue.S = sk.ToString();
            }

            var keys = new Dictionary<string, AttributeValue>
            {
                { propertyNames.Where(a=>a.Key == pkProperty).First().Value, pkAttributeValue },
                { propertyNames.Where(a=>a.Key == skProperty).First().Value, skAttributeValue }
            };
            return await GetByKeysAsync(keys);
        }

        private async Task<T?> GetByKeysAsync(Dictionary<string, AttributeValue> keys)
        {
            var request = new GetItemRequest
            {
                TableName = tableName,
                Key = keys
            };

            var response = await context.Client.GetItemAsync(request);
            if (response.HttpStatusCode != System.Net.HttpStatusCode.OK)
            {
                return default(T);
            }

            var data = (T)Activator.CreateInstance(typeof(T));
            foreach (var property in propertyNames)
            {
                if (!response.Item.Where(x => x.Key == property.Value).Any())
                {
                    continue;
                }

                object? value = null;
                if (property.Key.PropertyType == typeof(string))
                {
                    value = response.Item.Where(x => x.Key == property.Value).FirstOrDefault().Value.S;
                }
                property.Key.SetValue(data, value);
            }
            entities.Add(new Tracking<T>(data, EntityState.Unchanged));
            return data;
        }

        private bool HasValueChanged(T entity, T original) 
        {
            return entity != original;
        }

        public async Task SaveChangesAsync()
        {
            entities
                .Where(x=> x.EntityState == EntityState.Unchanged && HasValueChanged(x.Entity,x.Original))
                .ToList()
                .ForEach(x=> x.EntityState = EntityState.Modified);
            await Task.CompletedTask;
        }
    }
}
