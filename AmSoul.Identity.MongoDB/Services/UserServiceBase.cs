using AmSoul.Core.Extensions;
using AmSoul.Core.Interfaces;
using AmSoul.Identity.MongoDB.Interfaces;
using AmSoul.Identity.MongoDB.Models;
using AmSoul.Identity.MongoDB.Utilis;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AmSoul.Identity.MongoDB.Services;
public class UserServiceBase : UserServiceBase<BaseUser, BaseRole>, IUserService
{
    public UserServiceBase(
        JwtTokenOptions tokenOptions,
        UserManager<BaseUser> userManager,
        RoleManager<BaseRole> roleManager,
        ILogger<UserServiceBase> logger,
        IMapper mapper)
        : base(tokenOptions, userManager, roleManager, logger, mapper) { }
}

public abstract class UserServiceBase<TUser, TRole> : IUserService<TUser, TRole>
    where TUser : BaseUser
    where TRole : BaseRole
{
    private readonly JwtTokenOptions _tokenOptions;
    private readonly UserManager<TUser> _userManager;
    private readonly RoleManager<TRole> _roleManager;
    private readonly ILogger _logger;
    private readonly IMapper _mapper;

    public UserServiceBase(JwtTokenOptions tokenOptions, UserManager<TUser> userManager, RoleManager<TRole> roleManager, ILogger<UserServiceBase<TUser, TRole>> logger, IMapper mapper)
    {
        _tokenOptions = tokenOptions;
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }
    public virtual async Task<IBaseResponse> CreateAsync(TUser user, string password, List<string> roles)
    {
        try
        {
            var roleexit = roles.Where(x =>
            {
                if (string.IsNullOrWhiteSpace(x)) throw new ArgumentException(IdentityErrorResource.NotAllowedNull.Format(nameof(x)));
                return _roleManager.RoleExistsAsync(x).Result;
            });
            if (roleexit.Any())
            {
                var result = await _userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    result = await _userManager.AddToRolesAsync(user, roleexit);
                    _logger.Log(LogLevel.Information, $"User {user.UserName} Created Success");
                    return new IdentityResponse<UserDto>()
                    {
                        Succeeded = true,
                        Message = IdentityErrorResource.UserCreated.Format(user.UserName!),
                        Data = _mapper.Map<UserDto>(user)
                    };
                }
                throw new IdentityException(IdentityErrorResource.UserCreateFail, result);
            }
            throw new IdentityException(IdentityErrorResource.UserCreateFail, IdentityResult.Failed(new LocalizedIdentityErrorDescriber().InvalidRole()));
        }
        catch (IdentityException ex)
        {
            return new IdentityResponse<IdentityError>() { Succeeded = false, Message = $"{ex.Message}", Errors = ex.Errors!.Errors.ToList() };
        }
    }
    public virtual async Task<IBaseResponse> CreateRoleAsync(TRole role)
    {
        try
        {
            var result = await _roleManager.CreateAsync(role);
            if (result.Succeeded)
                return new IdentityResponse<RoleDto>()
                {
                    Succeeded = true,
                    Message = IdentityErrorResource.RoleCreated.Format(role.Name!),
                    Data = _mapper.Map<RoleDto>(role)
                };
            throw new IdentityException(IdentityErrorResource.RoleCreateFail, result);
        }
        catch (IdentityException ex)
        {
            return new IdentityResponse<IdentityError>() { Succeeded = false, Message = $"{ex.Message}", Errors = ex.Errors!.Errors.ToList() };
        }
    }

    public virtual IQueryable<TUser> GetUsers() => _userManager.Users;
    public virtual IQueryable<TRole> GetRoles() => _roleManager.Roles;
    public virtual async Task<bool> UserExistsAsync(string userName) => await _userManager.FindByNameAsync(userName) != null;
    public virtual async Task<bool> RoleExistsAsync(string roleName) => await _roleManager.RoleExistsAsync(roleName);
    public virtual async Task<TUser?> GetUserByIdAsync(string id) => await _userManager.FindByIdAsync(id);
    public virtual async Task<TUser?> GetUserByNameAsync(string userName) => await _userManager.FindByNameAsync(userName);
    public virtual async Task<TUser?> GetUserByEmailAsync(string email) => await _userManager.FindByEmailAsync(email);
    public virtual async Task<IList<string>> GetUserRolesAsync(string id) => await _userManager.GetRolesAsync((await GetUserByIdAsync(id))!);
    public virtual async Task<IList<TUser>> GetUsersByRoleAsync(TRole role) => await _userManager.GetUsersInRoleAsync(role.Name!);
    public virtual async Task<bool> CheckPasswordAsync(TUser user, string password) => await _userManager.CheckPasswordAsync(user, password);
    public virtual async Task<IdentityResult> ChangePassword(TUser user, string currentPwd, string newPwd) => await _userManager.ChangePasswordAsync(user, currentPwd, newPwd);
    public virtual async Task<SecurityToken> CreateAuthorizationToken(TUser user) => await BuildToken(user);
    public virtual async Task<IdentityResult> Remove(TUser user) => await _userManager.DeleteAsync(user);
    public virtual async Task<IdentityResult> Remove(string id) => await _userManager.DeleteAsync((await GetUserByIdAsync(id))!);
    public virtual async Task<IdentityResult> Update(TUser user) => await _userManager.UpdateAsync(user);
    public virtual async Task<IdentityResult> UpdateUserRoleAsync(string id, List<string> roles) => await _userManager.AddToRolesAsync((await GetUserByIdAsync(id))!, roles);
    private async Task<SecurityToken> BuildToken(TUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        JwtSecurityTokenHandler tokenHandler = new();
        List<Claim> claims = new()
            {
                new Claim(JwtRegisteredClaimNames.UniqueName, user.RealName ?? ""),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
                new Claim(JwtRegisteredClaimNames.NameId, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
        claims.AddRange(roles.Select(r => new Claim("role", r)));
        SymmetricSecurityKey signingKey = new(Encoding.UTF8.GetBytes(_tokenOptions.SecurityKey ?? string.Empty));

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = _tokenOptions.Issuer,
            Audience = _tokenOptions.Audience,
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256)
        };
        return tokenHandler.CreateToken(tokenDescriptor);
    }
}
