using System;
using Newtonsoft.Json;

namespace Thread_.NET.DAL.Entities
{
    public sealed class PasswordTokenRequest
    {

        public PasswordTokenRequest()
        {
            SigningKey = Environment.GetEnvironmentVariable("SecretJWTKey");
        }

        public string Password { get; set; }
        public string Token { get; set; }

        [JsonIgnore]
        public string SigningKey { get; private set; }
    }
}
