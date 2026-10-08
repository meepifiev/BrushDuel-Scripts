using Mirror;
using UnityEngine;

namespace _Project.Infrastructure.Network
{
    public static class ServerLog
    {
        private const string Prefix = "[Server] ";

        public static void Info(string message)
        {
            Debug.Log(Prefix + message);
        }

        public static void Warn(string message)
        {
            Debug.LogWarning(Prefix + message);
        }

        public static void Error(string message)
        {
            Debug.LogError(Prefix + message);
        }

        public static string Describe(ServerPlayer player)
        {
            return Describe(player.Connection) + " '" + Nickname(player.Nickname) + "'";
        }

        public static string Describe(NetworkConnectionToClient connection)
        {
            return connection == null ? "conn#none" : "conn#" + connection.connectionId;
        }

        private static string Nickname(string nickname)
        {
            return string.IsNullOrEmpty(nickname) ? "anonymous" : nickname;
        }
    }
}
