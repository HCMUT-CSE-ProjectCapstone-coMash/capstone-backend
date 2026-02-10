using Capstone.Application.Common;

namespace Capstone.Application.Services.Authentication;

public static class AuthErrors
{
    public static readonly Error UserAlreadyExists =
        new("Auth.UserAlreadyExists", "User already exists");

    public static readonly Error InvalidCredentials =
        new("Auth.InvalidCredentials", "Invalid email or password");

    public static readonly Error UserNotExisted =
        new("Auth.UserNotExisted", "User not existed");
}