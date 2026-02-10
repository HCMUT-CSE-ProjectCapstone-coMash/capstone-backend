namespace Capstone.Contracts.Authentication;

public record AuthenticationResponse(
    Guid Id,
    string FullName,
    string Email,
    string Role,
    DateTime CreatedAt,
    string Token
);