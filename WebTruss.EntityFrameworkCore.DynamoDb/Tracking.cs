using System.Reflection;
using System.Text.Json;

namespace WebTruss.EntityFrameworkCore.DynamoDb
{
    public class Tracking<T> 
    {
        public Tracking(T entity, EntityState entityState)
        {
            Entity = entity;
            Original = JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(entity));
            EntityState = entityState;
        }

        public T Entity { get; set; }
        public T Original { get; set; }
        public EntityState EntityState { get; set; }
    }
}
