using AmSoul.Core.Models;
using MongoDB.Bson;
using Newtonsoft.Json.Linq;
using System.Text;

namespace AmSoul.Core.Extensions;

public static class PaginationExtensions
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
    public static BsonArray ParseMongoAggregateStage(this Pagination param)
    {
        BsonArray stageArray = new();
        var matchCondition = new BsonDocument();
        if (param.QueryParams != null)
        {
            foreach (var query in param.QueryParams)
            {
                if (query.Conditions != null)
                    matchCondition.Add(query.Field.ToPascalCase(), query.Conditions.ParseMongoQueryRule());
            }
        }
        var match = new BsonDocument("$match", matchCondition);
        stageArray.Add(match);

        var sortCondition = new BsonDocument("Id", 1);
        if (param.OrderBy != null && param.OrderBy.Count > 0) sortCondition = new BsonDocument(param.OrderBy.ParseMongoSortRule());
        var sort = new BsonDocument("$sort", sortCondition);
        stageArray.Add(sort);

        var facet = new BsonDocument("$facet", new BsonDocument{
            {"metadata", new BsonArray{new BsonDocument("$count", "total")}},
            {"data", new BsonArray{new BsonDocument("$skip", param.Offset), new BsonDocument("$limit", param.Limit) }}}
        );
        stageArray.Add(facet);
        var project = new BsonDocument("$project", new BsonDocument {
            { "total", new BsonDocument("$cond", new BsonDocument { { "if", new BsonDocument("$arrayElemAt", new BsonArray { "$metadata.total", 0 }) }, { "then", new BsonDocument("$arrayElemAt", new BsonArray { "$metadata.total", 0 }) }, { "else", 0 } }) },
            { "data", 1 } }
        );
        stageArray.Add(project);
        return stageArray;
    }
    private static BsonDocument ParseMongoQueryRule(this Dictionary<string, object> dic)
    {
        var newBsonDocument = new BsonDocument();
        foreach (var item in dic)
        {
            if (item.Key is not null && item.Value is not null)
            {
                string mongoRule = item.Key.ToUpper() switch
                {
                    "=" => "$eq",
                    "!=" => "$ne",
                    "LIKE" => "$regex",
                    "<" => "$lt",
                    "<=" => "$lte",
                    ">" => "$gt",
                    ">=" => "$gte",
                    _ => item.Key,
                };
                _ = DateTime.TryParse(item.Value.ToString(), out DateTime dtDate)
                    ? newBsonDocument.Add(mongoRule, BsonValue.Create(dtDate.ToLocalTime()))
                    : newBsonDocument.Add(mongoRule, BsonValue.Create(item.Value));
            }
        }
        return newBsonDocument;
    }
    private static Dictionary<string, object> ParseMongoSortRule(this JObject dic)
    {
        Dictionary<string, object> newdic = new();
        IEnumerable<JProperty> properties = dic.Properties();
        foreach (var p in properties)
        {
            switch (p.Value.ToString().ToLower())
            {
                case "asc": newdic.Add(p.Name.ToPascalCase(), 1); break;
                case "desc": newdic.Add(p.Name.ToPascalCase(), -1); break;
                default: newdic.Add(p.Name.ToPascalCase(), 1); break;
            }
        }
        return newdic;
    }

}
