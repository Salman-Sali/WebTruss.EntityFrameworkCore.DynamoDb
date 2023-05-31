using WebTruss.EntityFrameworkCore.DynamoDb.Attributes;

namespace ExampleWebApi
{
    public class Employee
    {
        [Pk]
        [DynamoPropertyName("EmployeeId")]
        public string Id { get; set; } = null!;

        public string Name { get; set; } = null!;
    }
}
