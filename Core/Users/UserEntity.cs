using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Text.Json.Serialization;

using NuScien.Data;
using NuScien.Security;
using Trivial.Security;
using Trivial.SocialNetwork;
using Trivial.Text;
using Trivial.Users;

namespace NuScien.Users;

/// <summary>
/// The user information.
/// </summary>
[DataContract]
[Table("nsusers")]
public class UserEntity : BaseSecurityEntity
{
    /// <summary>
    /// Gets the security entity type.
    /// </summary>
    [NotMapped]
    [JsonIgnore]
    public override SecurityEntityTypes SecurityEntityType => SecurityEntityTypes.User;

    /// <summary>
    /// Gets or sets the password.
    /// </summary>
    [JsonIgnore]
    [Column("password")]
    public string PasswordEncrypted
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets or sets the gender.
    /// </summary>
    [DataMember(Name = "gender")]
    [JsonPropertyName("gender")]
    [JsonConverter(typeof(JsonIntegerEnumCompatibleConverter<Genders>))]
    [NotMapped]
    public Genders Gender
    {
        get => GetCurrentProperty<Genders>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets or sets the gender code.
    /// </summary>
    [JsonIgnore]
    [Column("gender")]
    [Required]
    public int GenderCode
    {
        get => (int)Gender;
        set => Gender = (Genders)value;
    }

    /// <summary>
    /// Gets or sets the birthday.
    /// </summary>
    [DataMember(Name = "birthday", EmitDefaultValue = true)]
    [JsonPropertyName("birthday")]
    [JsonConverter(typeof(JsonJavaScriptTicksConverter.FallbackNullableConverter))]
    [Column("birthday")]
    public DateTime? Birthday
    {
        get => GetCurrentProperty<DateTime?>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets or sets the code of the preferred market.
    /// </summary>
    [DataMember(Name = "market", EmitDefaultValue = true)]
    [JsonPropertyName("market")]
    [Column("market")]
    public string Market
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Sets a new password.
    /// </summary>
    /// <param name="password">The new password.</param>
    /// <param name="old">The optional old password to validate; or null, to ignore validation.</param>
    /// <param name="confirm">The optional new password to confirm; or null, to ignore confirmation.</param>
    public bool SetPassword(string password, string old = null, string confirm = null)
    {
        if (!string.IsNullOrEmpty(old) && !ValidatePassword(old)) return false;
        if (password == null) return false;
        password = password.Trim();
        if (password.Length < 6) return false;
        if (!string.IsNullOrEmpty(confirm) && !password.Equals(confirm, StringComparison.Ordinal)) return false;
        PasswordEncrypted = HashPassword(password);
        return true;
    }

    /// <summary>
    /// Sets a new password.
    /// </summary>
    /// <param name="password">The new password.</param>
    /// <param name="old">The optional old password to validate; or null, to ignore validation.</param>
    public bool SetPassword(SecureString password, string old = null)
    {
        return SetPassword(password?.ToUnsecureString(), old);
    }

    /// <summary>
    /// Tests if the given password is valid.
    /// </summary>
    /// <param name="password">The original password.</param>
    /// <returns>true if the password is correct; otherwise, false.</returns>
    public bool ValidatePassword(string password)
    {
        if (password == null) return false;
        password = password.Trim();
        return HashPassword(password).Equals(PasswordEncrypted?.Trim(), StringComparison.Ordinal);
    }

    /// <summary>
    /// Tests if the given password is valid.
    /// </summary>
    /// <param name="password">The original password.</param>
    /// <returns>true if the password is correct; otherwise, false.</returns>
    public bool ValidatePassword(SecureString password)
    {
        return ValidatePassword(password?.ToUnsecureString());
    }

    /// <summary>
    /// Returns an enumerator that iterates through the user.
    /// </summary>
    /// <returns>The claim enumerator.</returns>
    public IEnumerator<Claim> EnumerateClaims()
    {
        return GetClaims().GetEnumerator();
    }

    private string HashPassword(string original)
    {
        var s = $"{original?.Trim()} - {Name?.Trim()}";
        return HashUtility.ComputeSHA512String(s);
    }

    private List<Claim> GetClaims()
    {
        var col = new List<Claim>
        {
            new Claim(ClaimTypes.Name, Nickname),
            new Claim(ClaimTypes.NameIdentifier, Name),
            new Claim(ClaimTypes.Gender, Gender.ToString())
        };
        if (Birthday.HasValue) col.Add(new Claim(ClaimTypes.DateOfBirth, Birthday.ToString()));
        return col;
    }

    /// <summary>
    /// Converts to claims identity.
    /// </summary>
    /// <param name="user">The user entity.</param>
    /// <returns>The claims identity.</returns>
    public static explicit operator ClaimsIdentity (UserEntity user)
    {
        if (user == null) return null;
        return new ClaimsIdentity(user.GetClaims());
    }
}

/// <summary>
/// The user owner relationship resource entity.
/// </summary>
[DataContract]
public class UserResourceEntity : OwnerResourceEntity<UserEntity>
{
    /// <summary>
    /// Initializes a new instance of the UserResourceEntity class.
    /// </summary>
    public UserResourceEntity()
    {
    }

    /// <summary>
    /// Initializes a new instance of the UserResourceEntity class.
    /// </summary>
    /// <param name="copy">The source to copy.</param>
    /// <param name="owner">The owner resource entity.</param>
    public UserResourceEntity(OwnerResourceEntity<UserEntity> copy, UserEntity owner)
        : base(copy, owner)
    {
    }
}

/// <summary>
/// The user owner relationship resource entity.
/// </summary>
/// <typeparam name="T">The type of target resource.</typeparam>
[DataContract]
public class UserResourceEntity<T> : OwnerResourceEntity<UserEntity, T> where T : BaseResourceEntity
{
    /// <summary>
    /// Initializes a new instance of the UserResourceEntity class.
    /// </summary>
    public UserResourceEntity()
    {
    }

    /// <summary>
    /// Initializes a new instance of the UserResourceEntity class.
    /// </summary>
    /// <param name="copy">The source to copy.</param>
    /// <param name="owner">The owner resource entity.</param>
    /// <param name="target">The target resource entity.</param>
    public UserResourceEntity(OwnerResourceEntity<UserEntity, T> copy, UserEntity owner, T target)
        : base(copy, owner, target)
    {
        if (!string.IsNullOrWhiteSpace(target?.Name)) Name = target.Name;
    }
}
