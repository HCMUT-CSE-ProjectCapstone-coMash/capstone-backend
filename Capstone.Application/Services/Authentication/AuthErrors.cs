using Capstone.Application.Common;

namespace Capstone.Application.Services.Authentication;

public static class AuthErrors
{
    public static readonly Error UserAlreadyExists =
        new("Auth.UserAlreadyExists", "Tài khoản này đã tồn tại");

    public static readonly Error InvalidCredentials =
        new("Auth.InvalidCredentials", "Sai email hoặc mật khẩu");

    public static readonly Error UserNotExisted =
        new("Auth.UserNotExisted", "Người dùng không tồn tại");
}