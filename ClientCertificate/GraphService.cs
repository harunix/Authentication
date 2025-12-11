using Azure.Identity;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using System.Security.Cryptography.X509Certificates;

namespace ClientCertificate
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
            /* Load certificate from .pfx file */
            //var cert = new X509Certificate2(_entraSettings.CertificatePath, _entraSettings.CertificatePassword, X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.EphemeralKeySet | X509KeyStorageFlags.Exportable);

            /* Load certificate from Base64 string */
            /*
            byte[] certBytes = Convert.FromBase64String(_entraSettings.CertificateBase64 ?? throw new ArgumentNullException("CertificateBase64 is null"));
            var cert = new X509Certificate2(certBytes, _entraSettings.CertificatePassword, X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.EphemeralKeySet | X509KeyStorageFlags.Exportable);
            */

            /* Load certificate from Certificate Store by Thumbprint */
            X509Certificate2? cert = null;
            using (var store = new X509Store(StoreName.My, StoreLocation.CurrentUser))
            {
                store.Open(OpenFlags.ReadOnly);
                cert = store.Certificates
                    .Find(X509FindType.FindByThumbprint, _entraSettings.CertificateThumbPrint, false)
                    .OfType<X509Certificate2>()
                    .FirstOrDefault();
            }


            if (cert == null)
                throw new Exception("Certificate not found!");

            var options = new ClientCertificateCredentialOptions
            {
                SendCertificateChain = true
            };
            if (withAuthority)
            {
                options.AuthorityHost = AzureAuthorityHosts.AzurePublicCloud;
            }

            var credential = new ClientCertificateCredential(_entraSettings.TenantId, _entraSettings.ClientId, cert, options);
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
