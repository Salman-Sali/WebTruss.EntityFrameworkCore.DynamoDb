using Amazon.DynamoDBv2.Model;
using ExampleWebApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

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

        [HttpGet]
        public async Task<IActionResult> GetById(string id)
        {
            return this.Ok(await context.Employees.FirstOrDefaultAsync(id));
        }

        [HttpGet("Check")]
        public async Task<IActionResult> Any(string id)
        {
            return this.Ok(await context.Employees.AnyAsync(id));
        }

        [HttpPut]
        public async Task<IActionResult> Add(Employee employee)
        {
            await context.Employees.ExecutePutAsync(employee);
            return this.Ok();
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(string id)
        {
            await context.Employees.ExecuteDeleteAsync(id);
            return Ok();
        }

        [HttpGet("List")]
        public async Task<IActionResult> List(string? token)
        {
            return this.Ok(await context.Employees.ScannedListAsync(1, token));
        }
    }
}
