using Mirror;

namespace _Project.Infrastructure.Network
{
    public readonly struct ServerPlayer
    {
        public ServerPlayer(NetworkConnectionToClient connection, string nickname, string playerId)
        {
            Connection = connection;
            Nickname = nickname;
            PlayerId = playerId;
        }

        public NetworkConnectionToClient Connection { get; }
        public string Nickname { get; }
        public string PlayerId { get; }
    }
}
