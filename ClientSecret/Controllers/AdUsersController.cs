using Microsoft.AspNetCore.Mvc;

namespace ClientSecret.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AdUsersController : ControllerBase
    {
        
        private readonly ILogger<AdUsersController> _logger;
        private readonly IGraphService _graphService;

        public AdUsersController(ILogger<AdUsersController> logger, IGraphService graphService)
        {
            _logger = logger;
            _graphService = graphService;
        }

        [HttpGet(Name = "")]
        public async Task<IEnumerable<AdUser>> Get()
        {
            var users = await _graphService.GetAdUsersAsync();
            return users;
        }
    }
}
