using AmSoul.Core.Models;
using Microsoft.AspNetCore.Identity;

namespace AmSoul.Identity.MongoDB.Models;

public class IdentityResponse : BaseResponse<string>
{
    public string? Token { get; set; }
}
public class IdentityResponse<T> : BaseResponse<T> where T : class
{
    public string? Token { get; set; }
    public new List<T>? Errors { get; set; }
}
public class IdentityException : Exception
{
    public IdentityException() { }
    public IdentityException(string? message, IdentityResult result) : base(message)
    {
        Errors = result;

    }
    public IdentityException(IdentityResult result)
    {
        Errors = result;
    }
    public IdentityResult? Errors { get; set; }
}
