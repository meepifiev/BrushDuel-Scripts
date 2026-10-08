using _Project.Application.Profile;
using Steamworks;

namespace _Project.Infrastructure.Steam
{
    public class SteamPlayerProfileService : IPlayerProfileService
    {
        public string Nickname => SteamClient.IsValid ? SteamClient.Name : string.Empty;

        public string PlayerId => SteamClient.IsValid ? SteamClient.SteamId.Value.ToString() : string.Empty;
    }
}
