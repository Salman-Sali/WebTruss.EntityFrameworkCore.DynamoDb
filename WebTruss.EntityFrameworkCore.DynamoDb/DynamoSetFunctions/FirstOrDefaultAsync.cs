using WebTruss.EntityFrameworkCore.DynamoDb.BaseFunctions;

namespace WebTruss.EntityFrameworkCore.DynamoDb.DynamoSetFunctions
{
    public partial class DynamoSet<T>
    {
        public async Task<T?> FirstOrDefaultAsync<Pid>(Pid id)
        {
            var values = await Get.GetByKeysAsync(PkDictionary(id), string.Empty, entityInfo.TableName, context.Client);
        }

        public Task<T?> FirstOrDefaultAsync<Pid, Sid>(Pid pk, Sid sk)
        {
            throw new NotImplementedException();
        }
    }
}
