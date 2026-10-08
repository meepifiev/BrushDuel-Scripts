using System;
using _Project.Application.Localization;
using _Project.Presentation.Drawing;
using TMPro;
using UnityEngine;
using VContainer;

namespace _Project.Presentation.Match
{
    [RequireComponent(typeof(TMP_Text))]
    public class SimilarityView : MonoBehaviour
    {
        private const float PercentMultiplier = 100f;

        private TMP_Text _label;
        private SimilarityHandler _similarityHandler;
        private ILocalizationService _localization;

        [Inject]
        public void Construct(SimilarityHandler similarityHandler, ILocalizationService localization)
        {
            _similarityHandler = similarityHandler ?? throw new ArgumentNullException(nameof(similarityHandler));
            _localization = localization ?? throw new ArgumentNullException(nameof(localization));
        }

        private void Awake()
        {
            _label = GetComponent<TMP_Text>();
        }

        private void OnEnable()
        {
            _similarityHandler.SimilarityChanged += OnSimilarityChanged;
        }

        private void OnDisable()
        {
            _similarityHandler.SimilarityChanged -= OnSimilarityChanged;
        }

        private void OnSimilarityChanged(float similarity)
        {
            int percent = Mathf.RoundToInt(similarity * PercentMultiplier);
            _label.text = string.Format(_localization.Get("hud.similarity"), percent);
        }
    }
}
