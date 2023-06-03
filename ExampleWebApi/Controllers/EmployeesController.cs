using Amazon.DynamoDBv2.Model;
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

        [HttpDelete("id")]
        public async Task<IActionResult> DeleteEmployee(string id)
        {
            return Ok(await context.Employees.ExecuteDeleteAsync(id));
        }

        [HttpGet]
        public async Task<IActionResult> GetEmployees(int limit, string? paginationToken)
        {
            return this.Ok(await context.Employees.ScanAsync(limit, paginationToken));
        }

        [HttpPost]
        public async Task<IActionResult> AddEmployee([FromBody]Employee employee)
        {
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
