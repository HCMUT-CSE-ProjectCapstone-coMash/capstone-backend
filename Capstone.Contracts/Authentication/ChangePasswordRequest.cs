namespace Capstone.Contracts.Authentication;

public class ChangePasswordRequest
{
    public string NewPassword { get; set; } = string.Empty;
}