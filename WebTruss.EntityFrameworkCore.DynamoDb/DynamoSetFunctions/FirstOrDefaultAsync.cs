using Amazon.DynamoDBv2.Model;
using WebTruss.EntityFrameworkCore.DynamoDb.BaseFunctions;
using Get = WebTruss.EntityFrameworkCore.DynamoDb.BaseFunctions.Get;

namespace WebTruss.EntityFrameworkCore.DynamoDb.DynamoSetFunctions
{
    public partial class DynamoSet<T>
    {
        /// <summary>
        /// Get an entity by its partition key.
        /// </summary>
        public async Task<T?> FirstOrDefaultAsync<Pid>(Pid id)
        {
            var values = await Get.GetByKeysAsync(PkDictionary(id), string.Empty, entityInfo.TableName, context.Client);
            if (values == null)
            {
                return null;
            }

            return DictionaryToEntity(values);
        }

        /// <summary>
        /// Get an entity by its partition key and sort key.
        /// </summary>
        public async Task<T?> FirstOrDefaultAsync<Pid, Sid>(Pid pk, Sid sk)
        {
            var keys = new Dictionary<string, AttributeValue>();
            var skDictionary = SkDictionary(sk).First();
            var pkDictionary = PkDictionary(pk).First();
            keys.Add(pkDictionary.Key, pkDictionary.Value);
            keys.Add(skDictionary.Key, skDictionary.Value);

            var values = await Get.GetByKeysAsync(keys, string.Empty, entityInfo.TableName, context.Client);
            if (values == null)
            {
                return null;
            }

            return DictionaryToEntity(values);
        }
    }
}
