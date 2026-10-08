using _Project.Application.Localization;
using _Project.Presentation.Drawing;
using _Project.Presentation.UI;
using TMPro;
using UnityEngine;
using VContainer;

namespace _Project.Presentation.Match
{
    public class OpponentVisibilityToggle : ButtonHandler
    {
        private const string HideKey = "match.hideOpponent";
        private const string ShowKey = "match.showOpponent";

        [SerializeField] private GameObject _opponentCanvas;
        [SerializeField] private GameObject _playerNicknameLabel;
        [SerializeField] private DrawingCanvasView _playerCanvas;
        [SerializeField] private RectTransform _referencePanel;
        [SerializeField] private RectTransform _palette;
        [SerializeField] private TMP_Text _label;

        [Header("Layout without the opponent canvas")]
        [SerializeField] private Vector2 _soloCanvasPosition;
        [SerializeField] private Vector2 _soloCanvasSize;
        [SerializeField] private Vector2 _soloReferencePosition;
        [SerializeField] private Vector2 _soloPalettePosition;

        private RectTransform _playerCanvasRect;
        private Vector2 _splitCanvasPosition;
        private Vector2 _splitCanvasSize;
        private Vector2 _splitReferencePosition;
        private Vector2 _splitPalettePosition;
        private bool _opponentHidden;
        private ILocalizationService _localization;

        [Inject]
        public void Construct(ILocalizationService localization)
        {
            _localization = localization;
        }

        protected override void Awake()
        {
            base.Awake();

            _playerCanvasRect = (RectTransform)_playerCanvas.transform;
            _splitCanvasPosition = _playerCanvasRect.anchoredPosition;
            _splitCanvasSize = _playerCanvasRect.sizeDelta;
            _splitReferencePosition = _referencePanel.anchoredPosition;
            _splitPalettePosition = _palette.anchoredPosition;
        }

        private void Start()
        {
            RefreshLabel();
        }

        protected override void OnClick()
        {
            _opponentHidden = _opponentHidden == false;
            ApplyLayout();
        }

        public void ForceShowOpponent()
        {
            if (_opponentHidden == false)
            {
                return;
            }

            _opponentHidden = false;
            ApplyLayout();
        }

        private void ApplyLayout()
        {
            _opponentCanvas.SetActive(_opponentHidden == false);
            _playerNicknameLabel.SetActive(_opponentHidden == false);

            _referencePanel.anchoredPosition = _opponentHidden ? _soloReferencePosition : _splitReferencePosition;
            _palette.anchoredPosition = _opponentHidden ? _soloPalettePosition : _splitPalettePosition;
            _playerCanvasRect.anchoredPosition = _opponentHidden ? _soloCanvasPosition : _splitCanvasPosition;
            _playerCanvas.FitInto(_opponentHidden ? _soloCanvasSize : _splitCanvasSize);

            RefreshLabel();
        }

        private void RefreshLabel()
        {
            if (_label == null || _localization == null)
            {
                return;
            }

            _label.text = _localization.Get(_opponentHidden ? ShowKey : HideKey);
        }
    }
}
