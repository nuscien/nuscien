using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using System.Security.Principal;
using System.Text;
using System.Text.Json.Serialization;

using NuScien.Data;
using NuScien.Reflection;
using Trivial.Reflection;
using Trivial.Text;

namespace NuScien.Security;

/// <summary>
/// Token entity.
/// </summary>
[Table("nstokens")]
public class TokenEntity : BaseResourceEntity
{
    /// <summary>
    /// The default timeout span.
    /// </summary>
    public static TimeSpan DefaultTimeout = new TimeSpan(2, 0, 0);

    /// <summary>
    /// Gets or sets the refresh token.
    /// </summary>
    [Column("refreshtoken")]
    public string RefreshToken
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets or sets the expiration time.
    /// </summary>
    [Column("expiration")]
    public DateTime ExpirationTime
    {
        get => GetCurrentProperty(DateTime.Now + DefaultTimeout);
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets a value indicating whether it is expired.
    /// </summary>
    [NotMapped]
    [JsonIgnore]
    public bool IsExpired
    {
        get => ExpirationTime <= DateTime.Now;
    }

    /// <summary>
    /// Gets a value indicating whether it is closed to expiration.
    /// </summary>
    [NotMapped]
    [JsonIgnore]
    public bool IsClosedToExpiration
    {
        get
        {
            var rest = ExpirationTime - DateTime.Now;
            var total = ExpirationTime - LastModificationTime;
#if NETSTANDARD2_0 || NETFRAMEWORK
            return (total.TotalSeconds / 4 > rest.TotalSeconds) || (ExpirationTime <= DateTime.Now);
#else
            return (total / 4 > rest) || (ExpirationTime <= DateTime.Now);
#endif
        }
    }

    /// <summary>
    /// Gets or sets the user identifier.
    /// </summary>
    [Column("user")]
    public string UserId
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets or sets the client identifier.
    /// </summary>
    [Column("client")]
    public string ClientId
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets or sets the grant type.
    /// </summary>
    [Column("granttype")]
    public string GrantType
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Gets or sets the scope string.
    /// </summary>
    [Column("scope")]
    public string ScopeString
    {
        get => GetCurrentProperty<string>();
        set => SetCurrentProperty(value);
    }

    /// <summary>
    /// Creates a new token.
    /// </summary>
    /// <param name="includeRefreshToken">true if also renew the refresh token.</param>
    public void CreateToken(bool includeRefreshToken = false)
    {
        Name = Guid.NewGuid().ToString("n") + Guid.NewGuid().ToString("n");
        ExpirationTime = DateTime.Now + DefaultTimeout;
        if (includeRefreshToken) RefreshToken = Guid.NewGuid().ToString("n") + Guid.NewGuid().ToString("n");
    }

    /// <summary>
    /// Creates a token entity from a specific user.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="tokenRequest">The token request.</param>
    /// <returns>A token entity from the given user.</returns>
    public static TokenEntity Create(Users.UserEntity user, Trivial.Security.TokenRequest tokenRequest)
    {
        InternalAssertion.IsNotNull(user, nameof(user));
        InternalAssertion.IsNotNull(tokenRequest, nameof(tokenRequest));
        if (user.IsNew || string.IsNullOrWhiteSpace(user.Name)) throw new ArgumentException("user does not exist.", nameof(user));
        var token = new TokenEntity
        {
            UserId = user.Id,
            ClientId = tokenRequest.ClientId,
            GrantType = tokenRequest.GrantType
        };
        token.CreateToken(true);
        return token;
    }
}
