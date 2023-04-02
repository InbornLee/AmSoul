using AmSoul.Core.Extensions;
using AmSoul.Core.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using System.Security.Cryptography;

namespace AmSoul.Core.Services;

public class MongoGridFSServiceBase : IMongoGridFSService
{
    private readonly IMongoDatabase _database;
    public MongoGridFSServiceBase(IDatabaseSetting settings)
    {
        _database = MongoDbQueryExtensions.GetDatabase(settings);
    }
    public virtual async Task<bool> DeleteFile(string bucketName, string id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var bucket = new GridFSBucket(_database, new GridFSBucketOptions { BucketName = bucketName });
        using Task task = bucket.DeleteAsync(new ObjectId(id), cancellationToken: cancellationToken);
        await task;
        return task.IsCompleted;
    }

    public virtual async Task<GridFSDownloadStream> DownloadFile(string bucketName, string id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var bucket = new GridFSBucket(_database, new GridFSBucketOptions { BucketName = bucketName });
        return await bucket.OpenDownloadStreamAsync(new ObjectId(id), new GridFSDownloadOptions() { CheckMD5 = true }, cancellationToken: cancellationToken);
    }

    public virtual async Task<List<GridFSFileInfo>> FindFilesAsync(string bucketName, FilterDefinition<GridFSFileInfo> filter, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var bucket = new GridFSBucket(_database, new GridFSBucketOptions { BucketName = bucketName });
        using var result = await bucket.FindAsync(filter, cancellationToken: cancellationToken);
        return await result.ToListAsync(cancellationToken: cancellationToken);
    }

    public virtual string GetMD5Hash(Stream stream)
    {
        string result = string.Empty;
        byte[] arrbytHashValue;
        //using MD5CryptoServiceProvider md5Hasher = new();
        MD5 md5Hasher = MD5.Create();
        try
        {
            arrbytHashValue = md5Hasher.ComputeHash(stream);//计算指定Stream 对象的哈希值
                                                            //由以连字符分隔的十六进制对构成的String，其中每一对表示value 中对应的元素；例如“F-2C-4A”
            string hashData = BitConverter.ToString(arrbytHashValue);
            //替换-
            hashData = hashData.Replace("-", "");
            result = hashData;
        }
        catch (Exception)
        {
        }
        return result;
    }
    public virtual async Task<string> UploadFile(string bucketName, string fileName, Stream fileStream, BsonDocument? metaData = null, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var bucket = new GridFSBucket(_database, new GridFSBucketOptions { BucketName = bucketName });
        return (await bucket.UploadFromStreamAsync(fileName, fileStream, new GridFSUploadOptions { Metadata = metaData }, cancellationToken: cancellationToken)).ToString();
    }
}
