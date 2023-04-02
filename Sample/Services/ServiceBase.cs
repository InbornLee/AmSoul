using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Panda.DynamicWebApi;
using Panda.DynamicWebApi.Attributes;
using Sample.Interfaces;

namespace Sample.Services;

/// <summary>
/// 基础服务
/// </summary>
/// <typeparam name="T"></typeparam>
[DynamicWebApi]
public class ServiceBase<T> : IServiceBase<T>, IDynamicWebApi where T : class
{
    private static readonly Dictionary<int, string> Apples = new Dictionary<int, string>()
    {
        [1] = "Big Apple",
        [2] = "Small Apple"
    };

    /// <summary>
    /// 获取.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id:int}")]
    public virtual string Get(int id)
    {
        if (Apples.ContainsKey(id))
        {
            return Apples[id];
        }
        else
        {
            return "No Apple!";
        }
    }

    /// <summary>
    /// Get All Apple.
    /// </summary>
    /// <returns></returns>
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public virtual IEnumerable<string> Get()
    {
        return Apples.Values;
    }

    public virtual void Update(T dto)
    {
        //if (Apples.ContainsKey(dto.Id))
        //{
        //    Apples[dto.Id] = dto.Name;
        //}
    }

    /// <summary>
    /// Delete Apple
    /// </summary>
    /// <param name="id">Apple Id</param>
    [HttpDelete("{id:int}")]
    public virtual void Delete(int id)
    {
        if (Apples.ContainsKey(id))
        {
            Apples.Remove(id);
        }
    }
}
