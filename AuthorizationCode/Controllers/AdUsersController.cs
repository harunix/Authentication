using Azure.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Graph;

namespace AuthorizationCode.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AdUsersController : ControllerBase
    {

        private readonly ILogger<AdUsersController> _logger;
        private readonly IGraphService _graphService;
        private readonly EntraSettings _entraSettings;

        public AdUsersController(ILogger<AdUsersController> logger, IGraphService graphService,
            IOptions<EntraSettings> entraSettingsOptions)
        {
            _logger = logger;
            _graphService = graphService;
            _entraSettings = entraSettingsOptions.Value;
        }

        [HttpGet(Name = "")]
        public async Task<IEnumerable<AdUser>> Get()
        {
            var users = await _graphService.GetAdUsersAsync();
            return users;
        }

        [HttpGet, Route("me")]
        public async Task<IActionResult> GetMe([FromQuery] string? code = null)
        {
            if (string.IsNullOrEmpty(code))
            {
                string redirectUri = "http://localhost:7160/adusers/me";

                string url = $"https://login.microsoftonline.com/{_entraSettings.TenantId}/oauth2/v2.0/authorize" +
                             $"?client_id={_entraSettings.ClientId}" +
                             $"&response_type=code" +
                             $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
                             $"&response_mode=query" +
                             $"&scope=openid%20profile%20User.Read" +
                             $"&prompt=select_account";

                return Redirect(url);
            }

            // Step 2: Code received  exchange token
            var credential = new AuthorizationCodeCredential(
                _entraSettings.TenantId,
                _entraSettings.ClientId,
                _entraSettings.ClientSecret,
                code,
                new AuthorizationCodeCredentialOptions
                {
                    RedirectUri = new Uri("http://localhost:7160/adusers/me")
                });

            var graphClient = new GraphServiceClient(credential);

            var me = await graphClient.Me.GetAsync(config =>
            {
                config.QueryParameters.Select = ["id", "displayName", "userPrincipalName", "mail", "onPremisesSamAccountName"];
            });

            return new OkObjectResult(new AdUser
            {
                Id = me.Id,
                DisplayName = me.DisplayName,
                UserPrincipalName = me.UserPrincipalName,
                Email = me.Mail,
                Initial = me.OnPremisesSamAccountName
            });
        }
    }
}

