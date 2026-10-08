using UnityEngine;

namespace _Project.Application.Configs
{
    [CreateAssetMenu(fileName = "BootConfig", menuName = "BrushDuel/Boot Config", order = 0)]
    public class BootConfig : ScriptableObject
    {
        [SerializeField] private float _introDuration = 2.4f;
        [SerializeField] private float _fadeDuration = 0.4f;

        public float IntroDuration => _introDuration;
        public float FadeDuration => _fadeDuration;
    }
}
