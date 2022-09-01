using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AuthServiceController : ControllerBase
    {
        [HttpPost]
        
        public string GetToken()
        {
            return DateTime.Now.ToString();
        }
    }
}
