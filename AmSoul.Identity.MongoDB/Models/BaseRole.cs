using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using System.ComponentModel.DataAnnotations;

namespace AmSoul.Identity.MongoDB.Models;

public class BaseRole : BaseRole<ObjectId>
{
    public BaseRole() : base() { }
    public BaseRole(string name) : base(name) { }
}

public class BaseRole<TKey> : IdentityRole<TKey> where TKey : IEquatable<TKey>
{
    public BaseRole()
    {
        RoleClaims = new List<BaseRoleClaim>();
    }
    public BaseRole(string name) : this()
    {
        Name = name;
        NormalizedName = name.ToUpperInvariant();
    }
    public string? CharacterName { get; set; }
    public string? Description { get; set; }
    public string? Tag { get; set; }
    public List<BaseRoleClaim> RoleClaims { get; set; }
}

public class BaseRoleClaim
{
    public string? ClaimType { get; set; }
    public string? ClaimValue { get; set; }
    public BaseRoleClaim() { }
    public BaseRoleClaim(string claimType, string claimValue)
    {
        ClaimType = claimType;
        ClaimValue = claimValue;
    }
}

public sealed class RoleDto
{
    public string? Id;
    [Required]
    public string? Name { get; set; }
    [Required]
    public string? CharacterName { get; set; }
    public string? Description { get; set; }
    public string? Tag { get; set; }
}