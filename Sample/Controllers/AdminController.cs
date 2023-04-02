using AmSoul.Identity.MongoDB.Controllers;
using AmSoul.Identity.MongoDB.Interfaces;
using AutoMapper;

namespace Sample.Controllers;

/// <summary>
/// 管理员
/// </summary>
public class AdminController : AdminControllerBase
{
    public AdminController(IUserService userService, IMapper mapper) : base(userService, mapper)
    {
    }
}
