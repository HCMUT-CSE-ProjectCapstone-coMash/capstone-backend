using System.Security.Claims;
using System.Threading.Tasks;
using Capstone.Application.Services.Authentication;
using Capstone.Application.Services.FileStorageService;
using Capstone.Contracts.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Capstone.Api.Controllers.Authentication;

[ApiController]
[Route("auth")]
public class AuthenticationController : ControllerBase
{
    private readonly IAuthenticationService _auth;
    private readonly IFileStorageService _fileStorageService;

    public AuthenticationController(IAuthenticationService auth, IFileStorageService fileStorageService)
    {
        _auth = auth;
        _fileStorageService = fileStorageService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromForm] RegisterRequest request)
    {
        var userResult = await _auth.Register(request.EmployeeId, request.FullName, request.Email,  request.PhoneNumber, request.Gender, request.DateOfBirth);

        if (request.Image != null)
        {
            var extension = Path.GetExtension(request.Image.FileName);

            var ImageResult = await _fileStorageService.UploadImageAsync(
                "users",
                userResult.Value,
                request.Image.OpenReadStream(),
                request.Image.ContentType,
                extension
            );

            // Gọi hàm update user
        }

        var user = await _auth.GetUserById(userResult.Value);

        if (user.IsFailure)
        {
            return BadRequest(new
            {
                error = user.Error.Code,
                message = user.Error.Description
            });
        }
        
        Response.Cookies.Append("accessToken", user.Value.Id.ToString(), new CookieOptions
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

    [HttpGet("create-employee-id")]
    public async Task<IActionResult> CreateEmployeeId()
    {
        var result = await _auth.CreateEmployeeId();

        if (result.IsFailure)
        {
            return BadRequest(new
            {
                error = result.Error.Code,
                message = result.Error.Description
            });
        }

        return Ok(new CreateEmployeeIdResponse(result.Value));
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

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("accessToken", new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict
        });
        
        return Ok();
    }

    [HttpGet("employees")]
    public async Task<IActionResult> GetAllEmployees()
    {
        var result = await _auth.GetAllEmployees();

        if (result.IsFailure)
        {
            return BadRequest(new
            {
                error = result.Error.Code,
                message = result.Error.Description
            });
        }

        return Ok(result.Value);
    }

    [HttpGet("get-employee-by-id/{userId}")]
    public async Task<IActionResult> GetEmployeeById([FromRoute] string userId)
    {
        var result = await _auth.GetUserById(userId);

        if (result.IsFailure)
        {
            return BadRequest(new
            {
                error = result.Error.Code,
                message = result.Error.Description
            });
        }

        return Ok(new GetEmployeeByIdResponse(
            result.Value.EmployeeId,
            result.Value.FullName,
            result.Value.Email,
            result.Value.Role,
            result.Value.PhoneNumber,
            result.Value.Gender,
            result.Value.DateOfBirth,
            result.Value.ImageURL
        ));
    }
}