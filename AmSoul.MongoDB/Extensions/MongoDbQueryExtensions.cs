using AmSoul.Core;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace AmSoul.MongoDB;

public static class MongoDbQueryExtensions
{
    public static IMongoDatabase GetDatabase(IDatabaseSetting settings) => new MongoClient(settings.ConnectionString).GetDatabase(settings.DatabaseName ?? "default");
    public static IMongoCollection<T> GetCollection<T>(IDatabaseSetting settings, string collectionName)
    {
        IMongoCollection<T> collection;
        return collection = GetDatabase(settings).GetCollection<T>(collectionName ?? nameof(T).ToCamelCase());
    }
    private static FindOptions<TItem> LimitOneOption<TItem>() => new() { Limit = 1 };

    public static async Task<TItem?> FirstOrDefaultAsync<TItem>(
        this IMongoCollection<TItem> collection,
        Expression<Func<TItem, bool>> expression,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(collection, nameof(collection));

        var result = await collection
            .FindAsync(expression, LimitOneOption<TItem>(), cancellationToken)
            .ConfigureAwait(false);

        return await result
            .FirstOrDefaultAsync(cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }

    public static async Task<IEnumerable<TItem>> WhereAsync<TItem>(
        this IMongoCollection<TItem> collection,
        Expression<Func<TItem, bool>> expression,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(collection, nameof(collection));

        var result = await collection
            .FindAsync(expression, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        return result.ToEnumerable(cancellationToken: cancellationToken);
    }
}