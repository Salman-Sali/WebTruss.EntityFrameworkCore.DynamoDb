using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ExampleWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SingleTablesController : ControllerBase
    {
        private readonly ApplicationDbContext context;

        public SingleTablesController(ApplicationDbContext context)
        {
            this.context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Create(string pk, string sk)
        {
            await context.SingleTable.ExecutePutAsync(new Models.SingleTable
            {
                Pk = pk,
                Sk = sk
            });
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return Ok(await context.SingleTable.PagedListAsync("1", 10, null));
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(string sk)
        {
            var item = await context.SingleTable.FirstOrDefaultAsync("1", sk);
            await context.SingleTable.ExecuteDeleteAsync(item);
            return Ok();
        }
    }
}
