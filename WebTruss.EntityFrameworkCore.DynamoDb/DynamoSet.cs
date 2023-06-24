using Amazon.Auth.AccessControlPolicy;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using KellermanSoftware.CompareNetObjects;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Xml.Linq;
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

        /// <summary>
        /// Starts tracking entity to be added.
        /// </summary>
        /// <param name="entity"></param>
        public void Add(T entity)
        {
            entities.Add(new Tracking<T>(entity, EntityState.Added));
        }

        /// <summary>
        /// Starts tracking entity to be updated
        /// </summary>
        /// <param name="entity"></param>
        public void Update(T entity)
        {
            entities.Add(new Tracking<T>(entity, EntityState.Modified));
        }

        /// <summary>
        /// Starts tracking entity to be deleted.
        /// </summary>
        /// <param name="entity"></param>
        public void Delete(T entity)
        {
            entities.Add(new Tracking<T>(entity, EntityState.Deleted));
        }

        /// <summary>
        /// Starts tracking entity to be deleted.
        /// </summary>
        /// <typeparam name="Tid"></typeparam>
        /// <param name="pkId"></param>
        public void Delete<Tid>(Tid pkId)
        {
            CheckType(pkProperty, typeof(Tid), DynamoDbKey.Pk);
            var data = (T)Activator.CreateInstance(typeof(T));
            pkProperty.SetValue(data, pkId);
            entities.Add(new Tracking<T>(data, EntityState.Deleted));
        }


        /// <summary>
        /// Starts tracking entity to be deleted.
        /// </summary>
        /// <typeparam name="Pid"></typeparam>
        /// <typeparam name="Sid"></typeparam>
        /// <param name="pk"></param>
        /// <param name="sk"></param>
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

        private AttributeValue GetAttributeValue<Tid>(Tid id, DynamoDbKey key)
        {
            if (key == DynamoDbKey.Pk)
            {
                CheckType(pkProperty, typeof(Tid), DynamoDbKey.Pk);
            }
            else
            {
                CheckType(skProperty, typeof(Tid), DynamoDbKey.Sk);
            }

            var attributeValue = new AttributeValue();

            if (typeof(Tid) == typeof(int))
            {
                attributeValue.N = id.ToString();
            }
            else if (typeof(Tid) == typeof(string))
            {
                attributeValue.S = id.ToString();
            }

            return attributeValue;
        }

        public async Task<T?> FirstOrDefaultAsync<Tid>(Tid pk)
        {
            var keys = new Dictionary<string, AttributeValue>
            {
                { propertyNames.Where(a=>a.Key == pkProperty).First().Value, GetAttributeValue(pk, DynamoDbKey.Pk) }
            };
            return await GetByKeysAsync(keys, propertyNames.Select(x => x.Key).ToList());
        }

        public async Task<T?> FirstOrDefaultAsync<Pid, Sid>(Pid pk, Sid sk)
        {
            var keys = new Dictionary<string, AttributeValue>
            {
                { propertyNames.Where(a=>a.Key == pkProperty).First().Value, GetAttributeValue(pk, DynamoDbKey.Pk) },
                { propertyNames.Where(a=>a.Key == skProperty).First().Value, GetAttributeValue(sk, DynamoDbKey.Sk) }
            };
            return await GetByKeysAsync(keys, propertyNames.Select(x => x.Key).ToList());
        }

        private T AttributesToEntity(Dictionary<string, AttributeValue> attributes)
        {
            var data = (T)Activator.CreateInstance(typeof(T));
            foreach (var property in propertyNames)
            {
                if (!attributes.Where(x => x.Key == property.Value).Any())
                {
                    continue;
                }

                object? value = null;
                var attribute = attributes.Where(x => x.Key == property.Value).FirstOrDefault();
                if (property.Key.PropertyType == typeof(string))
                {
                    value = attribute.Value.S;
                }
                else if (property.Key.PropertyType == typeof(int))
                {
                    value = attribute.Value.N;
                }
                else if (property.Key.PropertyType == typeof(bool))
                {
                    value = attribute.Value.BOOL;
                }
                else if (property.Key.PropertyType == typeof(List<string>))
                {
                    value = attribute.Value.SS;
                }
                else if (property.Key.PropertyType == typeof(DateTime))
                {
                    value = DateTime.Parse(attribute.Value.S);
                }
                else if (property.Key.PropertyType == typeof(DateOnly))
                {
                    value = DateOnly.Parse(attribute.Value.S);
                }
                else if (property.Key.PropertyType == typeof(TimeOnly))
                {
                    value = TimeOnly.Parse(attribute.Value.S);
                }
                else
                {
                    var converter = TypeDescriptor.GetConverter(property.Key.PropertyType);
                    value = converter.ConvertFromString(attribute.Value.S);
                }
                property.Key.SetValue(data, value);
            }
            return data;
        }

        private async Task<T?> GetByKeysAsync(Dictionary<string, AttributeValue> keys, List<PropertyInfo> properties)
        {
            var attributesToGet = propertyNames.Where(x => properties.Contains(x.Key)).Select(x => x.Value).ToList();
            var request = new GetItemRequest
            {
                TableName = tableName,
                Key = keys,
                AttributesToGet = attributesToGet
            };

            var response = await context.Client.GetItemAsync(request);
            if (response.Item.Count == 0)
            {
                return null;
            }

            var data = AttributesToEntity(response.Item);

            entities.Add(new Tracking<T>(data, EntityState.Unchanged));
            return data;
        }

        private bool HasValueChanged(T entity, T original)
        {
            CompareLogic compareLogic = new CompareLogic();

            ComparisonResult result = compareLogic.Compare(entity, original);
            return !result.AreEqual;
        }


        /// <summary>
        /// Return paginated list. Can only be called on tables with composite key.
        /// </summary>
        /// <param name="pk"></param>
        /// <param name="limit"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<QueriedPaginatedResult<T>> PagedListAsync(string pk, int limit, string? token)
        {
            if (skProperty == null)
            {
                throw new Exception("PagedList can only be called on tables with composite key.");
            }

            var paginationToken = PaginationToken.FromString(token);
            var request = new QueryRequest
            {
                TableName = this.tableName,
                ScanIndexForward = paginationToken?.Forward ?? true,
                KeyConditionExpression = $"{propertyNames.Where(a=>a.Key == pkProperty).First().Value} = :v_Id",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {{":v_Id", new AttributeValue { S =  pk }}},
                Limit = limit,
                ExclusiveStartKey = paginationToken != null? new Dictionary<string, AttributeValue>
                {
                    { propertyNames.Where(a=>a.Key == pkProperty).First().Value, new AttributeValue { S = pk } },
                    { propertyNames.Where(a=>a.Key == skProperty).First().Value, new AttributeValue { S = paginationToken.Sk } }
                } : new()
            };
            var queryResult = await this.context.Client.QueryAsync(request);
            var result = new QueriedPaginatedResult<T>();

            if (queryResult.ScannedCount == 0)
            {
                return result;
            }

            if(!paginationToken?.Forward ?? false)
            {
                queryResult.Items.Reverse();
            }

            foreach (var item in queryResult.Items)
            {
                var data = AttributesToEntity(item);
                result.Items.Add(data);
            }

            var firstSk = queryResult.Items.First().Where(a => a.Key == propertyNames.Where(a => a.Key == skProperty).First().Value).First();

            if (paginationToken == null || firstSk.Value.S == paginationToken.FirstSk)
            {
                result.PreviousToken = null;
            }
            else
            {
                result.PreviousToken = (new PaginationToken(paginationToken.FirstSk, false, firstSk.Value.S)).ToString();
            }

            if (queryResult.ScannedCount != limit)
            {
                result.NextToken = null;
            }
            else if (paginationToken?.Forward ?? true)
            {
                if (queryResult.LastEvaluatedKey.Count == 0)
                {
                    result.NextToken = null;
                }
                else
                {
                    var sk = queryResult.LastEvaluatedKey.Where(a => a.Key == propertyNames.Where(a => a.Key == skProperty).First().Value).First();
                    result.NextToken = (new PaginationToken(paginationToken?.FirstSk ?? firstSk.Value.S, true, sk.Value.S)).ToString();
                }
            }
            else
            {
                var lastSk = queryResult.Items.Last().Where(a => a.Key == propertyNames.Where(a => a.Key == skProperty).First().Value).First();
                result.NextToken = (new PaginationToken(paginationToken.FirstSk, true, lastSk.Value.S)).ToString();
            }

            return result;
        }

        /// <summary>
        /// Saves currently tracking changes. Tracking will stop once saved.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            entities
                .Where(x => x.EntityState == EntityState.Unchanged && HasValueChanged(x.Entity, x.Original))
                .ToList()
                .ForEach(x => x.EntityState = EntityState.Modified);

            foreach (var entity in entities.Where(x => x.EntityState == EntityState.Deleted).ToList())
            {
                await ExecuteDeleteAsync(entity.Original);
            }

            foreach (var entity in entities.Where(x => x.EntityState == EntityState.Added || x.EntityState == EntityState.Modified).ToList())
            {
                await ExecutePutAsync(entity.Entity);
            }
            entities = new List<Tracking<T>>();
        }

        /// <summary>
        /// Add or update entity without tracking.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<bool> ExecutePutAsync(T entity, CancellationToken cancellationToken = default)
        {

            var request = new PutItemRequest
            {
                TableName = tableName,
                Item = EntityToAttributeValues(entity)
            };

            var result = await context.Client.PutItemAsync(request, cancellationToken);
            return result.HttpStatusCode == System.Net.HttpStatusCode.OK;
        }

        private Dictionary<string, AttributeValue> EntityToAttributeValues(T entity)
        {
            var item = new Dictionary<string, AttributeValue>();
            var properties = typeof(T).GetProperties();
            foreach (var property in propertyNames)
            {
                var value = property.Key.GetValue(entity);
                if (value == null)
                {
                    continue;
                }
                var attribute = new AttributeValue();
                if (property.Key.PropertyType == typeof(string))
                {
                    attribute.S = value.ToString();
                }
                else if (property.Key.PropertyType == typeof(int))
                {
                    attribute.N = value.ToString();
                }
                else if (property.Key.PropertyType == typeof(bool))
                {
                    attribute.BOOL = (bool)value;
                }
                else if (property.Key.PropertyType == typeof(List<string>))
                {
                    attribute.SS = (List<string>)value;
                }
                else
                {
                    attribute.S = value.ToString();
                }
                item.Add(property.Value, attribute);
            }
            return item;
        }

        /// <summary>
        /// Delete entity without tracking.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<bool> ExecuteDeleteAsync(T entity, CancellationToken cancellationToken = default)
        {
            var keys = new Dictionary<string, AttributeValue>();
            var pk = new AttributeValue();
            if (pkProperty.PropertyType == typeof(string))
            {
                pk.S = (string)pkProperty.GetValue(entity);
            }
            else if (pkProperty.PropertyType == typeof(int))
            {
                pk.N = (string)pkProperty.GetValue(entity);
            }
            keys.Add(propertyNames.Where(x => x.Key == pkProperty).First().Value, pk);

            if (skProperty != null)
            {
                var sk = new AttributeValue();
                if (skProperty.PropertyType == typeof(string))
                {
                    sk.S = (string)skProperty.GetValue(entity);
                }
                else if (skProperty.PropertyType == typeof(int))
                {
                    sk.N = (string)skProperty.GetValue(entity);
                }
                keys.Add(propertyNames.Where(x => x.Key == skProperty).First().Value, sk);
            }
            var result = await context.Client.DeleteItemAsync(tableName, keys, cancellationToken);
            return result.HttpStatusCode == System.Net.HttpStatusCode.OK;
        }

        public async Task<bool> ExecuteDeleteAsync<Tid>(Tid pk, CancellationToken cancellationToken = default)
        {
            var keys = new Dictionary<string, AttributeValue>();
            var primaryKey = new AttributeValue();
            if (pkProperty.PropertyType == typeof(string))
            {
                primaryKey.S = pk.ToString();
            }
            else if (pkProperty.PropertyType == typeof(int))
            {
                primaryKey.N = pk.ToString();
            }
            keys.Add(propertyNames.Where(x => x.Key == pkProperty).First().Value, primaryKey);
            var result = await context.Client.DeleteItemAsync(tableName, keys, cancellationToken);
            return result.HttpStatusCode == System.Net.HttpStatusCode.OK;
        }

        public async Task<bool> ExecuteDeleteAsync<Pid, Sid>(Pid pk, Sid sk, CancellationToken cancellationToken = default)
        {
            var keys = new Dictionary<string, AttributeValue>();
            var primaryKey = new AttributeValue();
            if (pkProperty.PropertyType == typeof(string))
            {
                primaryKey.S = pk.ToString();
            }
            else if (pkProperty.PropertyType == typeof(int))
            {
                primaryKey.N = pk.ToString();
            }
            keys.Add(propertyNames.Where(x => x.Key == pkProperty).First().Value, primaryKey);

            var secondaryKey = new AttributeValue();
            if (pkProperty.PropertyType == typeof(string))
            {
                secondaryKey.S = sk.ToString();
            }
            else if (pkProperty.PropertyType == typeof(int))
            {
                secondaryKey.N = sk.ToString();
            }
            keys.Add(propertyNames.Where(x => x.Key == skProperty).First().Value, secondaryKey);

            var result = await context.Client.DeleteItemAsync(tableName, keys, cancellationToken);
            return result.HttpStatusCode == System.Net.HttpStatusCode.OK;
        }


        /// <summary>
        /// List elements of the table. This method performs a dynamodb scan.
        /// </summary>
        /// <param name="count"></param>
        /// <param name="paginationToken"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public async Task<ScannedPaginatedResult<T>> ScanAsync(int count, string? paginationToken = null, QueryFilter? filter = null)
        {
            var table = Table.LoadTable(context.Client, tableName);
            Search search = table.Scan(new ScanOperationConfig
            {
                ConsistentRead = true,
                Limit = count,
                Select = SelectValues.AllAttributes,
                PaginationToken = paginationToken,

            });
            var searchResult = await search.GetNextSetAsync();
            var result = new ScannedPaginatedResult<T>();
            result.PaginationToken = search.PaginationToken;
            foreach (var item in searchResult)
            {
                var data = AttributesToEntity(item.ToAttributeMap());
                result.Items.Add(data);
            }
            return result;
        }

        /// <summary>
        /// Put upto 100 items to the table.
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        public async Task<bool> ExecuteAdd100Async(List<T> entities, CancellationToken cancellationToken = default)
        {
            if (entities.Count() > 100)
            {
                throw new Exception("ExecutePut100Async can only handle 100 entities at a time.");
            }

            var items = new List<TransactWriteItem>();
            foreach (var entity in entities)
            {
                items.Add(new TransactWriteItem
                {
                    Put = new Put
                    {
                        TableName = this.tableName,
                        Item = EntityToAttributeValues(entity)
                    }
                });
            }

            var request = new TransactWriteItemsRequest
            {
                TransactItems = items,
            };
            var result = await context.Client.TransactWriteItemsAsync(request);
            return result.HttpStatusCode == System.Net.HttpStatusCode.OK;
        }

        //public DynamnoQuery<T> Where(Expression<Func<T, bool>> predicate)
        //{
        //    return new DynamnoQuery<T>(this.context);
        //}

        public DynamoSelection Select<TResult>(Expression<Func<T, TResult>> selector)
        {
            List<PropertyInfo> properties = new();
            foreach (var property in ((System.Linq.Expressions.NewExpression)selector.Body).Members)
            {
                properties.Add(propertyNames.Where(x => x.Key.Name == property.Name).First().Key);
            }
            return new DynamoSelection(this, properties);
        }

        public class DynamoSelection
        {
            private readonly DynamoSet<T> dynamoSet;

            public DynamoSelection(DynamoSet<T> dynamoSet, List<PropertyInfo> properties)
            {
                this.dynamoSet = dynamoSet;
                selectedProperties = properties;
            }

            public List<PropertyInfo> selectedProperties { get; set; }

            public async Task<T?> FirstOrDefaultAsync<Tid>(Tid pk)
            {
                var keys = new Dictionary<string, AttributeValue>
                {
                    { dynamoSet.propertyNames.Where(a=>a.Key == dynamoSet.pkProperty).First().Value, dynamoSet.GetAttributeValue(pk, DynamoDbKey.Pk) }
                };
                return await dynamoSet.GetByKeysAsync(keys, selectedProperties);
            }
        }

        //public class DynamnoQuery<T>
        //{
        //    private readonly DynamoDbContext context;

        //    public DynamnoQuery(DynamoDbContext context)
        //    {
        //        this.context = context;
        //    }

        //    public void ToList()
        //    {

        //    }
        //}
    }
}
