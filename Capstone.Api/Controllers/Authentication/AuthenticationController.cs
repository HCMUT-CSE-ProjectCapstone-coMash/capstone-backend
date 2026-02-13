using System.Security.Claims;
using System.Threading.Tasks;
using Capstone.Application.Services.Authentication;
using Capstone.Contracts.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Capstone.Api.Controllers.Authentication;

[ApiController]
[Route("auth")]
public class AuthenticationController : ControllerBase
{
    private readonly IAuthenticationService _auth;

    public AuthenticationController(IAuthenticationService auth)
    {
        _auth = auth;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var result = await _auth.Register(request.FullName, request.Email, request.Password);

        if (result.IsFailure)
        {
            return BadRequest(new
            {
                error = result.Error.Code,
                message = result.Error.Description
            });
        }

        Response.Cookies.Append("accessToken", result.Value.Token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddMinutes(60)
        });

        return Ok(new AuthenticationResponse(
            result.Value.Id,
            result.Value.FullName,
            result.Value.Email,
            result.Value.Role,
            result.Value.CreatedAt
        ));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var result = await _auth.Login(request.Email, request.Password);

        if (result.IsFailure)
        {
            return BadRequest(new
            {
                error = result.Error.Code,
                message = result.Error.Description
            });
        }

        Response.Cookies.Append("accessToken", result.Value.Token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddMinutes(60)
        });

        return Ok(new AuthenticationResponse(
            result.Value.Id,
            result.Value.FullName,
            result.Value.Email,
            result.Value.Role,
            result.Value.CreatedAt
        ));
    }

    [Authorize]
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
            return Unauthorized();

        var result = await _auth.GetUserById(userIdClaim.Value);

        return Ok(new AuthenticationResponse(
            result.Value.Id,
            result.Value.FullName,
            result.Value.Email,
            result.Value.Role,
            result.Value.CreatedAt
        ));
    }
}