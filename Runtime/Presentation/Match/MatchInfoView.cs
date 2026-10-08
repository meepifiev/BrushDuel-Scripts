using System;
using _Project.Application.Localization;
using _Project.Application.Match;
using _Project.Core.Match;
using TMPro;
using UnityEngine;
using VContainer;

namespace _Project.Presentation.Match
{
    [RequireComponent(typeof(TMP_Text))]
    public class MatchInfoView : MonoBehaviour
    {
        private TMP_Text _label;
        private MatchLaunchContext _context;
        private ILocalizationService _localization;

        [Inject]
        public void Construct(MatchLaunchContext context, ILocalizationService localization)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _localization = localization ?? throw new ArgumentNullException(nameof(localization));
        }

        private void Awake()
        {
            _label = GetComponent<TMP_Text>();
        }

        private void Start()
        {
            PaintingCategory category = _context.Reference != null ? _context.Reference.Category : _context.Category;
            _label.text = FormatCategory(category) + " - " + FormatMatchType(_context.MatchType);
        }

        private string FormatCategory(PaintingCategory category)
        {
            switch (category)
            {
                case PaintingCategory.Famous:
                    return _localization.Get("category.famous");
                case PaintingCategory.Memes:
                    return _localization.Get("category.memes");
                default:
                    throw new ArgumentOutOfRangeException(nameof(category));
            }
        }

        private string FormatMatchType(MatchType matchType)
        {
            switch (matchType)
            {
                case MatchType.Offline:
                    return _localization.Get("info.single");
                case MatchType.Online:
                    return _localization.Get("info.online");
                default:
                    throw new ArgumentOutOfRangeException(nameof(matchType));
            }
        }
    }
}
