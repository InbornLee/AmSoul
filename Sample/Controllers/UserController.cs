using AmSoul.Identity.MongoDB.Controllers;
using AmSoul.Identity.MongoDB.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Sample.Controllers;

/// <summary>
/// ”√ªß
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class UserController : UserControllerBase
{
    public UserController(IUserService userService, IMapper mapper)
      : base(userService, mapper)
    { }

}