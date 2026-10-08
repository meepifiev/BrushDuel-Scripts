using System;
using _Project.Application.Localization;
using _Project.Application.Match;
using _Project.Application.Profile;
using TMPro;
using UnityEngine;
using VContainer;

namespace _Project.Presentation.Match
{
    [RequireComponent(typeof(TMP_Text))]
    public class NicknameLabelView : MonoBehaviour
    {
        [SerializeField] private bool _showsOpponent;

        private TMP_Text _label;
        private MatchLaunchContext _context;
        private IPlayerProfileService _profile;
        private ILocalizationService _localization;

        [Inject]
        public void Construct(MatchLaunchContext context, IPlayerProfileService profile, ILocalizationService localization)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _profile = profile ?? throw new ArgumentNullException(nameof(profile));
            _localization = localization ?? throw new ArgumentNullException(nameof(localization));
        }

        private void Awake()
        {
            _label = GetComponent<TMP_Text>();
        }

        private void Start()
        {
            string nickname = _showsOpponent ? _context.OpponentNickname : _profile.Nickname;

            if (string.IsNullOrEmpty(nickname))
            {
                nickname = _localization.Get("net.anonymous");
            }

            if (_showsOpponent == false)
            {
                nickname = string.Format(_localization.Get("net.you"), nickname);
            }

            _label.text = nickname;
        }
    }
}
