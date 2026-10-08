using UnityEngine;

namespace _Project.Application.Configs
{
    [System.Serializable]
    public class ServerRegion
    {
        [SerializeField] private string _id;
        [SerializeField] private string _nameKey;
        [SerializeField] private string _address;
        [SerializeField] private ushort _clientPort = 7777;

        public string Id => _id;
        public string NameKey => _nameKey;
        public string Address => _address;
        public ushort ClientPort => _clientPort;
    }
}
