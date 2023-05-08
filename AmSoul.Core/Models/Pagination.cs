using Newtonsoft.Json.Linq;
using System.ComponentModel.DataAnnotations;

namespace AmSoul.Core;

/// <summary>
/// 分页参数
/// </summary>
public sealed class Pagination : IPagination
{
    /// <summary>
    /// 记录条数
    /// </summary>
    [Range(1, 500, ErrorMessage = "Limit must >0 and <=500")]
    public int Limit { get; set; } = 20;
    /// <summary>
    /// 偏移值
    /// </summary>
    [Range(0, int.MaxValue, ErrorMessage = "Offset must >= 0")]
    public int Offset { get; set; } = 0;
    /// <summary>
    /// 排序字段：asc desc
    /// </summary>
    public JObject? OrderBy { get; set; } = new();
    public ICollection<QueryParam>? QueryParams { get; set; }
    /// <summary>
    /// 库名称
    /// </summary>
    public string? Bucket;
}
/// <summary>
/// 查询参数
/// </summary>
public class QueryParam
{
    /// <summary>
    /// 字段名
    /// </summary>
    public string? Field { get; set; }
    /// <summary>
    /// 条件
    /// </summary>
    public Dictionary<string, object>? Conditions { get; set; }
    /// <summary>
    /// 逻辑：AND OR IN EQ
    /// </summary>
    public string Logical { get; set; } = "AND";
}
/// <summary>
/// 分页数据
/// </summary>
/// <typeparam name="T"></typeparam>
public sealed class PageData<T> : IBaseResponse
{
    public bool Succeeded { get; set; }
    public string? Message { get; set; }
    public IList<string>? Errors { get; set; }
    //[BsonElement("total")]
    public int? Total { get; set; }
    //[BsonElement("data")]
    public ICollection<T>? Data { get; set; }
}
