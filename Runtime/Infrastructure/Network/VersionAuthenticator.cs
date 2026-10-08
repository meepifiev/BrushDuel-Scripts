using System.Collections;
using _Project.Application.Configs;
using Mirror;
using UnityEngine;

namespace _Project.Infrastructure.Network
{
    public class VersionAuthenticator : NetworkAuthenticator
    {
        private struct VersionRequestMessage : NetworkMessage
        {
            public int ProtocolVersion;
        }

        private struct VersionResponseMessage : NetworkMessage
        {
            public bool Accepted;
        }

        [SerializeField] private ServerConfig _serverConfig;

        private const float AuthTimeoutSeconds = 6f;

        public override void OnStartServer()
        {
            NetworkServer.RegisterHandler<VersionRequestMessage>(OnVersionRequest, false);
        }

        public override void OnStopServer()
        {
            NetworkServer.UnregisterHandler<VersionRequestMessage>();
        }

        public override void OnServerAuthenticate(NetworkConnectionToClient conn)
        {
            StartCoroutine(AuthTimeout(conn));
        }

        public override void OnStartClient()
        {
            NetworkClient.RegisterHandler<VersionResponseMessage>(OnVersionResponse, false);
        }

        public override void OnStopClient()
        {
            NetworkClient.UnregisterHandler<VersionResponseMessage>();
        }

        public override void OnClientAuthenticate()
        {
            NetworkClient.Send(new VersionRequestMessage { ProtocolVersion = _serverConfig.ProtocolVersion });
        }

        private void OnVersionRequest(NetworkConnectionToClient conn, VersionRequestMessage message)
        {
            bool accepted = message.ProtocolVersion == _serverConfig.ProtocolVersion;
            conn.Send(new VersionResponseMessage { Accepted = accepted });

            if (accepted)
            {
                ServerAccept(conn);
                return;
            }

            conn.isAuthenticated = false;
            StartCoroutine(DelayedReject(conn));
        }

        private IEnumerator DelayedReject(NetworkConnectionToClient conn)
        {
            yield return new WaitForSeconds(1f);
            ServerReject(conn);
        }

        private IEnumerator AuthTimeout(NetworkConnectionToClient conn)
        {
            yield return new WaitForSeconds(AuthTimeoutSeconds);

            if (conn.isAuthenticated == false && NetworkServer.connections.ContainsKey(conn.connectionId))
            {
                conn.Disconnect();
            }
        }

        private void OnVersionResponse(VersionResponseMessage message)
        {
            if (message.Accepted)
            {
                ClientAccept();
                return;
            }

            ClientReject();
        }
    }
}
