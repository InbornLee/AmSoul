using AmSoul.Core;
using Newtonsoft.Json.Linq;
using System.Text;

namespace AmSoul.SQL;
public static partial class PaginationExtensions
{
    public static string ParseSqlOrderByString(this Pagination param)
    {
        if (param == null || param.OrderBy == null) return string.Empty;

        StringBuilder query = new();
        IEnumerable<JProperty> properties = param.OrderBy.Properties();
        foreach (var p in properties)
        {
            var key = p.Name;
            var value = p.Value.ToString();
            if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
            {
                query.Append($"{key} {value},");
            }
        }
        return query.ToString()[..(query.Length - 1)];

        //            using IEnumerator<KeyValuePair<string, string>> enumerator = param.OrderBy..GetEnumerator();
        //            StringBuilder query = new();
        //            while (enumerator.MoveNext())
        //            {
        //                var key = enumerator.Current.Key;
        //                var value = enumerator.Current.Value;
        //                if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
        //                {
        //                    query.Append($"{key} {value},");
        //                }
        //            }
        //            return query.ToString()[..(query.Length - 1)];
    }
    public static string ParseSqlQueryString(this Pagination param)
    {
        if (param == null || param.QueryParams == null) return string.Empty;

        IEnumerator<QueryParam> enumerator = param.QueryParams.GetEnumerator();

        StringBuilder query = new();
        int count = 0;
        while (enumerator.MoveNext())
        {
            var field = enumerator.Current.Field;
            var value = enumerator.Current.Conditions;
            var logical = enumerator.Current.Logical;

            if (!string.IsNullOrEmpty(field) && value != null)
            {
                _ = count == 0 ? query.AppendJoin(' ', "WHERE", string.Empty) : query.AppendJoin(' ', string.Empty, logical, string.Empty);
                query.Append('(');
                var i = 0;
                foreach (var item in value)
                {
                    if (i > 0) query.AppendJoin(' ', string.Empty, logical, string.Empty);
                    query.AppendJoin(' ', field, item.Key, item.Key.ToUpper() == "LIKE" ? $"'%{item.Value}%'" : $"'{item.Value}'");
                    i++;
                }
                query.Append(')');
            }
            //query.AppendJoin(' ', field, value.First().Key, value.First().Key.ToUpper() == "LIKE" ? $"'%{value.First().Value}%'" : $"'{value.First().Value}'");
            count++;
        }
        return query.ToString();
    }

}