using UnityEngine;

namespace _Project.Application.Configs
{
    [CreateAssetMenu(fileName = "ServerConfig", menuName = "BrushDuel/Server Config", order = 0)]
    public class ServerConfig : ScriptableObject
    {
        [SerializeField] private string _address = "brushduel.jtlstudio.ru";
        [SerializeField] private ushort _serverPort = 7777;
        [SerializeField] private int _protocolVersion = 1;
        [SerializeField] private float _readyTimeoutSeconds = 30f;

        public string Address => _address;
        public ushort ServerPort => _serverPort;
        public int ProtocolVersion => _protocolVersion;
        public float ReadyTimeoutSeconds => _readyTimeoutSeconds;
    }
}
