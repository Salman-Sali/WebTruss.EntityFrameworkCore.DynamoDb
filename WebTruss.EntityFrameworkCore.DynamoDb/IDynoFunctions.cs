namespace WebTruss.EntityFrameworkCore.DynamoDb
{
    public interface IDynoQueryableFunctions<T>
    {
        Task<T?> FirstOrDefaultAsync<Pid>(Pid id);
        Task<T?> FirstOrDefaultAsync<Pid, Sid>(Pid pk, Sid sk);
    }
}
