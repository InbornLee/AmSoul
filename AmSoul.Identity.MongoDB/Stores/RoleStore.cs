using AmSoul.MongoDB;
using Microsoft.AspNetCore.Identity;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System.ComponentModel;
using System.Data;
using System.Security.Claims;

namespace AmSoul.Identity.MongoDB;

/// <summary>
/// Creates a new instance of a persistence store for roles.
/// </summary>
/// <typeparam name="TRole">The type of the class representing a role</typeparam>
/// <typeparam name="TKey">The type of the class id</typeparam>
public class RoleStore<TRole, TKey> :
    IRoleClaimStore<TRole>,
    IQueryableRoleStore<TRole>,
    IDisposable
    where TRole : BaseRole<TKey>
    where TKey : IEquatable<TKey>
{
    /// <summary>
    /// Gets or sets the <see cref="IdentityErrorDescriber"/> for any error that occurred with the current operation.
    /// </summary>
    public IdentityErrorDescriber ErrorDescriber { get; set; }

    private readonly IMongoCollection<TRole> _collection;

    private bool _disposed;

    public RoleStore(IMongoCollection<TRole> collection, IdentityErrorDescriber? describer)
    {
        _collection = collection;

        ErrorDescriber = describer ?? new IdentityErrorDescriber();
    }

    /// <summary>
    /// A navigation property for the roles the store contains.
    /// </summary>
    public IQueryable<TRole> Roles => _collection.AsQueryable();

    /// <summary>
    /// Creates a new role in a store as an asynchronous operation.
    /// </summary>
    /// <param name="role">The role to create in the store.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>A <see cref="Task{TResult}"/> that represents the <see cref="IdentityResult"/> of the asynchronous query.</returns>
    public async Task<IdentityResult> CreateAsync(TRole role, CancellationToken cancellationToken)
    {
        NullCheck(cancellationToken, role);

        await _collection.InsertOneAsync(role, cancellationToken: cancellationToken).ConfigureAwait(false);

        return IdentityResult.Success;
    }

    /// <summary>
    /// Updates a role in a store as an asynchronous operation.
    /// </summary>
    /// <param name="role">The role to update in the store.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>A <see cref="Task{TResult}"/> that represents the <see cref="IdentityResult"/> of the asynchronous query.</returns>
    public async Task<IdentityResult> UpdateAsync(TRole role, CancellationToken cancellationToken)
    {
        NullCheck(cancellationToken, role);

        var currentConcurrencyStamp = role.ConcurrencyStamp;
        role.ConcurrencyStamp = Guid.NewGuid().ToString();

        var result = await _collection.ReplaceOneAsync(x => x.Id.Equals(role.Id) && x.ConcurrencyStamp!.Equals(currentConcurrencyStamp), role, cancellationToken: cancellationToken).ConfigureAwait(false);

        if (!result.IsAcknowledged && result.ModifiedCount == 0)
        {
            return IdentityResult.Failed(ErrorDescriber.ConcurrencyFailure());
        }

        return IdentityResult.Success;
    }

    /// <summary>
    /// Deletes a role from the store as an asynchronous operation.
    /// </summary>
    /// <param name="role">The role to delete from the store.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>A <see cref="Task{TResult}"/> that represents the <see cref="IdentityResult"/> of the asynchronous query.</returns>
    public async Task<IdentityResult> DeleteAsync(TRole role, CancellationToken cancellationToken)
    {
        NullCheck(cancellationToken, role);

        var result = await _collection.DeleteOneAsync(x => x.Id.Equals(role.Id) && x.ConcurrencyStamp!.Equals(role.ConcurrencyStamp), cancellationToken).ConfigureAwait(false);

        if (!result.IsAcknowledged && result.DeletedCount == 0)
        {
            return IdentityResult.Failed(ErrorDescriber.ConcurrencyFailure());
        }

        return IdentityResult.Success;
    }

    /// <summary>
    /// Gets the ID for a role from the store as an asynchronous operation.
    /// </summary>
    /// <param name="role">The role whose ID should be returned.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>A <see cref="Task{TResult}"/> that contains the ID of the role.</returns>
    public Task<string> GetRoleIdAsync(TRole role, CancellationToken cancellationToken)
    {
        NullCheck(cancellationToken, role);

        return Task.FromResult(ConvertIdToString(role.Id));
    }

    /// <summary>
    /// Gets the name of a role from the store as an asynchronous operation.
    /// </summary>
    /// <param name="role">The role whose name should be returned.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>A <see cref="Task{TResult}"/> that contains the name of the role.</returns>
    public Task<string?> GetRoleNameAsync(TRole role, CancellationToken cancellationToken)
    {
        NullCheck(cancellationToken, role);

        return Task.FromResult(role.Name);
    }

    /// <summary>
    /// Sets the name of a role in the store as an asynchronous operation.
    /// </summary>
    /// <param name="role">The role whose name should be set.</param>
    /// <param name="roleName">The name of the role.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
    public Task SetRoleNameAsync(TRole role, string? roleName, CancellationToken cancellationToken)
    {
        NullCheck(cancellationToken, role);

        role.Name = roleName;

        return Task.CompletedTask;
    }

    /// <summary>
    /// Get a role's normalized name as an asynchronous operation.
    /// </summary>
    /// <param name="role">The role whose normalized name should be retrieved.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>A <see cref="Task{TResult}"/> that contains the name of the role.</returns>
    public Task<string?> GetNormalizedRoleNameAsync(TRole role, CancellationToken cancellationToken)
    {
        NullCheck(cancellationToken, role);

        return Task.FromResult(role.NormalizedName);
    }

    /// <summary>
    /// Set a role's normalized name as an asynchronous operation.
    /// </summary>
    /// <param name="role">The role whose normalized name should be set.</param>
    /// <param name="normalizedName">The normalized name to set</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
    public Task SetNormalizedRoleNameAsync(TRole role, string? normalizedName, CancellationToken cancellationToken)
    {
        NullCheck(cancellationToken, role);

        role.NormalizedName = normalizedName;

        return Task.CompletedTask;
    }

    /// <summary>
    /// Finds the role who has the specified ID as an asynchronous operation.
    /// </summary>
    /// <param name="roleId">The role ID to look for.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>A <see cref="Task{TResult}"/> that result of the look up.</returns>
    public Task<TRole?> FindByIdAsync(string roleId, CancellationToken cancellationToken)
    {
        NullCheck(cancellationToken);

        return _collection.FirstOrDefaultAsync(x => x.Id.Equals(ConvertIdFromString(roleId)), cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Finds the role who has the specified normalized name as an asynchronous operation.
    /// </summary>
    /// <param name="normalizedRoleName">The normalized role name to look for.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>A <see cref="Task{TResult}"/> that result of the look up.</returns>
    public Task<TRole?> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
    {
        NullCheck(cancellationToken);

        return _collection.FirstOrDefaultAsync(x => x.NormalizedName == normalizedRoleName, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Get the claims associated with the specified <paramref name="role"/> as an asynchronous operation.
    /// </summary>
    /// <param name="role">The role whose claims should be retrieved.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>A <see cref="Task{TResult}"/> that contains the claims granted to a role.</returns>
    public async Task<IList<Claim>> GetClaimsAsync(TRole role, CancellationToken cancellationToken = default)
    {
        NullCheck(cancellationToken, role);

        var dbRole = await _collection.FirstOrDefaultAsync(x => x.Id.Equals(role.Id), cancellationToken: cancellationToken).ConfigureAwait(false);

        return dbRole?.RoleClaims.Select(claim => new Claim(claim.ClaimType!, claim.ClaimValue!)).ToList() ?? new List<Claim>();
    }

    /// <summary>
    /// Adds the <paramref name="claim"/> given to the specified <paramref name="role"/>.
    /// </summary>
    /// <param name="role">The role to add the claim to.</param>
    /// <param name="claim">The claim to add to the role.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
    public async Task AddClaimAsync(TRole role, Claim claim, CancellationToken cancellationToken = default)
    {
        NullCheck(cancellationToken, role, claim);

        var roleClaim = new BaseRoleClaim(claim.Type, claim.Value);

        role.RoleClaims.Add(roleClaim);

        await _collection.UpdateOneAsync(x => x.Id.Equals(role.Id), Builders<TRole>.Update.Set(x => x.RoleClaims, role.RoleClaims), cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Removes the <paramref name="claim"/> given from the specified <paramref name="role"/>.
    /// </summary>
    /// <param name="role">The role to remove the claim from.</param>
    /// <param name="claim">The claim to remove from the role.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
    public Task RemoveClaimAsync(TRole role, Claim claim, CancellationToken cancellationToken = default)
    {
        NullCheck(cancellationToken, role, claim);

        role.RoleClaims.RemoveAll(x => x.ClaimType == claim.Type && x.ClaimValue == claim.Value);

        return _collection.UpdateOneAsync(x => x.Id.Equals(role.Id), Builders<TRole>.Update.Set(x => x.RoleClaims, role.RoleClaims), cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Dispose the stores
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
    }

    /// <summary>
    /// Converts the provided <paramref name="id"/> to a strongly typed key object.
    /// </summary>
    /// <param name="id">The id to convert.</param>
    /// <returns>An instance of <typeparamref name="TKey"/> representing the provided <paramref name="id"/>.</returns>
    public virtual TKey ConvertIdFromString(string id)
    {
        var idKey = TypeDescriptor.GetConverter(typeof(TKey))?.ConvertFromInvariantString(id);

        ArgumentNullException.ThrowIfNull(idKey, nameof(idKey));

        return (TKey)idKey;
    }

    /// <summary>
    /// Converts the provided <paramref name="id"/> to its string representation.
    /// </summary>
    /// <param name="id">The id to convert.</param>
    /// <returns>An <see cref="string"/> representation of the provided <paramref name="id"/>.</returns>
    public virtual string ConvertIdToString(TKey id) => Equals(id, default(TKey)) ? string.Empty : id.ToString() ?? string.Empty;

    private void NullCheck(CancellationToken cancellationToken, params object[] objects)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (_disposed)
        {
            throw new ObjectDisposedException(GetType().Name);
        }

        foreach (var obj in objects)
        {
            ArgumentNullException.ThrowIfNull(obj, nameof(obj));
        }

    }
}
