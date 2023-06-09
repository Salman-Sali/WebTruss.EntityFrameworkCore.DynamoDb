﻿using WebTruss.EntityFrameworkCore.DynamoDb.Attributes;

namespace ExampleWebApi.Models
{
    public class Employee
    {
        [Pk]
        [DynamoPropertyName("EmployeeId")]
        public string Id { get; set; } = null!;

        public string Name { get; set; } = null!;

        public DateOnly DateOfBirth { get; set; }

        public TimeOnly ShiftStartTime { get; set; }

        public EmployeeType EmployeeType { get; set; }

        public List<string>? Items { get; set; }
    }

    public enum EmployeeType
    {
        Developer,
        Finance,
        Marketing,
        Other
    }
}
