using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using System.Security.Principal;
using System.Text;
using System.Text.Json.Serialization;

using NuScien.Data;
using Trivial.Reflection;
using Trivial.Text;

namespace NuScien.Security
{
    public class TokenEntity : BaseResourceEntity
    {
        public static TimeSpan DefaultTimeout = new TimeSpan(2, 0, 0);

        [Column("refreshtoken")]
        public string RefreshToken
        {
            get => GetCurrentProperty<string>();
            set => SetCurrentProperty(value);
        }

        [Column("expiration")]
        public DateTime ExpirationTime
        {
            get => GetCurrentProperty(DateTime.Now + DefaultTimeout);
            set => SetCurrentProperty(value);
        }

        [NotMapped]
        [JsonIgnore]
        public bool IsExpired
        {
            get => ExpirationTime <= DateTime.Now;
        }

        [NotMapped]
        [JsonIgnore]
        public bool IsClosedToExpiration
        {
            get => (ExpirationTime - LastModificationTime).TotalSeconds / 4 < (ExpirationTime - DateTime.Now).TotalSeconds;
        }

        [Column("user")]
        public string UserId
        {
            get => GetCurrentProperty<string>();
            set => SetCurrentProperty(value);
        }

        [Column("client")]
        public string ClientId
        {
            get => GetCurrentProperty<string>();
            set => SetCurrentProperty(value);
        }

        [Column("granttype")]
        public string GrantType
        {
            get => GetCurrentProperty<string>();
            set => SetCurrentProperty(value);
        }

        public void CreateToken(bool includeRefreshToken = false)
        {
            Name = Guid.NewGuid().ToString("n") + Guid.NewGuid().ToString("n");
            ExpirationTime = DateTime.Now + DefaultTimeout;
            if (includeRefreshToken) RefreshToken = Guid.NewGuid().ToString("n") + Guid.NewGuid().ToString("n");
        }

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
}
