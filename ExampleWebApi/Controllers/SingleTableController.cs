using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ExampleWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SingleTableController : ControllerBase
    {
        private readonly ApplicationDbContext context;

        public SingleTableController(ApplicationDbContext context)
        {
            this.context = context;
        }

        [HttpPost]
        public async Task<IActionResult> AddData(SingleTable singleTable)
        {
            await context.SingleTable.ExecutePutAsync(singleTable);
            return this.Ok();
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(string pk, string sk)
        {
            await context.SingleTable.ExecuteDeleteAsync(pk, sk);
            return this.Ok();
        }

        [HttpGet]
        public async Task<IActionResult> ScanList(string token = null)
        {
            return this.Ok(await context.SingleTable.ScanAsync(10, token));
        }

        [HttpGet("GetPage")]
        public async Task<IActionResult> TakePage(string? token)
        {
            return this.Ok(await context.SingleTable.PagedListAsync("Product", 5, token));
        }
    }
}
