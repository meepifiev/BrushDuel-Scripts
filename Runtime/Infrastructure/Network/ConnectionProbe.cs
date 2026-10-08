using Mirror;
using UnityEngine;

namespace _Project.Infrastructure.Network
{
    public class ConnectionProbe : NetworkBehaviour
    {
        [SyncVar] private int _onlineAtSpawn;

        public override void OnStartServer()
        {
            _onlineAtSpawn = NetworkServer.connections.Count;
        }

        public override void OnStartLocalPlayer()
        {
            Debug.Log(nameof(ConnectionProbe) + ": connected, online " + _onlineAtSpawn);
        }
    }
}
