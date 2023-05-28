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

        [HttpGet]
        public async Task<IActionResult> GetEmployeeById()
        {
            var data = await context.Employees.FirstOrDefaultAsync("Emp-1");
            data.Name = "Hello";
            context.SaveChangesAsync();

            return this.Ok(data);
        }
    }
}
