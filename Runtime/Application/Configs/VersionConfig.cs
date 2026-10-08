using UnityEngine;

namespace _Project.Application.Configs
{
    [CreateAssetMenu(fileName = "VersionConfig", menuName = "BrushDuel/Version Config", order = 0)]
    public class VersionConfig : ScriptableObject
    {
        [SerializeField] private string _version = "0.1.0";

        public string Version => _version;
    }
}
