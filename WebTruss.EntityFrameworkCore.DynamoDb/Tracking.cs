namespace WebTruss.EntityFrameworkCore.DynamoDb
{
    public class Tracking<T>
    {
        public Tracking(T entity, EntityState entityState)
        {
            Entity = entity;
            EntityState = entityState;
        }

        public T Entity { get; set; }
        public EntityState EntityState { get; set; }
    }
}
