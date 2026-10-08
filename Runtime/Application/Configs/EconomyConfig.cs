using UnityEngine;

namespace _Project.Application.Configs
{
    [CreateAssetMenu(fileName = "EconomyConfig", menuName = "BrushDuel/Economy Config", order = 0)]
    public class EconomyConfig : ScriptableObject
    {
        [SerializeField] private int _startingCoins = 300;

        public int StartingCoins => _startingCoins;
    }
}
