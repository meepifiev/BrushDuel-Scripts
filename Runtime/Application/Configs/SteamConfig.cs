using UnityEngine;

namespace _Project.Application.Configs
{
    [CreateAssetMenu(fileName = "SteamConfig", menuName = "BrushDuel/Steam Config", order = 0)]
    public class SteamConfig : ScriptableObject
    {
        [SerializeField] private uint _appId = 3331850;

        public uint AppId => _appId;
    }
}
