using System;
using System.Threading;
using System.Threading.Tasks;
using BaGet.Core.Configuration;
using Microsoft.Extensions.Options;

namespace BaGet.Core.Authentication
{
    public class ApiKeyAuthenticationService : IAuthenticationService
    {
        private readonly string _apiKey;

        public ApiKeyAuthenticationService(IOptions<BaGetOptions> options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));

            _apiKey = string.IsNullOrEmpty(options.Value.ApiKey) ? string.Empty : options.Value.ApiKey;
        }

        public Task<bool> AuthenticateAsync(string apiKey, CancellationToken cancellationToken)
            => Task.FromResult(Authenticate(apiKey));

        private bool Authenticate(string apiKey)
        {
            // No authentication is necessary if there is no required API key.
            if (string.IsNullOrEmpty(_apiKey)) return true;

            return _apiKey == apiKey;
        }
    }
}
