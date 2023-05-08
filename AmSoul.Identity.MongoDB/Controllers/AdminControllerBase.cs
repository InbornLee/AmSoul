using AmSoul.Core;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AmSoul.Identity.MongoDB;

[Route("api/[controller]")]
[ApiController]
//[Authorize(Roles = "admin")]
public abstract class AdminControllerBase : BaseController
{
    private readonly IMapper _mapper;
    public AdminControllerBase(IUserService userService, IMapper mapper) : base(userService)
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }
    /// <summary>
    /// 初始化系统
    /// </summary>
    /// <returns></returns>
    [HttpGet("Init")]
    public async Task<IActionResult> Init()
    {
        try
        {
            if (!await _userService.RoleExistsAsync("admin"))
            {
                var adminRole = new BaseRole()
                {
                    Name = "admin",
                    CharacterName = "系统管理员",
                    Description = "拥有所有权限"
                };
                await _userService.CreateRoleAsync(adminRole);
            }

            if (await _userService.UserExistsAsync("admin"))
                throw new ArgumentException(IdentityErrorResource.DuplicateUserName.Format("admin"));
            var userDto = new UserDto
            {
                UserName = "admin",
                Password = "123456",
                RealName = "管理员",
                //Email = "admin@GHPMS3.org"
            };
            var response = await _userService.CreateAsync(
                _mapper.Map<BaseUser>(userDto),
                userDto.Password,
                new List<string>() { "admin" });
            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new BaseResponse<string>()
            {
                Succeeded = false,
                Message = IdentityErrorResource.InitializeFail,
                Errors = new List<string>() { ex.Message }
            });
        }
    }
    /// <summary>
    /// 新增角色
    /// </summary>
    /// <param name="roleDto"></param>
    /// <returns></returns>
    [HttpPost("AddRole")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "admin")]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IBaseResponse>> AddRole([FromQuery] RoleDto roleDto)
    {
        try
        {
            var role = _mapper.Map<BaseRole>(roleDto);
            var result = await _userService.CreateRoleAsync(role);
            return result.Succeeded
             ? Ok(result)
             : BadRequest(result);
        }
        catch (IdentityException ex)
        {
            return new IdentityResponse<IdentityError>() { Succeeded = false, Message = $"{ex.Message}", Errors = ex.Errors!.Errors.ToList() };
        }
    }
    /// <summary>
    /// 新增用户
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    [HttpPost("CreateUser")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "admin")]
    public async Task<ActionResult<IBaseResponse>> CreateUser([FromQuery] UserDto user)
    {
        try
        {
            var result = await _userService.CreateAsync(_mapper.Map<BaseUser>(user), user.Password!, user.Roles!);
            return result.Succeeded
                ? (ActionResult<IBaseResponse>)Ok(result)
                : (ActionResult<IBaseResponse>)BadRequest(result);
        }
        catch (IdentityException ex)
        {
            return new IdentityResponse<IdentityError>() { Succeeded = false, Message = $"{ex.Message}", Errors = ex.Errors!.Errors.ToList() };
        }
    }
}
