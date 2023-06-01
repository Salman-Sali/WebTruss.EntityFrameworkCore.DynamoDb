﻿using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ExampleWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        private readonly ApplicationDbContext context;

        public EmployeesController(ApplicationDbContext context)
        {
            this.context = context;
            var a = context.Employees.Select(x => new { x.Name }).FirstOrDefaultAsync("Emp-1").Result;
            var asdsf = 1;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetEmployeeById(string id)
        {
            var employee = await context.Employees.FirstOrDefaultAsync(id);
            if (employee == null)
            {
                return this.NotFound();
            }
            return this.Ok(employee);
        }

        [HttpGet]
        public async Task<IActionResult> GetEmployees(int limit, string? paginationToken)
        {
            return this.Ok(await context.Employees.ScanAsync(limit, paginationToken));
        }

        [HttpPost]
        public async Task<IActionResult> AddEmployee(string id, string name)
        {
            var employee = new Employee
            {
                Id = id,
                Name = name,
            };
            context.Employees.Add(employee);
            await context.SaveChangesAsync();
            return this.Ok();
        }

        [HttpPut]
        public async Task<IActionResult> UpdateEmployee(string id, string name)
        {
            var employee = await context.Employees.FirstOrDefaultAsync(id);
            if(employee == null) 
            { 
                return this.NotFound();
            }
            employee.Name = name;
            await context.SaveChangesAsync();
            return this.Ok();
        }
    }
}
