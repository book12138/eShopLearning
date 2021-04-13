using Microsoft.AspNetCore.Mvc;

namespace eShopLearning.CartProductAggregator.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HealthController : ControllerBase
    {
        [HttpGet("Check")]
        public IActionResult Check() => Ok();
    }
}
