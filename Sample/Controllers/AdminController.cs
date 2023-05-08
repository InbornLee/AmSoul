using AmSoul.Identity.MongoDB;
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
