using System.Collections.Generic;
using UnityEngine;

namespace _Project.Application.Configs
{
    [CreateAssetMenu(fileName = "ServerRegionsConfig", menuName = "BrushDuel/Server Regions Config", order = 1)]
    public class ServerRegionsConfig : ScriptableObject
    {
        [SerializeField] private ServerRegion[] _regions;

        public IReadOnlyList<ServerRegion> Regions => _regions;
    }
}
