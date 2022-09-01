using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthServiceController : ControllerBase
    {
        [HttpGet]
        [Authorize]
        public string GetToken()
        {
            return DateTime.Now.ToString();
        }
    }
}
