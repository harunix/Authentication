using Microsoft.Graph;

namespace ClientSecret
{
    public interface IGraphService
    {
        GraphServiceClient GetGraphServiceClient(bool withAuthority = true);
        Task<AdUser?> GetAdUserAsync(string email, CancellationToken cancellationToken = default);
        Task<IEnumerable<AdUser>> GetAdUsersAsync(CancellationToken cancellationToken = default);
    }

    public class AdUser
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public string UserPrincipalName { get; set; }
        public string Email { get; set; }
        public string Initial { get; set; }
    }
}
