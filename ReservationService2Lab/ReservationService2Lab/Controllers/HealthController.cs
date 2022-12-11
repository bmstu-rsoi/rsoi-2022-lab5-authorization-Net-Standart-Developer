using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ReservationService2Lab.Controllers
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
