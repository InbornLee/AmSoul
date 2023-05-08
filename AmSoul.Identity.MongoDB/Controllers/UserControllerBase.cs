using AmSoul.Core;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AmSoul.Identity.MongoDB;

public abstract class UserControllerBase : BaseController
{
    private readonly IMapper _mapper;
    public UserControllerBase(IUserService userService, IMapper mapper)
      : base(userService)
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }
    /// <summary>
    /// 获取用户列表
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Route("GetUsers")]
    [ProducesResponseType(typeof(IEnumerable<UserDto>), StatusCodes.Status200OK)]
    public IActionResult Get()
    {
        var users = _userService
          .GetUsers()
          .ProjectTo<UserDto>(_mapper.ConfigurationProvider)
          .ToList();
        var result = users.Select(x => { x.Roles = _userService.GetUserRolesAsync(x.Id!).Result.ToList(); return x; });
        return Ok(new BaseResponse<IEnumerable<UserDto>>() { Succeeded = true, Data = result });
    }
    /// <summary>
    /// 按ID查询用户
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}", Name = "GetUserById")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get([FromRoute] string id)
    {
        if (Token == null) return Unauthorized();

        var user = await _userService.GetUserByIdAsync(id);
        var userDto = _mapper.Map<UserDto>(user);
        if (userDto != null && userDto.Id != null)
        {
            userDto.Roles = (await _userService.GetUserRolesAsync(userDto.Id)).ToList();
        }

        return user != null
            ? Ok(new BaseResponse<UserDto>() { Succeeded = true, Data = userDto })
            : BadRequest();
    }
    /// <summary>
    /// 获取角色列表
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Route("GetRoles")]
    [ProducesResponseType(typeof(List<RoleDto>), StatusCodes.Status200OK)]
    public IActionResult GetRoles()
    {
        var roles = _userService
            .GetRoles()
            .ProjectTo<RoleDto>(_mapper.ConfigurationProvider)
            .ToList();
        return Ok(new BaseResponse<List<RoleDto>>() { Succeeded = true, Data = roles });
    }
}
