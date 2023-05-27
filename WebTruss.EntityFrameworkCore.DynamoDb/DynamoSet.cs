using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using System.Reflection;
using WebTruss.EntityFrameworkCore.DynamoDb.Attributes;

namespace WebTruss.EntityFrameworkCore.DynamoDb
{
    public class DynamoSet<T>
    {
        private readonly string tableName;
        private readonly AmazonDynamoDBClient client;
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

        }




        private void CheckPkType(Type t)
        {
            if (pkProperty.PropertyType != t)
            {
                throw new Exception($"{typeof(T).Name} contains pk (Primary key) with type of {pkProperty.Name}. You have provided {t.Name}.");
            }
        }

        private void CheckPkAndSkType(Type pk, Type sk)
        {
            if(skProperty == null)
            {
                throw new Exception($"The model {typeof(T).Name} does not have a property with Sk (Secondary Key) attribute.");
            }
            if(pkProperty.PropertyType != pk || skProperty.PropertyType != sk)
            {
                throw new Exception($"{typeof(T).Name} contains Pk (Primary key) of type {pkProperty.Name} and Sk (Secondary key) of type {skProperty.Name}. You have provided Pk of type {pk.Name} and Sk of type {sk.Name}.");
            }
        }

        public async Task<T?> FirstOrDefaultAsync(int pk)
        {
            CheckPkType(typeof(int));
            var keys = new Dictionary<string, AttributeValue>
            {
                { propertyNames.Where(a=>a.Key == pkProperty).First().Value, new AttributeValue{ N = pk.ToString() } }
            };
            return await GetByKeysAsync(keys);
        }

        public async Task<T?> FirstOrDefaultAsync(string pk)
        {
            CheckPkType(typeof(string));
            var keys = new Dictionary<string, AttributeValue>
            {
                { propertyNames.Where(a=>a.Key == pkProperty).First().Value, new AttributeValue{ S = pk } }
            };
            return await GetByKeysAsync(keys);
        }

        public async Task<T?> FirstOrDefaultAsync(int pk, int sk)
        {
            CheckPkAndSkType(typeof(int), typeof(int));
            var keys = new Dictionary<string, AttributeValue>
            {
                { propertyNames.Where(a=>a.Key == pkProperty).First().Value, new AttributeValue{ N = pk.ToString() } },
                { propertyNames.Where(a=>a.Key == skProperty).First().Value, new AttributeValue{ N = sk.ToString() } }
            };
            return await GetByKeysAsync(keys);
        }

        public async Task<T?> FirstOrDefaultAsync(string pk, string sk)
        {
            CheckPkAndSkType(typeof(string), typeof(string));
            var keys = new Dictionary<string, AttributeValue>
            {
                { propertyNames.Where(a=>a.Key == pkProperty).First().Value, new AttributeValue{ N = pk } },
                { propertyNames.Where(a=>a.Key == skProperty).First().Value, new AttributeValue{ N = sk } }
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

            var response = await client.GetItemAsync(request);
            if (response.HttpStatusCode != System.Net.HttpStatusCode.OK)
            {
                return default(T);
            }

            var data = (T)Activator.CreateInstance(typeof(T));
            foreach (var property in typeof(T).GetProperties())
            {
                var value = Activator.CreateInstance(property.GetType());
                if (!response.Item.Where(x => x.Key == property.Name).Any())
                {
                    continue;
                }
                value = response.Item.Where(x => x.Key == property.Name).FirstOrDefault().Value;
                property.SetValue(data, value);
            }
            entities.Add(new Tracking<T>(data, EntityState.Unchanged));
            return data;
        }
    }
}
