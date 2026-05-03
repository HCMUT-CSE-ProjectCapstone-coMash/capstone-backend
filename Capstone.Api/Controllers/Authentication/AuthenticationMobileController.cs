using Capstone.Application.Services.Authentication;
using Capstone.Contracts.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace Capstone.Api.Controllers.Authentication;

[ApiController]
[Route("auth/mobile")]
public class AuthenticationMobileController : ControllerBase
{
    private readonly IAuthenticationService _auth;

    public AuthenticationMobileController(IAuthenticationService auth)
    {
        _auth = auth;
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

        return Ok(new AuthenticationMobileResponse(
            result.Value.Token,
            result.Value.Id,
            result.Value.EmployeeId,
            result.Value.FullName,
            result.Value.Email,
            result.Value.Role,
            result.Value.PhoneNumber,
            result.Value.Gender,
            result.Value.DateOfBirth,
            result.Value.ImageURL,
            result.Value.CreatedAt,
            result.Value.HasChangedPassword
        ));
    }

    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordMobileRequest request)
    {
        var result = await _auth.ChangePassword(request.UserId, request.NewPassword);

        if (result.IsFailure)
        {
            return BadRequest(new
            {
                error = result.Error.Code,
                message = result.Error.Description
            });
        }

        return Ok(new { message = "Password changed successfully" });
    }
}