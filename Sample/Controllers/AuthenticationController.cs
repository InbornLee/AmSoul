using AmSoul.Identity.MongoDB.Controllers;
using AmSoul.Identity.MongoDB.Interfaces;
using AutoMapper;

namespace Sample.Controllers;

/// <summary>
/// 验证
/// </summary>
public class AuthenticationController : AuthenticationControllerBase
{
    public AuthenticationController(IUserService userService, IMapper mapper) : base(userService, mapper)
    {
    }
}
