using Azure.Identity;
using Microsoft.Extensions.Options;
using Microsoft.Graph;

namespace ClientSecret
{
    public class GraphService : IGraphService
    {
        private readonly EntraSettings _entraSettings;
        private readonly ILogger<GraphService> _logger;
        private static GraphServiceClient? _graphServiceClient;
        public GraphService(IOptions<EntraSettings> entraSettingsOptions, ILogger<GraphService> logger)
        {
            _entraSettings = entraSettingsOptions.Value;
            _logger = logger;
        }

        public GraphServiceClient GetGraphServiceClient(bool withAuthority = true)
        {
            var options = new ClientCertificateCredentialOptions
            {
                SendCertificateChain = true
            };
            if (withAuthority)
            {
                options.AuthorityHost = AzureAuthorityHosts.AzurePublicCloud;
            }

            var credential = new ClientSecretCredential(_entraSettings.TenantId, _entraSettings.ClientId, _entraSettings.ClientSecret);
            string?[] scopes = [_entraSettings.DefaultScope];
            var graphClient = new GraphServiceClient(credential, scopes);
            return graphClient;
        }

        public async Task<AdUser?> GetAdUserAsync(string email, CancellationToken cancellationToken = default)
        {
            _graphServiceClient = GetGraphServiceClient(true);
            var users = await _graphServiceClient.Users.GetAsync(config =>
            {
                config.Headers.Add("Prefer", "HonorNonIndexedQueriesWarningMayFailRandomly");
                config.QueryParameters.Filter = $"userPrincipalName  eq '{email}' or mail eq '{email}'";
                config.QueryParameters.Select = ["id", "displayName", "userPrincipalName", "mail", "onPremisesSamAccountName"];
            });
            var user = users?.Value?.FirstOrDefault();
            if (user == null)
            {
                _logger.LogWarning("No user found with email - {email}", email);
                return null;
            }
            return new AdUser
            {
                Id = user.Id,
                DisplayName = user.DisplayName,
                UserPrincipalName = user.UserPrincipalName,
                Email = user.Mail,
                Initial = user.OnPremisesSamAccountName
            };
        }

        public async Task<IEnumerable<AdUser>> GetAdUsersAsync(CancellationToken cancellationToken = default)
        {
            _graphServiceClient = GetGraphServiceClient(true);
            var users = await _graphServiceClient.Users.GetAsync(config =>
            {
                config.Headers.Add("Prefer", "HonorNonIndexedQueriesWarningMayFailRandomly");
                config.QueryParameters.Select = ["id", "displayName", "userPrincipalName", "mail", "onPremisesSamAccountName"];
            });
            return users.Value.Select(x => new AdUser { Id = x.Id, DisplayName = x.DisplayName, UserPrincipalName = x.UserPrincipalName, Email = x.Mail, Initial = x.OnPremisesSamAccountName });
        }

    }
}
