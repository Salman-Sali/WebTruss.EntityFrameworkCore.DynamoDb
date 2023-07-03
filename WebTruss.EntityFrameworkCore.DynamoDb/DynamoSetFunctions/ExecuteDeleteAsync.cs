using Amazon.DynamoDBv2.Model;
using Amazon.Runtime.Internal.Transform;
using Delete = WebTruss.EntityFrameworkCore.DynamoDb.BaseFunctions.Delete;

namespace WebTruss.EntityFrameworkCore.DynamoDb.DynamoSetFunctions
{
    public partial class DynamoSet<T>
    {
        public async Task<bool> ExecuteDeleteAsync<Pid>(Pid pk, CancellationToken cancellationToken = default)
        {
            var pkProperty = PkDictionary(pk);
            var result = await Delete.DeleteAsync(pkProperty, entityInfo.TableName, context.Client, cancellationToken);
            return result.HttpStatusCode == System.Net.HttpStatusCode.OK;
        }

        public async Task<bool> ExecuteDeleteAsync<Pid, Sid>(Pid pk, Sid sk, CancellationToken cancellationToken = default)
        {
            var keys = new Dictionary<string, AttributeValue>();
            var skDictionary = SkDictionary(sk).First();
            var pkDictionary = PkDictionary(pk).First();
            keys.Add(pkDictionary.Key, pkDictionary.Value);
            keys.Add(skDictionary.Key, skDictionary.Value);
            var result = await Delete.DeleteAsync(keys, entityInfo.TableName, context.Client, cancellationToken);
            return result.HttpStatusCode == System.Net.HttpStatusCode.OK;
        }

        public async Task<bool> ExecuteDeleteAysnc(T entity, CancellationToken cancellationToken = default)
        {
            var keys = KeyDictionary(entity);
            var result = await Delete.DeleteAsync(keys, entityInfo.TableName, context.Client, cancellationToken);
            return result.HttpStatusCode == System.Net.HttpStatusCode.OK;
        }

        public async Task ExecuteDeleteRangeAsync(List<T> entities, CancellationToken cancellationToken = default)
        {
            List<Dictionary<string, AttributeValue>> items = new List<Dictionary<string, AttributeValue>>();

            for (int i = 0, c = 1; i < entities.Count; i++, c++)
            {
                items.Add(KeyDictionary(entities[i]));
                if (c == 100)
                {
                    await Delete.BatchDeleteAsync(items, entityInfo.TableName, context.Client, cancellationToken);
                    c = 0;
                    items = new List<Dictionary<string, AttributeValue>>();
                }
            }

            if (items.Count > 0)
            {
                await Delete.BatchDeleteAsync(items, entityInfo.TableName, context.Client, cancellationToken);
            }
        }
    }
}
