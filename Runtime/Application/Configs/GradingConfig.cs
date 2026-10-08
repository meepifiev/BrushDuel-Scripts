using System;
using UnityEngine;

namespace _Project.Application.Configs
{
    [CreateAssetMenu(fileName = "GradingConfig", menuName = "BrushDuel/Grading Config", order = 0)]
    public class GradingConfig : ScriptableObject
    {
        [Serializable]
        public class GradeEntry
        {
            [SerializeField] private string _label;
            [SerializeField] private string _subtitleKey;
            [SerializeField] [Range(0f, 1f)] private float _minSimilarity;

            public string Label => _label;
            public string SubtitleKey => _subtitleKey;
            public float MinSimilarity => _minSimilarity;
        }

        [SerializeField] private GradeEntry[] _grades;
        [SerializeField] private int _maxCoins = 100;

        public int MaxCoins => _maxCoins;

        public GradeEntry Resolve(float similarity)
        {
            for (int i = 0; i < _grades.Length; i++)
            {
                if (similarity >= _grades[i].MinSimilarity)
                {
                    return _grades[i];
                }
            }

            return _grades[_grades.Length - 1];
        }
    }
}
