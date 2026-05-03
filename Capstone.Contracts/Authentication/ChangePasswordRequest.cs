namespace Capstone.Contracts.Authentication;

public class ChangePasswordRequest
{
    public string NewPassword { get; set; } = string.Empty;
}

public class ChangePasswordMobileRequest
{
    public string UserId { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}