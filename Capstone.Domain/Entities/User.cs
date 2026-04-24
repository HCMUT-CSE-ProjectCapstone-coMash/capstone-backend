namespace Capstone.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string Role { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
    public DateOnly DateOfBirth { get; set; }
    public string Status { get; set; } = string.Empty;
    public int FailedLoginAttempts { get; set; }
    public string HasChangedPassword { get; set; } = string.Empty;
    public string? ImageKey { get; set; } = string.Empty;
    public string? EmployeeId { get; set; } = string.Empty;
}