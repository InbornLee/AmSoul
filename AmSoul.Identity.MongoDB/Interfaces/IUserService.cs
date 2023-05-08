using AmSoul.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace AmSoul.Identity.MongoDB;

public interface IUserService : IUserService<BaseUser, BaseRole>
{ }
public interface IUserService<TUser, TRole>
    where TUser : BaseUser
    where TRole : BaseRole
{
    Task<bool> CheckPasswordAsync(TUser user, string password);
    Task<IdentityResult> ChangePassword(TUser user, string currentPwd, string newPwd);
    Task<IBaseResponse> CreateRoleAsync(TRole role);
    Task<IBaseResponse> CreateAsync(TUser user, string password, List<string> roles);
    IQueryable<TUser> GetUsers();
    IQueryable<TRole> GetRoles();
    Task<bool> UserExistsAsync(string userName);
    Task<bool> RoleExistsAsync(string roleName);
    Task<TUser?> GetUserByIdAsync(string id);
    Task<TUser?> GetUserByNameAsync(string userName);
    Task<TUser?> GetUserByEmailAsync(string email);
    Task<IList<string>> GetUserRolesAsync(string id);
    Task<IList<TUser>> GetUsersByRoleAsync(TRole role);
    Task<SecurityToken> CreateAuthorizationToken(TUser user);
    Task<IdentityResult> Remove(TUser user);
    Task<IdentityResult> Remove(string id);
    Task<IdentityResult> Update(TUser user);
    Task<IdentityResult> UpdateUserRoleAsync(string id, List<string> roles);
}
