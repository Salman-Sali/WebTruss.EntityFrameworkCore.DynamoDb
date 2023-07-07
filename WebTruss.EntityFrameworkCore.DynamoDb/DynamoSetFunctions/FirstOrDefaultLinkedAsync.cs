using Amazon.DynamoDBv2.Model;
using Amazon.Runtime.Internal.Transform;
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
                KeyValuePair<string, AttributeValue>? linkedValue = values.Where(x => x.Key == entity.LinkingName).FirstOrDefault();
                if (linkedValue == null)
                {
                    continue;
                }

                var keys = new Dictionary<string, AttributeValue>();
                string projection = string.Empty;
                Dictionary<string, string>? expressionAttributes = null;
                foreach (var property in entity.Properties)
                {
                    var exp = "#" + property.Name.ToCharArray().First();
                    if (string.IsNullOrEmpty(projection))
                    {
                        projection = exp;
                    }
                    else
                    {
                        projection += ", " + exp;
                    }
                    if (expressionAttributes == null)
                    {
                        expressionAttributes = new Dictionary<string, string>
                        {
                            { exp, property.Name }
                        };
                    }
                    else
                    {
                        expressionAttributes.Add(exp, property.Name);
                    }
                }
                if (entity.PkValue != null)
                {
                    var pkDictionary = GetPropertyDictionary(entity.PkPropertyInfo, entity.PkValue).First();
                    keys.Add(pkDictionary.Key, pkDictionary.Value);

                    var skDictionary = new Dictionary<string, AttributeValue> 
                    {
                        { entity.SkPropertyInfo.Name, linkedValue.Value.Value }
                    }.First();
                    keys.Add(skDictionary.Key, skDictionary.Value);
                }
                else
                {
                    var pkDictionary = new Dictionary<string, AttributeValue>
                    {
                        { entity.PkPropertyInfo.Name, linkedValue.Value.Value }
                    }.First();
                    keys.Add(pkDictionary.Key, pkDictionary.Value);
                }

                entityValues = await Get.GetByKeysAsync(keys, projection, entity.TableName, context.Client, expressionAttributes);
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
