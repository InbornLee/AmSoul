using System.Dynamic;

namespace AmSoul.Core.Extensions;

/// <summary>
/// Dynamic动态类扩展
/// </summary>
public static class DynamicExtensions
{
    public static dynamic GetDynamicRootObject(ExpandoObject obj)
    {
        Dictionary<string, object> Data = new();

        if (obj is not IDictionary<string, object> dic) throw new ArgumentNullException();
        if (dic.Count == 0) throw new Exception("No information to process.");

        foreach (var property in dic)
        {
            string key = property.Key.Trim().ToCamelCase();

            if (string.IsNullOrWhiteSpace(key))
                throw new Exception($"The key corresponding to the value \"{property.Value}\" is not valid.");
            if (key == "_id") key = "id";
            var value = property.Value ?? string.Empty;
            Data[key] = value;

            //else if (key == "type")
            //    Data["type"] = property.Value.ToString(); // The values of the id and type members MUST be strings.

        }
        return Data;
    }
}
