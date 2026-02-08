namespace Capstone.Application.Services.Authentication;

public record AuthResult(
    Guid Id,
    string FullName,
    string Email,
    string Token
);