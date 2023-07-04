using Amazon.DynamoDBv2.Model;
using Get = WebTruss.EntityFrameworkCore.DynamoDb.BaseFunctions.Get;

namespace WebTruss.EntityFrameworkCore.DynamoDb.DynamoSetFunctions
{
    public partial class DynamoSet<T>
    {
        public async Task<bool> AnyAsync<Pid>(Pid pk)
        {
            var pkDictionary = PkDictionary(pk);
            return await Get.AnyAsync(pkDictionary, entityInfo.TableName, context.Client);
        }

        public async Task<bool> AnyAsync<Pid, Sid>(Pid pk, Sid sk)
        {
            var keys = new Dictionary<string, AttributeValue>();
            var skDictionary = SkDictionary(sk).First();
            var pkDictionary = PkDictionary(pk).First();
            keys.Add(pkDictionary.Key, pkDictionary.Value);
            keys.Add(skDictionary.Key, skDictionary.Value);
            return await Get.AnyAsync(keys, entityInfo.TableName, context.Client);
        }
    }
}
