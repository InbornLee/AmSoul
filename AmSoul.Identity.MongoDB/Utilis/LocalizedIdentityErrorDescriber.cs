using AmSoul.Core.Extensions;
using Microsoft.AspNetCore.Identity;

namespace AmSoul.Identity.MongoDB.Utilis;

public class LocalizedIdentityErrorDescriber : IdentityErrorDescriber
{
    public override IdentityError DefaultError() => new() { Code = nameof(DefaultError), Description = IdentityErrorResource.DefaultError };
    public override IdentityError ConcurrencyFailure() => new() { Code = nameof(ConcurrencyFailure), Description = IdentityErrorResource.ConcurrencyFailure };
    public override IdentityError PasswordMismatch() => new() { Code = nameof(PasswordMismatch), Description = IdentityErrorResource.PasswordMismatch };
    public override IdentityError InvalidToken() => new() { Code = nameof(InvalidToken), Description = IdentityErrorResource.InvalidToken };
    public override IdentityError LoginAlreadyAssociated() => new() { Code = nameof(LoginAlreadyAssociated), Description = IdentityErrorResource.LoginAlreadyAssociated };
    public override IdentityError InvalidUserName(string? userName) => new() { Code = nameof(InvalidUserName), Description = IdentityErrorResource.InvalidUserName.Format(userName!) };
    public override IdentityError InvalidEmail(string? email) => new() { Code = nameof(InvalidEmail), Description = IdentityErrorResource.InvalidEmail.Format(email!) };
    public override IdentityError DuplicateUserName(string? userName) => new() { Code = nameof(DuplicateUserName), Description = IdentityErrorResource.DuplicateUserName.Format(userName!) };
    public override IdentityError DuplicateEmail(string? email) => new() { Code = nameof(DuplicateEmail), Description = IdentityErrorResource.DuplicateEmail.Format(email!) };
    public override IdentityError InvalidRoleName(string? role) => new() { Code = nameof(InvalidRoleName), Description = IdentityErrorResource.InvalidRoleName.Format(role!) };
    public override IdentityError DuplicateRoleName(string? role) => new() { Code = nameof(DuplicateRoleName), Description = IdentityErrorResource.DuplicateRoleName.Format(role!) };
    public override IdentityError UserAlreadyHasPassword() => new() { Code = nameof(UserAlreadyHasPassword), Description = IdentityErrorResource.UserAlreadyHasPassword };
    public override IdentityError UserLockoutNotEnabled() => new() { Code = nameof(UserLockoutNotEnabled), Description = IdentityErrorResource.UserLockoutNotEnabled };
    public override IdentityError UserAlreadyInRole(string? role) => new() { Code = nameof(UserAlreadyInRole), Description = IdentityErrorResource.UserAlreadyInRole.Format(role!) };
    public override IdentityError UserNotInRole(string? role) => new() { Code = nameof(UserNotInRole), Description = IdentityErrorResource.UserNotInRole.Format(role!) };
    public override IdentityError PasswordTooShort(int length) => new() { Code = nameof(PasswordTooShort), Description = IdentityErrorResource.PasswordTooShort.Format(length) };
    public override IdentityError PasswordRequiresNonAlphanumeric() => new() { Code = nameof(PasswordRequiresNonAlphanumeric), Description = IdentityErrorResource.PasswordRequiresNonAlphanumeric };
    public override IdentityError PasswordRequiresDigit() => new() { Code = nameof(PasswordRequiresDigit), Description = IdentityErrorResource.PasswordRequiresDigit };
    public override IdentityError PasswordRequiresLower() => new() { Code = nameof(PasswordRequiresLower), Description = IdentityErrorResource.PasswordRequiresLower };
    public override IdentityError PasswordRequiresUpper() => new() { Code = nameof(PasswordRequiresUpper), Description = IdentityErrorResource.PasswordRequiresUpper };
    public IdentityError UserDoseNotExit() => new() { Code = nameof(UserDoseNotExit), Description = IdentityErrorResource.UserDoseNotExit };
    public IdentityError UserNameIsNull() => new() { Code = nameof(UserNameIsNull), Description = IdentityErrorResource.UserNameIsNull };
    public IdentityError EmailIsNull() => new() { Code = nameof(EmailIsNull), Description = IdentityErrorResource.EmailIsNull };
    public IdentityError InvalidRole() => new() { Code = nameof(InvalidRole), Description = IdentityErrorResource.InvalidRole };
}
