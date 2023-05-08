using Newtonsoft.Json.Linq;

namespace AmSoul.Core;

public interface IPagination
{
    int Limit { get; set; }
    int Offset { get; set; }
    JObject? OrderBy { get; set; }
    ICollection<QueryParam>? QueryParams { get; set; }
}
public interface IPageData<T>
{
    int? Total { get; set; }
    ICollection<T>? Data { get; set; }
}
