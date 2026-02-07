using Capstone.Contracts.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace Capstone.Api.Controllers.Authentication;

[ApiController]
[Route("auth")]
public class AuthenticationController : ControllerBase
{
    [HttpPost("register")]
    public IActionResult Register(RegisterRequest request)
    {
        return Ok(request);
    }

    [HttpPost("login")]
    public IActionResult Login(LoginRequest request)
    {
        return Ok(request);
    }
}