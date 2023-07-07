using Amazon.DynamoDBv2.Model;
using WebTruss.EntityFrameworkCore.DynamoDb.BaseFunctions;
using Get = WebTruss.EntityFrameworkCore.DynamoDb.BaseFunctions.Get;

namespace WebTruss.EntityFrameworkCore.DynamoDb.DynamoSetFunctions
{
    public partial class DynamoSet<T>
    {
        public async Task<T?> FirstOrDefaultLinkedAsync<Pid>(Pid pk)
        {
            var values = await Get.GetByKeysAsync(PkDictionary(pk), string.Empty, entityInfo.TableName, context.Client);
            if (values == null)
            {
                return null;
            }

            var result = DictionaryToEntity(values);

            foreach (var entity in entityInfo.LinkedEntities)
            {
                var entityValues = new Dictionary<string, AttributeValue>();
                KeyValuePair<string, AttributeValue>? linkedValue = values.Where(x => x.Key == entity.LinkingProperty.Name).FirstOrDefault();
                if (linkedValue == null)
                {
                    continue;
                }

                var keys = new Dictionary<string, AttributeValue>();
                var projection = string.Join(", ", entity.Properties.Select(x => x.Name).ToList());
                keys.Add(linkedValue.Value.Key, linkedValue.Value.Value);
                if (entity.PkValue != null)
                {
                    var pkDictionary = GetPropertyDictionary(entity.PkPropertyInfo, entity.PkValue).First();
                    keys.Add(pkDictionary.Key, pkDictionary.Value);
                }
                entityValues = await Get.GetByKeysAsync(keys, projection, entity.TableName, context.Client);
                if (entityValues != null)
                {
                    result = AppendValues(result, entityValues, entity.Properties);
                }
            }

            return result;
        }

        //public async Task FirstOrDefaultLinkedAsync<Pid, Sid>(Pid pk, Sid sk)
        //{

        //}
    }
}
