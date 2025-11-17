using Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Services.Contracts;
using System.Diagnostics;

namespace SendlertService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MyController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly IJob _job;
        private static readonly ActivitySource _activitySource = new("MyApp");

        public MyController(IProductService productService, IJob job)
        {
            _productService = productService;
            _job = job;
        }

        [HttpPost("AddProduct")]
        public async Task<IActionResult> SendMessage([FromBody] Product product)
        {
            await _productService.Add(product);
            return NoContent();
        }

        [HttpGet]
        [Route("RunOutboxMessages")]
        public async Task<IActionResult> RunJob()
        {
            using (var activity = _activitySource.StartActivity("Span_start"))
            {
                activity?.SetTag("user.id", 123);

                await Step1();
                await Step2();

                //await _job.Execute();
                return Ok();
            }
        }
        [HttpGet]
        private async Task Step1()
        {
            using (var span = _activitySource.StartActivity("Span_2"))
            {
                await Task.Delay(3000);
            }
        }
        [HttpGet]
        private async Task Step2()
        {
            using (var span = _activitySource.StartActivity("Span_3"))
            {
                await Task.Delay(5000);
            }
        }
    }
}
