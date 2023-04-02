using AmSoul.Core.Extensions;
using AmSoul.Identity.MongoDB.Models;
using Microsoft.AspNetCore.Identity;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq.Expressions;
using System.Security.Claims;

namespace AmSoul.Identity.MongoDB.Stores;

/// <summary>
/// Represents a new instance of a persistence store for the specified user and role types.
/// </summary>
/// <typeparam name="TUser">The type representing a user.</typeparam>
/// <typeparam name="TRole">The type representing a role.</typeparam>
/// <typeparam name="TKey">The type of the primary key for a user/role.</typeparam>
public class UserStore<TUser, TRole, TKey> :
    IUserClaimStore<TUser>,
    IUserLoginStore<TUser>,
    IUserRoleStore<TUser>,
    IUserPasswordStore<TUser>,
    IUserSecurityStampStore<TUser>,
    IUserEmailStore<TUser>,
    IUserPhoneNumberStore<TUser>,
    IQueryableUserStore<TUser>,
    IUserTwoFactorStore<TUser>,
    IUserLockoutStore<TUser>,
    IUserAuthenticatorKeyStore<TUser>,
    IUserAuthenticationTokenStore<TUser>,
    IUserTwoFactorRecoveryCodeStore<TUser>,
    IProtectedUserStore<TUser>,
    IDisposable
    where TKey : IEquatable<TKey>
    where TUser : BaseUser<TKey>
    where TRole : BaseRole<TKey>
{
    /// <summary>
    /// Gets or sets the <see cref="IdentityErrorDescriber"/> for any error that occurred with the current operation.
    /// </summary>
    public IdentityErrorDescriber ErrorDescriber { get; set; }

    /// <summary>
    /// A navigation property for the users the store contains.
    /// </summary>
    public IQueryable<TUser> Users => _userCollection.AsQueryable();

    private readonly IMongoCollection<TUser> _userCollection;
    private readonly IMongoCollection<TRole> _roleCollection;

    private readonly InsertOneOptions _insertOneOptions = new();
    private readonly FindOptions<TUser> _findOptions = new();
    private readonly ReplaceOptions _replaceOptions = new();

    private const string _internalLoginProvider = "[AspNetUserStore]";
    private const string _authenticatorKeyTokenName = "AuthenticatorKey";
    private const string _recoveryCodeTokenName = "RecoveryCodes";

    private bool _disposed;

    /// <summary>
    /// Creates a new instance of the store.
    /// </summary>
    /// <param name="userCollection">The user mongo collection.</param>
    /// <param name="roleCollection">The role mongo collection.</param>
    /// <param name="describer">The <see cref="IdentityErrorDescriber"/> used to describe store errors.</param>
    public UserStore(IMongoCollection<TUser> userCollection, IMongoCollection<TRole> roleCollection, IdentityErrorDescriber? describer)
    {
        _userCollection = userCollection;
        _roleCollection = roleCollection;

        ErrorDescriber = describer ?? new IdentityErrorDescriber();

        EnsureIndex(index => index.NormalizedEmail!);
        EnsureIndex(index => index.NormalizedUserName!);
    }

    /// <summary>
    /// Sets the token value for a particular user.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="loginProvider">The authentication provider for the token.</param>
    /// <param name="name">The name of the token.</param>
    /// <param name="value">The value of the token.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
    public async Task SetTokenAsync(TUser user, string loginProvider, string name, string? value, CancellationToken cancellationToken)
    {
        NullCheck(cancellationToken, user);

        var token = await FindTokenAsync(user, loginProvider, name, cancellationToken);

        if (token is null)
        {
            var userToken = new BaseUserToken(loginProvider, name, value);

            user.UserTokens.Add(userToken);
        }
        else
        {
            token.Value = value;

            var idx = user.UserTokens.FindIndex(x => x.LoginProvider == token.LoginProvider && x.Name == token.Name);

            user.UserTokens[idx].Value = token.Value;
        }
    }

    /// <summary>
    /// Deletes a token for a user.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="loginProvider">The authentication provider for the token.</param>
    /// <param name="name">The name of the token.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
    public async Task RemoveTokenAsync(TUser user, string loginProvider, string name, CancellationToken cancellationToken)
    {
        NullCheck(cancellationToken, user);

        var entry = await FindTokenAsync(user, loginProvider, name, cancellationToken);
        if (entry != null)
        {
            user.UserTokens.RemoveAll(x => x.LoginProvider == entry.LoginProvider && x.Name == entry.Name);
        }
    }

    /// <summary>
    /// Returns the token value.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="loginProvider">The authentication provider for the token.</param>
    /// <param name="name">The name of the token.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
    public async Task<string?> GetTokenAsync(TUser user, string loginProvider, string name, CancellationToken cancellationToken)
    {
        NullCheck(cancellationToken, user);

        var token = await FindTokenAsync(user, loginProvider, name, cancellationToken);
        return token?.Value ?? string.Empty;
    }

    /// <summary>
    /// Get the authenticator key for the specified <paramref name="user" />.
    /// </summary>
    /// <param name="user">The user whose security stamp should be set.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the security stamp for the specified <paramref name="user"/>.</returns>
    public Task<string?> GetAuthenticatorKeyAsync(TUser user, CancellationToken cancellationToken) => GetTokenAsync(user, _internalLoginProvider, _authenticatorKeyTokenName, cancellationToken);

    /// <summary>
    /// Sets the authenticator key for the specified <paramref name="user"/>.
    /// </summary>
    /// <param name="user">The user whose authenticator key should be set.</param>
    /// <param name="key">The authenticator key to set.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
    public Task SetAuthenticatorKeyAsync(TUser user, string key, CancellationToken cancellationToken) => SetTokenAsync(user, _internalLoginProvider, _authenticatorKeyTokenName, key, cancellationToken);

    /// <summary>
    /// Creates the specified <paramref name="user"/> in the user store.
    /// </summary>
    /// <param name="user">The user to create.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="IdentityResult"/> of the creation operation.</returns>
    public async Task<IdentityResult> CreateAsync(TUser user, CancellationToken cancellationToken)
    {
        NullCheck(cancellationToken, user);

        await _userCollection.InsertOneAsync(user, _insertOneOptions, cancellationToken).ConfigureAwait(false);

        return IdentityResult.Success;
    }

    /// <summary>
    /// Deletes the specified <paramref name="user"/> from the user store.
    /// </summary>
    /// <param name="user">The user to delete.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="IdentityResult"/> of the update operation.</returns>
    public async Task<IdentityResult> DeleteAsync(TUser user, CancellationToken cancellationToken)
    {
        NullCheck(cancellationToken, user);

        var result = await _userCollection.DeleteOneAsync(x => x.Id.Equals(user.Id) && x.ConcurrencyStamp!.Equals(user.ConcurrencyStamp), cancellationToken).ConfigureAwait(false);
        if (!result.IsAcknowledged && result.DeletedCount == 0)
        {
            return IdentityResult.Failed(ErrorDescriber.ConcurrencyFailure());
        }

        return IdentityResult.Success;
    }

    /// <summary>
    /// Finds and returns a user, if any, who has the specified <paramref name="userId"/>.
    /// </summary>
    /// <param name="userId">The user ID to search for.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>
    /// The <see cref="Task"/> that represents the asynchronous operation, containing the user matching the specified <paramref name="userId"/> if it exists.
    /// </returns>
    public Task<TUser?> FindByIdAsync(string userId, CancellationToken cancellationToken)
    {
        NullCheck(cancellationToken);

        return FindByIdAsync(ConvertIdFromString(userId), cancellationToken);
    }

    /// <summary>
    /// Finds and returns a user, if any, who has the specified normalized user name.
    /// </summary>
    /// <param name="normalizedUserName">The normalized user name to search for.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>
    /// The <see cref="Task"/> that represents the asynchronous operation, containing the user matching the specified <paramref name="normalizedUserName"/> if it exists.
    /// </returns>
    public Task<TUser?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
    {
        NullCheck(cancellationToken);

        return _userCollection.FirstOrDefaultAsync(x => x.NormalizedUserName == normalizedUserName, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Updates the specified <paramref name="user"/> in the user store.
    /// </summary>
    /// <param name="user">The user to update.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="IdentityResult"/> of the update operation.</returns>
    public async Task<IdentityResult> UpdateAsync(TUser user, CancellationToken cancellationToken)
    {
        NullCheck(cancellationToken, user);

        var currentConcurrencyStamp = user.ConcurrencyStamp;
        user.ConcurrencyStamp = Guid.NewGuid().ToString();

        var result = await _userCollection.ReplaceOneAsync(x => x.Id.Equals(user.Id) && x.ConcurrencyStamp!.Equals(currentConcurrencyStamp), user, _replaceOptions, cancellationToken).ConfigureAwait(false);

        return !result.IsAcknowledged && result.ModifiedCount == 0
            ? IdentityResult.Failed(ErrorDescriber.ConcurrencyFailure())
            : IdentityResult.Success;
    }

    /// <summary>
    /// Adds the <paramref name="claims"/> given to the specified <paramref name="user"/>.
    /// </summary>
    /// <param name="user">The user to add the claim to.</param>
    /// <param name="claims">The claim to add to the user.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
    public Task AddClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
    {
        NullCheck(cancellationToken, user, claims);

        foreach (var claim in claims)
        {
            var identityClaim = new BaseUserClaim(claim.Type, claim.Value);

            user.UserClaims.Add(identityClaim);
        }

        return Task.FromResult(false);
    }

    /// <summary>
    /// Replaces the <paramref name="claim"/> on the specified <paramref name="user"/>, with the <paramref name="newClaim"/>.
    /// </summary>
    /// <param name="user">The user to replace the claim on.</param>
    /// <param name="claim">The claim replace.</param>
    /// <param name="newClaim">The new claim replacing the <paramref name="claim"/>.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
    public Task ReplaceClaimAsync(TUser user, Claim claim, Claim newClaim, CancellationToken cancellationToken)
    {
        NullCheck(cancellationToken, user, claim, newClaim);

        var matchedClaims = user.UserClaims.Where(uc => uc.ClaimValue == claim.Value && uc.ClaimType == claim.Type).ToList();
        foreach (var matchedClaim in matchedClaims)
        {
            matchedClaim.ClaimValue = newClaim.Value;
            matchedClaim.ClaimType = newClaim.Type;
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Removes the <paramref name="claims"/> given from the specified <paramref name="user"/>.
    /// </summary>
    /// <param name="user">The user to remove the claims from.</param>
    /// <param name="claims">The claim to remove.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
    public Task RemoveClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
    {
        NullCheck(cancellationToken, user, claims);

        foreach (var claim in claims)
        {
            user.UserClaims.RemoveAll(x => x.ClaimType == claim.Type && x.ClaimValue == claim.Value);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Retrieves all users with the specified claim.
    /// </summary>
    /// <param name="claim">The claim whose users should be retrieved.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>
    /// The <see cref="Task"/> contains a list of users, if any, that contain the specified claim.
    /// </returns>
    public async Task<IList<TUser>> GetUsersForClaimAsync(Claim claim, CancellationToken cancellationToken)
    {
        NullCheck(cancellationToken, claim);

        return (await _userCollection.WhereAsync(u => u.UserClaims.Any(c => c.ClaimType == claim.Type && c.ClaimValue == claim.Value), cancellationToken).ConfigureAwait(false)).ToList();
    }

    /// <summary>
    /// Gets the normalized user name for the specified <paramref name="user"/>.
    /// </summary>
    /// <param name="user">The user whose normalized name should be retrieved.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the normalized user name for the specified <paramref name="user"/>.</returns>
    public Task<string?> GetNormalizedUserNameAsync(TUser user, CancellationToken cancellationToken)
    {
        NullCheck(cancellationToken, user);

        return Task.FromResult(user.NormalizedUserName);
    }

    /// <summary>
    /// Gets the user identifier for the specified <paramref name="user"/>.
    /// </summary>
    /// <param name="user">The user whose identifier should be retrieved.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the identifier for the specified <paramref name="user"/>.</returns>
    public Task<string> GetUserIdAsync(TUser user, CancellationToken cancellationToken)
    {
        NullCheck(cancellationToken, user);

        return Task.FromResult(ConvertIdToString(user.Id));
    }

    /// <summary>
    /// Gets the user name for the specified <paramref name="user"/>.
    /// </summary>
    /// <param name="user">The user whose name should be retrieved.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the name for the specified <paramref name="user"/>.</returns>
    public Task<string?> GetUserNameAsync(TUser user, CancellationToken cancellationToken)
    {
        NullCheck(cancellationToken, user);

        return Task.FromResult(user.UserName);
    }

    /// <summary>
    /// Get the claims associated with the specified <paramref name="user"/> as an asynchronous operation.
    /// </summary>
    /// <param name="user">The user whose claims should be retrieved.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>A <see cref="Task{TResult}"/> that contains the claims granted to a user.</returns>
    public async Task<IList<Claim>> GetClaimsAsync(TUser user, CancellationToken cancellationToken)
    {
        NullCheck(cancellationToken, user);

        var dbUser = await FindByIdAsync(user.Id, cancellationToken).ConfigureAwait(true);
        return dbUser?.UserClaims?.Select(claim => new Claim(claim.ClaimType!, claim.ClaimValue!))?.ToList() ?? new List<Claim>();
    }

    /// <summary>
    /// Sets the given normalized name for the specified <paramref name="user"/>.
    /// </summary>
    /// <param name="user">The user whose name should be set.</param>
    /// <param name="normalizedName">The normalized name to set.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
    public Task SetNormalizedUserNameAsync(TUser user, string? normalizedName, CancellationToken cancellationToken)
    {
        NullCheck(cancellationToken, user);

        user.NormalizedUserName = normalizedName;

        return Task.CompletedTask;
    }

    /// <summary>
    /// Sets the given <paramref name="userName" /> for the specified <paramref name="user"/>.
    /// </summary>
    /// <param name="user">The user whose name should be set.</param>
    /// <param name="userName">The user name to set.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
    public Task SetUserNameAsync(TUser user, string? userName, CancellationToken cancellationToken)
    {
        NullCheck(cancellationToken, user);

        user.UserName = userName;

        return Task.CompletedTask;
    }

    /// <summary>
    /// Gets the email address for the specified <paramref name="user"/>.
    /// </summary>
    /// <param name="user">The user whose email should be returned.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The task object containing the results of the asynchronous operation, the email address for the specified <paramref name="user"/>.</returns>
    public Task<string?> GetEmailAsync(TUser user, CancellationToken cancellationToken)
    {
        NullCheck(cancellationToken, user);

        return Task.FromResult(user.Email);
    }

    /// <summary>
    /// Gets a flag indicating whether the email address for the specified <paramref name="user"/> has been verified, true if the email address is verified otherwise
    /// false.
    /// </summary>
    /// <param name="user">The user whose email confirmation status should be returned.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>
    /// The task object containing the results of the asynchronous operation, a flag indicating whether the email address for the specified <paramref name="user"/>
    /// has been confirmed or not.
    /// </returns>
    public Task<bool> GetEmailConfirmedAsync(TUser user, CancellationToken cancellationToken)
    {
        NullCheck(cancellationToken, user);

        return Task.FromResult(user.EmailConfirmed);
    }

    /// <summary>
    /// Gets the user, if any, associated with the specified, normalized email address.
    /// </summary>
    /// <param name="normalizedEmail">The normalized email address to return the user for.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>
    /// The task object containing the results of the asynchronous lookup operation, the user if any associated with the specified normalized email address.
    /// </returns>
    public Task<TUser?> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
    {
        NullCheck(cancellationToken);

        return _userCollection.FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail, cancellationToken);
    }

    /// <summary>
    /// Returns the normalized email for the specified <paramref name="user"/>.
    /// </summary>
    /// <param name="user">The user whose email address to retrieve.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>
    /// The task object containing the results of the asynchronous lookup operation, the normalized email address if any associated with the specified user.
    /// </returns>
    public Task<string?> GetNormalizedEmailAsync(TUser user, CancellationToken cancellationToken)
    {
        NullCheck(cancellationToken, user);

        return Task.FromResult(user.NormalizedEmail);
    }

    /// <summary>
    /// Sets the flag indicating whether the specified <paramref name="user"/>'s email address has been confirmed or not.
    /// </summary>
    /// <param name="user">The user whose email confirmation status should be set.</param>
    /// <param name="confirmed">A flag indicating if the email address has been confirmed, true if the address is confirmed otherwise false.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public Task SetEmailConfirmedAsync(TUser user, bool confirmed, CancellationToken cancellationToken)
    {
        NullCheck(cancellationToken, user);

        user.EmailConfirmed = confirmed;

        return Task.CompletedTask;
    }

    /// <summary>
    /// Sets the normalized email for the specified <paramref name="user"/>.
    /// </summary>
    /// <param name="user">The user whose email address to set.</param>
    /// <param name="normalizedEmail">The normalized email to set for the specified <paramref name="user"/>.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public Task SetNormalizedEmailAsync(TUser user, string? normalizedEmail, CancellationToken cancellationToken)
    {
        NullCheck(cancellationToken, user);

        user.NormalizedEmail = normalizedEmail;

        return Task.CompletedTask;
    }

    /// <summary>
    /// Sets the <paramref name="email"/> address for a <paramref name="user"/>.
    /// </summary>
    /// <param name="user">The user whose email should be set.</param>
    /// <param name="email">The email to set.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public Task SetEmailAsync(TUser user, string? email, CancellationToken cancellationToken)
    {
        NullCheck(cancellationToken, user);

        user.Email = email;

        return Task.CompletedTask;
    }

    /// <summary>
    /// Retrieves the current failed access count for the specified <paramref name="user"/>..
    /// </summary>
    /// <param name="user">The user whose failed access count should be retrieved.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the failed access count.</returns>
    public Task<int> GetAccessFailedCountAsync(TUser user, CancellationToken cancellationToken)
    {
        NullCheck(cancellationToken, user);

        return Task.FromResult(user.AccessFailedCount);
    }

    /// <summary>
    /// Retrieves a flag indicating whether user lockout can enabled for the specified user.
    /// </summary>
    /// <param name="user">The user whose ability to be locked out should be returned.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>
    /// The <see cref="Task"/> that represents the asynchronous operation, true if a user can be locked out, otherwise false.
    /// </returns>
    public Task<bool> GetLockoutEnabledAsync(TUser user, CancellationToken cancellationToken)
    {
        NullCheck(cancellationToken, user);

        return Task.FromResult(user.LockoutEnabled);
    }

    /// <summary>
    /// Records that a failed access has occurred, incrementing the failed access count.
    /// </summary>
    /// <param name="user">The user whose cancellation count should be incremented.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the incremented failed access count.</returns>
    public Task<int> IncrementAccessFailedCountAsync(TUser user, CancellationToken cancellationToken)
    {
        NullCheck(cancellationToken, user);

        user.AccessFailedCount++;
        return Task.FromResult(user.AccessFailedCount);
    }

    /// <summary>
    /// Resets a user's failed access count.
    /// </summary>
    /// <param name="user">The user whose failed access count should be reset.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <remarks>This is typically called after the account is successfully accessed.</remarks>
    public Task ResetAccessFailedCountAsync(TUser user, CancellationToken cancellationToken)
    {
        NullCheck(cancellationToken, user);

        user.AccessFailedCount = 0;
        return Task.CompletedTask;
    }

    /// <summary>
    /// Gets the last <see cref="DateTimeOffset"/> a user's last lockout expired, if any.
    /// Any time in the past should be indicates a user is not locked out.
    /// </summary>
    /// <param name="user">The user whose lockout date should be retrieved.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> that represents the result of the asynchronous query, a <see cref="DateTimeOffset"/> containing the last time
    /// a user's lockout expired, if any.
    /// </returns>
    public Task<DateTimeOffset?> GetLockoutEndDateAsync(TUser user, CancellationToken cancellationToken)
    {
        NullCheck(cancellationToken, user);

        return Task.FromResult(user.LockoutEnd);
    }

    /// <summary>
    /// Locks out a user until the specified end date has passed. Setting a end date in the past immediately unlocks a user.
    /// </summary>
    /// <param name="user">The user whose lockout date should be set.</param>
    /// <param name="lockoutEnd">The <see cref="DateTimeOffset"/> after which the <paramref name="user"/>'s lockout should end.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
    public Task SetLockoutEndDateAsync(TUser user, DateTimeOffset? lockoutEnd, CancellationToken cancellationToken)
    {
        NullCheck(cancellationToken, user);

        user.LockoutEnd = lockoutEnd;
        return Task.CompletedTask;
    }

    /// <summary>
    /// Set the flag indicating if the specified <paramref name="user"/> can be locked out..
    /// </summary>
    /// <param name="user">The user whose ability to be locked out should be set.</param>
    /// <param name="enabled">A flag indicating if lock out can be enabled for the specified <paramref name="user"/>.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
    public Task SetLockoutEnabledAsync(TUser user, bool enabled, CancellationToken cancellationToken)
    {
        NullCheck(cancellationToken, user);

        user.LockoutEnabled = enabled;
        return Task.CompletedTask;
    }

    /// <summary>
    /// Adds the <paramref name="login"/> given to the specified <paramref name="user"/>.
    /// </summary>
    /// <param name="user">The user to add the login to.</param>
    /// <param name="login">The login to add to the user.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
    public Task AddLoginAsync(TUser user, UserLoginInfo login, CancellationToken cancellationToken)
    {
        NullCheck(cancellationToken, user);

        if (login is null) throw new ArgumentNullException(nameof(login));

        var userLogin = new BaseUserLogin(login.LoginProvider, login.ProviderKey, login.ProviderDisplayName!);

        user.UserLogins.Add(userLogin);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Removes the <paramref name="loginProvider"/> given from the specified <paramref name="user"/>.
    /// </summary>
    /// <param name="user">The user to remove the login from.</param>
    /// <param name="loginProvider">The login to remove from the user.</param>
    /// <param name="providerKey">The key provided by the <paramref name="loginProvider"/> to identify a user.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
    public Task RemoveLoginAsync(TUser user, string loginProvider, string providerKey, CancellationToken cancellationToken)
    {
        NullCheck(cancellationToken, user);

        user.UserLogins.RemoveAll(x => x.LoginProvider == loginProvider && x.ProviderKey == providerKey);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Retrieves the user associated with the specified login provider and login provider key.
    /// </summary>
    /// <param name="loginProvider">The login provider who provided the <paramref name="providerKey"/>.</param>
    /// <param name="providerKey">The key provided by the <paramref name="loginProvider"/> to identify a user.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>
    /// The <see cref="Task"/> for the asynchronous operation, containing the user, if any which matched the specified login provider and key.
    /// </returns>
    public async Task<TUser?> FindByLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken)
    {
        NullCheck(cancellationToken);

        return await _userCollection.FirstOrDefaultAsync(u =>
            u.UserLogins.Any(l => l.LoginProvider == loginProvider && l.ProviderKey == providerKey), cancellationToken).ConfigureAwait(true);
    }

    /// <summary>
    /// Retrieves the associated logins for the specified <param ref="user"/>.
    /// </summary>
    /// <param name="user">The user whose associated logins to retrieve.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>
    /// The <see cref="Task"/> for the asynchronous operation, containing a list of <see cref="UserLoginInfo"/> for the specified <paramref name="user"/>, if any.
    /// </returns>
    public async Task<IList<UserLoginInfo>> GetLoginsAsync(TUser user, CancellationToken cancellationToken)
    {
        NullCheck(cancellationToken, user);

        var dbUser = await FindByIdAsync(user.Id, cancellationToken).ConfigureAwait(true);

        return dbUser?.UserLogins?.Select(x => new UserLoginInfo(x.LoginProvider!, x.ProviderKey!, x.ProviderDisplayName))?.ToList() ?? new List<UserLoginInfo>();
    }

    /// <summary>
    /// Gets the password hash for a user.
    /// </summary>
    /// <param name="user">The user to retrieve the password hash for.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>A <see cref="Task{TResult}"/> that contains the password hash for the user.</returns>
    public Task<string?> GetPasswordHashAsync(TUser user, CancellationToken cancellationToken)
    {
        NullCheck(cancellationToken, user);

        return Task.FromResult(user.PasswordHash);
    }

    /// <summary>
    /// Returns a flag indicating if the specified user has a password.
    /// </summary>
    /// <param name="user">The user to retrieve the password hash for.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>A <see cref="Task{TResult}"/> containing a flag indicating if the specified user has a password. If the 
    /// user has a password the returned value with be true, otherwise it will be false.</returns>
    public Task<bool> HasPasswordAsync(TUser user, CancellationToken cancellationToken)
    {
        NullCheck(cancellationToken, user);

        return Task.FromResult(user.PasswordHash != null);
    }

    /// <summary>
    /// Sets the password hash for a user.
    /// </summary>
    /// <param name="user">The user to set the password hash for.</param>
    /// <param name="passwordHash">The password hash to set.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
    public Task SetPasswordHashAsync(TUser user, string? passwordHash, CancellationToken cancellationToken)
    {
        NullCheck(cancellationToken, user);

        user.PasswordHash = passwordHash;

        return Task.CompletedTask;
    }

    /// <summary>
    /// Gets the telephone number, if any, for the specified <paramref name="user"/>.
    /// </summary>
    /// <param name="user">The user whose telephone number should be retrieved.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the user's telephone number, if any.</returns>
    public Task<string?> GetPhoneNumberAsync(TUser user, CancellationToken cancellationToken)
    {
        NullCheck(cancellationToken, user);

        return Task.FromResult(user.PhoneNumber);
    }

    /// <summary>
    /// Gets a flag indicating whether the specified <paramref name="user"/>'s telephone number has been confirmed.
    /// </summary>
    /// <param name="user">The user to return a flag for, indicating whether their telephone number is confirmed.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>
    /// The <see cref="Task"/> that represents the asynchronous operation, returning true if the specified <paramref name="user"/> has a confirmed
    /// telephone number otherwise false.
    /// </returns>
    public Task<bool> GetPhoneNumberConfirmedAsync(TUser user, CancellationToken cancellationToken)
    {
        NullCheck(cancellationToken, user);

        return Task.FromResult(user.PhoneNumberConfirmed);
    }

    /// <summary>
    /// Sets the telephone number for the specified <paramref name="user"/>.
    /// </summary>
    /// <param name="user">The user whose telephone number should be set.</param>
    /// <param name="phoneNumber">The telephone number to set.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
    public Task SetPhoneNumberAsync(TUser user, string? phoneNumber, CancellationToken cancellationToken)
    {
        NullCheck(cancellationToken, user);

        user.PhoneNumber = phoneNumber;
        return Task.CompletedTask;
    }

    /// <summary>
    /// Sets a flag indicating if the specified <paramref name="user"/>'s phone number has been confirmed..
    /// </summary>
    /// <param name="user">The user whose telephone number confirmation status should be set.</param>
    /// <param name="confirmed">A flag indicating whether the user's telephone number has been confirmed.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
    public Task SetPhoneNumberConfirmedAsync(TUser user, bool confirmed, CancellationToken cancellationToken)
    {
        NullCheck(cancellationToken, user);

        user.PhoneNumberConfirmed = confirmed;

        return Task.CompletedTask;
    }

    /// <summary>
    /// Adds the given <paramref name="roleName"/> to the specified <paramref name="user"/>.
    /// </summary>
    /// <param name="user">The user to add the role to.</param>
    /// <param name="roleName">The role to add.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
    public Task AddToRoleAsync(TUser user, string roleName, CancellationToken cancellationToken)
    {
        NullCheck(cancellationToken, user);

        return string.IsNullOrWhiteSpace(roleName)
            ? throw new ArgumentNullException(nameof(roleName), "Value cannot be null or empty.")
            : AddToRoleInternalAsync(user, roleName, cancellationToken);
    }

    private async Task AddToRoleInternalAsync(TUser user, string roleName, CancellationToken cancellationToken)
    {
        var roleEntity = await FindRoleAsync(roleName, cancellationToken) ?? throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Role {0} does not exist.", roleName));
        var userRole = new BaseUserRole<TKey>(roleEntity.Id, roleEntity.Name!);

        user.UserRoles.Add(userRole);
    }

    /// <summary>
    /// Removes the given <paramref name="roleName"/> from the specified <paramref name="user"/>.
    /// </summary>
    /// <param name="user">The user to remove the role from.</param>
    /// <param name="roleName">The role to remove.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
    public Task RemoveFromRoleAsync(TUser user, string roleName, CancellationToken cancellationToken)
    {
        NullCheck(cancellationToken, user);

        return string.IsNullOrWhiteSpace(roleName)
            ? throw new ArgumentNullException(nameof(roleName), "Value cannot be null or empty.")
            : RemoveFromRoleInternalAsync(user, roleName, cancellationToken);
    }

    private async Task RemoveFromRoleInternalAsync(TUser user, string roleName, CancellationToken cancellationToken)
    {
        var roleEntity = await FindRoleAsync(roleName, cancellationToken);

        if (roleEntity != null)
        {
            var userRole = new BaseUserRole<TKey>(roleEntity.Id, roleEntity.Name!);
            user.UserRoles.Remove(userRole);
        }
    }

    /// <summary>
    /// Retrieves all users in the specified role.
    /// </summary>
    /// <param name="roleName">The role whose users should be retrieved.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>
    /// The <see cref="Task"/> contains a list of users, if any, that are in the specified role.
    /// </returns>
    public Task<IList<TUser>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken)
    {
        NullCheck(cancellationToken);

        return string.IsNullOrWhiteSpace(roleName)
             ? throw new ArgumentNullException(nameof(roleName), "Value cannot be null or empty.")
             : GetUsersInRoleInternalAsync(roleName, cancellationToken);
    }

    private async Task<IList<TUser>> GetUsersInRoleInternalAsync(string roleName, CancellationToken cancellationToken)
    {
        var role = await FindRoleAsync(roleName, cancellationToken);

        if (role is null)
        {
            return new List<TUser>();
        }

        var userRole = new BaseUserRole<TKey>(role.Id, role.Name!);
        var filter = Builders<TUser>.Filter.AnyEq(x => x.UserRoles, userRole);

        return (await _userCollection.FindAsync(filter, _findOptions, cancellationToken).ConfigureAwait(true)).ToList(cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Retrieves the roles the specified <paramref name="user"/> is a member of.
    /// </summary>
    /// <param name="user">The user whose roles should be retrieved.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>A <see cref="Task{TResult}"/> that contains the roles the user is a member of.</returns>
    public async Task<IList<string>> GetRolesAsync(TUser user, CancellationToken cancellationToken)
    {
        NullCheck(cancellationToken, user);

        var userDb = await FindByIdAsync(user.Id, cancellationToken).ConfigureAwait(true);
        if (userDb is null)
        {
            return new List<string>();
        }

        var roles = new List<string>();
        foreach (var item in userDb.UserRoles)
        {
            var dbRole = await _roleCollection.FirstOrDefaultAsync(r => r.Id.Equals(item.Id), cancellationToken).ConfigureAwait(true);

            if (dbRole != null)
            {
                roles.Add(dbRole.Name!);
            }
        }
        return roles;
    }

    /// <summary>
    /// Returns a flag indicating if the specified user is a member of the give <paramref name="roleName"/>.
    /// </summary>
    /// <param name="user">The user whose role membership should be checked.</param>
    /// <param name="roleName">The role to check membership of</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>A <see cref="Task{TResult}"/> containing a flag indicating if the specified user is a member of the given group. If the
    /// user is a member of the group the returned value with be true, otherwise it will be false.</returns>
    public async Task<bool> IsInRoleAsync(TUser user, string roleName, CancellationToken cancellationToken)
    {
        NullCheck(cancellationToken, user);

        var dbUser = await FindByIdAsync(user.Id, cancellationToken).ConfigureAwait(true);
        var role = await FindRoleAsync(roleName, cancellationToken).ConfigureAwait(true);

        if (role is null)
        {
            return false;
        }

        var userRole = new BaseUserRole<TKey>(role.Id, role.Name!);

        return dbUser?.UserRoles.Contains(userRole) ?? false;
    }

    /// <summary>
    /// Get the security stamp for the specified <paramref name="user" />.
    /// </summary>
    /// <param name="user">The user whose security stamp should be set.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the security stamp for the specified <paramref name="user"/>.</returns>
    public Task<string?> GetSecurityStampAsync(TUser user, CancellationToken cancellationToken)
    {
        NullCheck(cancellationToken, user);

        return Task.FromResult(user.SecurityStamp);
    }

    /// <summary>
    /// Sets the provided security <paramref name="stamp"/> for the specified <paramref name="user"/>.
    /// </summary>
    /// <param name="user">The user whose security stamp should be set.</param>
    /// <param name="stamp">The security stamp to set.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
    public Task SetSecurityStampAsync(TUser user, string stamp, CancellationToken cancellationToken)
    {
        NullCheck(cancellationToken, user, stamp);

        user.SecurityStamp = stamp;

        return Task.CompletedTask;
    }

    /// <summary>
    /// Updates the recovery codes for the user while invalidating any previous recovery codes.
    /// </summary>
    /// <param name="user">The user to store new recovery codes for.</param>
    /// <param name="recoveryCodes">The new recovery codes for the user.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The new recovery codes for the user.</returns>
    public Task ReplaceCodesAsync(TUser user, IEnumerable<string> recoveryCodes, CancellationToken cancellationToken)
    {
        NullCheck(cancellationToken, user, recoveryCodes);

        var mergedCodes = string.Join(";", recoveryCodes);

        return SetTokenAsync(user, _internalLoginProvider, _recoveryCodeTokenName, mergedCodes, cancellationToken);
    }

    /// <summary>
    /// Returns whether a recovery code is valid for a user. Note: recovery codes are only valid
    /// once, and will be invalid after use.
    /// </summary>
    /// <param name="user">The user who owns the recovery code.</param>
    /// <param name="code">The recovery code to use.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>True if the recovery code was found for the user.</returns>
    public async Task<bool> RedeemCodeAsync(TUser user, string code, CancellationToken cancellationToken)
    {
        NullCheck(cancellationToken, user, code);

        var mergedCodes = await GetTokenAsync(user, _internalLoginProvider, _recoveryCodeTokenName, cancellationToken) ?? "";
        var splitCodes = mergedCodes.Split(';');

        if (splitCodes.Contains(code))
        {
            var updatedCodes = new List<string>(splitCodes.Where(s => s != code));
            await ReplaceCodesAsync(user, updatedCodes, cancellationToken);

            return true;
        }

        return false;
    }

    /// <summary>
    /// Returns how many recovery code are still valid for a user.
    /// </summary>
    /// <param name="user">The user who owns the recovery code.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The number of valid recovery codes for the user..</returns>
    public async Task<int> CountCodesAsync(TUser user, CancellationToken cancellationToken)
    {
        NullCheck(cancellationToken, user);

        var mergedCodes = await GetTokenAsync(user, _internalLoginProvider, _recoveryCodeTokenName, cancellationToken) ?? "";
        if (mergedCodes.Length > 0)
        {
            return mergedCodes.Split(';').Length;
        }
        return 0;
    }

    /// <summary>
    /// Returns a flag indicating whether the specified <paramref name="user"/> has two factor authentication enabled or not,
    /// as an asynchronous operation.
    /// </summary>
    /// <param name="user">The user whose two factor authentication enabled status should be set.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>
    /// The <see cref="Task"/> that represents the asynchronous operation, containing a flag indicating whether the specified 
    /// <paramref name="user"/> has two factor authentication enabled or not.
    /// </returns>
    public Task<bool> GetTwoFactorEnabledAsync(TUser user, CancellationToken cancellationToken)
    {
        NullCheck(cancellationToken, user);

        return Task.FromResult(user.TwoFactorEnabled);
    }

    /// <summary>
    /// Sets a flag indicating whether the specified <paramref name="user"/> has two factor authentication enabled or not,
    /// as an asynchronous operation.
    /// </summary>
    /// <param name="user">The user whose two factor authentication enabled status should be set.</param>
    /// <param name="enabled">A flag indicating whether the specified <paramref name="user"/> has two factor authentication enabled.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
    public Task SetTwoFactorEnabledAsync(TUser user, bool enabled, CancellationToken cancellationToken)
    {
        NullCheck(cancellationToken, user);

        user.TwoFactorEnabled = enabled;

        return Task.CompletedTask;
    }

    /// <summary>
    /// Dispose the store
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


    #region private

    private Task<TRole?> FindRoleAsync(string normalizedRoleName, CancellationToken cancellationToken = default) => _roleCollection.FirstOrDefaultAsync(x => x.NormalizedName == normalizedRoleName.ToUpperInvariant(), cancellationToken);

    private async Task<BaseUserToken?> FindTokenAsync(TUser user, string loginProvider, string name, CancellationToken cancellationToken = default)
    {
        var dbUser = await FindByIdAsync(user.Id, cancellationToken).ConfigureAwait(true);

        return dbUser!.UserTokens.FirstOrDefault(token => token.LoginProvider == loginProvider && token.Name == name);
    }

    private void EnsureIndex(Expression<Func<TUser, object>> field)
    {
        var model = new CreateIndexModel<TUser>(Builders<TUser>.IndexKeys.Ascending(field));
        _userCollection.Indexes.CreateOne(model);
    }

    private Task<TUser?> FindByIdAsync(TKey id, CancellationToken cancellationToken = default) => _userCollection.FirstOrDefaultAsync(x => x.Id.Equals(id), cancellationToken);

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

    #endregion
}
