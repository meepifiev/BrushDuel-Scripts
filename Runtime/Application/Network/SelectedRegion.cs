using _Project.Application.Configs;

namespace _Project.Application.Network
{
    public class SelectedRegion
    {
        public ServerRegion Current { get; private set; }

        public void Set(ServerRegion region)
        {
            Current = region;
        }
    }
}
