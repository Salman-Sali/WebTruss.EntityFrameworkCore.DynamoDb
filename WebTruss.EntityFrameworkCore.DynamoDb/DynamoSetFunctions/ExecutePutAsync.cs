using Amazon.DynamoDBv2.Model;
using WebTruss.EntityFrameworkCore.DynamoDb.BaseFunctions;
using Put = WebTruss.EntityFrameworkCore.DynamoDb.BaseFunctions.Put;

namespace WebTruss.EntityFrameworkCore.DynamoDb.DynamoSetFunctions
{
    public partial class DynamoSet<T>
    {
        public async Task<bool> ExecutePutAsync(T entity, CancellationToken cancellationToken = default)
        {
            var dictionary = EntityToDictionary(entity);

            var result = await Put.PutAsync(dictionary, entityInfo.TableName, context.Client, cancellationToken);

            return result.HttpStatusCode == System.Net.HttpStatusCode.OK;
        }

        public async Task ExecutePutRangeAsync(List<T> entities, CancellationToken cancellationToken = default)
        {
            List<Dictionary<string, AttributeValue>> items = new List<Dictionary<string, AttributeValue>>();

            for(int i = 0, c = 1; i< entities.Count; i++, c++)
            {
                items.Add(EntityToDictionary(entities[i]));
                if(c == 100)
                {
                    await Put.BatchPutAsync(items, entityInfo.TableName, context.Client, cancellationToken);
                    c = 0;
                    items = new List<Dictionary<string, AttributeValue>>();
                }
            }

            if(items.Count > 0)
            {
                await Put.BatchPutAsync(items, entityInfo.TableName, context.Client, cancellationToken);
            }

        }
    }
}
