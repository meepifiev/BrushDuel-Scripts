using _Project.Core.Scoring;
using UnityEngine;

namespace _Project.Application.Configs
{
    [CreateAssetMenu(fileName = "ScoringConfig", menuName = "BrushDuel/Scoring Config", order = 0)]
    public class ScoringConfig : ScriptableObject
    {
        [Header("Masterstroke: точность по карте вердиктов + контур")]
        [SerializeField] [Range(0f, 1f)] private float _masterPixelExponent = 0.6f;
        [SerializeField] [Range(0f, 1f)] private float _masterEdgeExponent = 0.4f;
        [SerializeField] [Range(0, 12)] private int _masterSilhouetteToleranceCells = 4;
        [SerializeField] [Range(0.5f, 12f)] private float _masterEdgeThetaCells = 6f;
        [SerializeField] [Range(0f, 2f)] private float _masterWrongColorPenalty = 0.8f;
        [SerializeField] [Range(0f, 2f)] private float _masterOverpaintPenalty = 0.6f;
        [SerializeField] [Range(0f, 2f)] private float _masterMissedPenalty = 1f;

        [Header("Wildstroke: узнаваемость мягче, окно точности")]
        [SerializeField] [Range(0, 16)] private int _wildSilhouetteToleranceCells = 10;
        [SerializeField] [Range(0.5f, 12f)] private float _wildEdgeThetaCells = 3f;
        [SerializeField] [Range(0f, 1f)] private float _wildWindowLow = 0.2f;
        [SerializeField] [Range(0f, 1f)] private float _wildWindowComfortMin = 0.4f;
        [SerializeField] [Range(0f, 1f)] private float _wildWindowComfortMax = 0.6f;
        [SerializeField] [Range(0f, 1f)] private float _wildWindowHigh = 0.9f;
        [SerializeField] [Range(0f, 1f)] private float _wildOverAccuracyPenalty = 0.6f;

        public MasterstrokeSettings BuildMasterstroke()
        {
            return new MasterstrokeSettings(
                _masterPixelExponent,
                _masterEdgeExponent,
                _masterSilhouetteToleranceCells,
                _masterEdgeThetaCells,
                _masterWrongColorPenalty,
                _masterOverpaintPenalty,
                _masterMissedPenalty);
        }

        public WildstrokeSettings BuildWildstroke()
        {
            return new WildstrokeSettings(
                _wildSilhouetteToleranceCells,
                _wildEdgeThetaCells,
                _wildWindowLow,
                _wildWindowComfortMin,
                _wildWindowComfortMax,
                _wildWindowHigh,
                _wildOverAccuracyPenalty);
        }
    }
}
