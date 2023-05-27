using WebTruss.EntityFrameworkCore.DynamoDb.Attributes;

namespace ExampleWebApi
{
    public class Employee
    {
        [Pk]
        [DynamoPropertyName("EmployeeId")]
        public int Id { get; set; }

        public string Name { get; set; } = null!;
    }
}
