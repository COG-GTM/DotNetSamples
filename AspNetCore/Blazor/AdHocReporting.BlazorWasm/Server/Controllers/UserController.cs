using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace EqDemo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetCurrentUser()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return Ok(new
                {
                    IsAuthenticated = true,
                    UserName = User.Identity.Name,
                    Roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList()
                });
            }

            return Ok(new { IsAuthenticated = false });
        }
    }
}
