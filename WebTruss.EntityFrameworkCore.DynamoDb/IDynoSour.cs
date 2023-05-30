namespace WebTruss.EntityFrameworkCore.DynamoDb
{
    public interface IDynoSour
    {
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
