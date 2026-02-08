namespace Capstone.Application.Common;

public record Error
{
    public string Code { get; }
    public string Description { get; }

    public static readonly Error None = new("", "");

    public Error(string code, string description)
    {
        Code = code;
        Description = description;
    }
}