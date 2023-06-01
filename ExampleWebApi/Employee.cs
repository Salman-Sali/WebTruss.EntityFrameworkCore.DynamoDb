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

    public class ABC
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public int Balance { get; set; }
    }

    public class Employee2
    {
        public Employee2()
        {
            var a = new List<ABC>
            {
                new ABC{ Age = 1, Balance = 1, Name = "342" },
                new ABC{ Age = 1, Balance = 1, Name = "342" },
                new ABC{ Age = 1, Balance = 1, Name = "342" },
            };

            var aaa = a.Select(x => new { x.Name }).First();
        }
    }
}
