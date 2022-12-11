using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LoyaltyServiceLab2.Controllers
{
    [Route("")]
    [ApiController]
    public class HealthController : ControllerBase
    {
        [HttpGet("/manage/health")]
        public ActionResult GetHealth()
        {
            return Ok();
        }
    }
}
