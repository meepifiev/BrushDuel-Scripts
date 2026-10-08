using System;
using _Project.Application.Match;
using _Project.Core.Match;
using _Project.Presentation.Popups;
using _Project.Presentation.UI;
using UnityEngine;
using VContainer;

namespace _Project.Presentation.Menu
{
    public class RoomModeSelectButton : ButtonHandler
    {
        [SerializeField] private PaintingCategory _category;
        [SerializeField] private PopupNavigator _navigator;
        [SerializeField] private PopupView _roomPaintingsPopup;

        private MatchLaunchContext _matchContext;

        [Inject]
        public void Construct(MatchLaunchContext matchContext)
        {
            _matchContext = matchContext ?? throw new ArgumentNullException(nameof(matchContext));
        }

        protected override void OnClick()
        {
            _matchContext.Setup(GameMode.Masterstroke, MatchType.Online);
            _matchContext.SetCategory(_category);
            _navigator.Open(_roomPaintingsPopup);
        }
    }
}
