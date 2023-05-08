using AmSoul.Core;
using MongoDB.Bson;
using Newtonsoft.Json.Linq;

namespace AmSoul.MongoDB
{
    public static partial class PaginationExtensions
    {
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
            {"metadata", new BsonArray{new BsonDocument("$count", "Total")}},
            {"Data", new BsonArray{new BsonDocument("$skip", param.Offset), new BsonDocument("$limit", param.Limit) }}}
            );
            stageArray.Add(facet);
            var project = new BsonDocument("$project", new BsonDocument {
            { "Total", new BsonDocument("$cond", new BsonDocument { { "if", new BsonDocument("$arrayElemAt", new BsonArray { "$metadata.Total", 0 }) }, { "then", new BsonDocument("$arrayElemAt", new BsonArray { "$metadata.Total", 0 }) }, { "else", 0 } }) },
            { "Data", 1 } }
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
}
