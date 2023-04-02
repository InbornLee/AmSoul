using AmSoul.Core.Interfaces;
using AmSoul.Core.Models;
using AmSoul.Identity.MongoDB.Interfaces;
using AmSoul.Identity.MongoDB.Models;
using AmSoul.Identity.MongoDB.Utilis;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

namespace AmSoul.Identity.MongoDB.Controllers;

[Route("api/[controller]")]
[ApiController]
public abstract class AuthenticationControllerBase : BaseController
{
    private readonly IMapper _mapper;
    public AuthenticationControllerBase(IUserService userService, IMapper mapper)
      : base(userService)
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }
    /// <summary>
    /// 身份验证
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(typeof(IdentityResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(IdentityResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(IdentityResponse), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Authenticate([FromForm] UserDto request)
    {
        if (string.IsNullOrWhiteSpace(request.UserName))
        {
            if (string.IsNullOrWhiteSpace(request.Email))
                return UnprocessableEntity(new IdentityResponse<IdentityError>()
                {
                    Succeeded = false,
                    Message = IdentityErrorResource.RequiresUserNameOrEmail,
                    Errors = new() {
                        new LocalizedIdentityErrorDescriber().UserNameIsNull(),
                        new LocalizedIdentityErrorDescriber().EmailIsNull()
                    }
                });
            return UnprocessableEntity(new IdentityResponse<IdentityError>()
            {
                Succeeded = false,
                Message = IdentityErrorResource.UserNameIsNull,
                Errors = new() { new LocalizedIdentityErrorDescriber().UserNameIsNull() }
            });
        }
        //if (string.IsNullOrEmpty(request.Email))
        //    return UnprocessableEntity("Email are required");
        if (string.IsNullOrWhiteSpace(request.Password))
            return UnprocessableEntity(new IdentityResponse<IdentityError>()
            {
                Succeeded = false,
                Message = IdentityErrorResource.RequiresPassword,
            });

        var user = await _userService.GetUserByNameAsync(request.UserName);
        if (user == null)
        {
            return BadRequest(new IdentityResponse<IdentityError>()
            {
                Succeeded = false,
                Message = IdentityErrorResource.UserDoseNotExit,
                Errors = new() { new LocalizedIdentityErrorDescriber().UserDoseNotExit() }
            });
        }

        if (await _userService.CheckPasswordAsync(user, request.Password))
        {
            var token = await _userService.CreateAuthorizationToken(user);
            return Ok(new IdentityResponse()
            {
                Succeeded = true,
                Message = IdentityErrorResource.AuthenticationPassed,
                Token = new JwtSecurityTokenHandler().WriteToken(token)
            });
        }

        return BadRequest(new IdentityResponse<IdentityError>()
        {
            Succeeded = false,
            Message = IdentityErrorResource.PasswordMismatch,
            Errors = new() { new LocalizedIdentityErrorDescriber().PasswordMismatch() }
        });
    }
    /// <summary>
    /// 获取当前身份
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("Me")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Me()
    {
        if (Token != null)
        {
            var tokenIdClaim = Token.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.NameId);
            if (tokenIdClaim != null)
            {
                var user = await _userService.GetUserByIdAsync(tokenIdClaim.Value);
                var userDto = _mapper.Map<UserDto>(user);
                if (userDto != null && userDto.Id != null)
                {
                    userDto.Roles = (await _userService.GetUserRolesAsync(userDto.Id)).ToList();
                }
                return userDto != null
                    ? Ok(new BaseResponse<UserDto>() { Succeeded = true, Data = userDto })
                    : BadRequest();
            }
        }
        return Unauthorized();
    }
    /// <summary>
    /// 修改密码
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("ChangePassword")]
    public async Task<ActionResult<IBaseResponse>> ChangePassword([FromForm] ChangePasswordDto model)
    {
        try
        {
            if (Token != null)
            {
                var user = await base.GetUser();
                if (user == null) return BadRequest(new IdentityResponse() { Succeeded = false, Message = IdentityErrorResource.UserDoseNotExit });
                var result = await _userService.ChangePassword(user, model.CurrentPwd!.Trim(), model.NewPwd!.Trim());
                if (result.Succeeded)
                {
                    return Ok(new IdentityResponse() { Succeeded = true, Message = IdentityErrorResource.PasswordChanged });
                }
                throw new IdentityException(IdentityErrorResource.PasswordIsNotChanged, result);
            }
            return Unauthorized();
        }
        catch (IdentityException ex)
        {
            return new IdentityResponse<IdentityError>() { Succeeded = false, Message = $"{ex.Message}", Errors = ex.Errors!.Errors.ToList() };
        }
    }
}
