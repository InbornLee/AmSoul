using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using System.ComponentModel.DataAnnotations;

namespace AmSoul.Identity.MongoDB;

public class BaseUser : BaseUser<ObjectId>
{
    public BaseUser() : base() { }
    public BaseUser(string userName) : base(userName) { }
    public BaseUser(string email, string userName) : base(email, userName) { }
}

public class BaseUser<TKey> : IdentityUser<TKey> where TKey : IEquatable<TKey>
{
    public BaseUser()
    {
        UserRoles = new List<BaseUserRole<TKey>>();
        UserClaims = new List<BaseUserClaim>();
        UserLogins = new List<BaseUserLogin>();
        UserTokens = new List<BaseUserToken>();
    }
    public BaseUser(string userName) : this()
    {
        UserName = userName;
        NormalizedUserName = userName.ToUpperInvariant();
    }

    public BaseUser(string email, string userName) : this()
    {
        UserName = userName;
        NormalizedUserName = userName.ToUpperInvariant();
        Email = email;
        NormalizedEmail = email?.ToUpperInvariant();
    }
    public string? RealName { get; set; }
    public byte[]? HeaderImage { get; set; }
    public List<BaseUserRole<TKey>> UserRoles { get; set; }
    public List<BaseUserClaim> UserClaims { get; set; }
    public List<BaseUserLogin> UserLogins { get; set; }
    public List<BaseUserToken> UserTokens { get; set; }
}

public class BaseUserClaim
{
    public string? ClaimType { get; set; }
    public string? ClaimValue { get; set; }
    public BaseUserClaim() { }
    public BaseUserClaim(string claimType, string claimValue)
    {
        ClaimType = claimType;
        ClaimValue = claimValue;
    }
}
public class BaseUserLogin
{
    public string? LoginProvider { get; set; }
    public string? ProviderKey { get; set; }
    public string? ProviderDisplayName { get; set; }
    public BaseUserLogin() { }

    public BaseUserLogin(string loginProvider, string providerKey, string providerDisplayName)
    {
        LoginProvider = loginProvider;
        ProviderKey = providerKey;
        ProviderDisplayName = providerDisplayName;
    }
}

public class BaseUserRole : BaseUserRole<ObjectId>
{
    public BaseUserRole(ObjectId id, string name) : base(id, name) { }
}

public class BaseUserRole<TKey> where TKey : IEquatable<TKey>
{
    public TKey Id { get; set; }
    public string Name { get; set; }
    public BaseUserRole(TKey id, string name)
    {
        Id = id;
        Name = name;
    }
    public override bool Equals(object? obj) => obj is BaseUserRole<TKey> userRole && userRole.Id.Equals(Id);
    public override int GetHashCode() => Id.GetHashCode();
}

public class BaseUserToken
{
    public string? LoginProvider { get; set; }
    public string? Name { get; set; }
    public string? Value { get; set; }
    public BaseUserToken() { }

    public BaseUserToken(string loginProvider, string name, string? value)
    {
        LoginProvider = loginProvider;
        Name = name;
        Value = value;
    }
}

public class UserDto
{
    public string? Id;
    [Required]
    public string? UserName { get; set; }
    [Required]
    [DataType(DataType.Password)]
    public string? Password { get; set; }
    [DataType(DataType.EmailAddress)]
    public string? Email { get; set; }
    public string? RealName { get; set; }
    public byte[]? HeaderImage { get; set; }
    public List<string>? Roles { get; set; }

}
public class ChangePasswordDto
{
    [Required]
    [DataType(DataType.Password)]
    public string? CurrentPwd { get; set; }

    [Required]
    [DataType(DataType.Password)]
    public string? NewPwd { get; set; }
}
