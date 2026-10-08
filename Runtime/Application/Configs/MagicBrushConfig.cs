using UnityEngine;

namespace _Project.Application.Configs
{
    [CreateAssetMenu(fileName = "MagicBrushConfig", menuName = "BrushDuel/Magic Brush Config", order = 0)]
    public class MagicBrushConfig : ScriptableObject
    {
        [SerializeField] [Range(10f, 600f)] private float _cooldownSeconds = 90f;
        [SerializeField] [Range(1f, 25f)] private float _coveragePercent = 5f;
        [SerializeField] [Range(1, 10)] private int _usesPerRound = 2;
        [SerializeField] private string _rewardId = "magic_brush";
        [SerializeField] private int _coinPrice = 150;

        public float CooldownSeconds => _cooldownSeconds;
        public float CoveragePercent => _coveragePercent;
        public int UsesPerRound => _usesPerRound;
        public string RewardId => _rewardId;
        public int CoinPrice => _coinPrice;
    }
}
