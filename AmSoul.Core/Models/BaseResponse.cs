using AmSoul.Core.Interfaces;

namespace AmSoul.Core.Models;

public class BaseResponse<T> : IBaseResponse
{
    public BaseResponse()
    {
    }
    public BaseResponse(T data, string? message = null)
    {
        Message = message;
        Data = data;
    }
    public BaseResponse(string message)
    {
        Message = message;
    }
    public bool Succeeded { get; set; }
    public string? Message { get; set; }
    public IList<string>? Errors { get; set; }
    public T? Data;
}
/// <summary>
/// 响应信息描述
/// </summary>
public class ResponseDescription
{
    public ResponseDescription() { }
    public ResponseDescription(string code, string data)
    {
        Code = code;
        Data = data;
    }
    /// <summary>
    /// 代码
    /// </summary>
    public string? Code { get; set; }
    public string? Data { get; set; }
}
