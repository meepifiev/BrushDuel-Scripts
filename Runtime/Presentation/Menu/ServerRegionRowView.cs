using System;
using _Project.Application.Configs;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Presentation.Menu
{
    public class ServerRegionRowView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _pingText;
        [SerializeField] private Button _button;
        [SerializeField] private GameObject _selectedMarker;

        [SerializeField] private int _goodMaxMs = 100;
        [SerializeField] private int _okMaxMs = 200;
        [SerializeField] private Color _goodColor = new Color(0.35f, 0.85f, 0.4f);
        [SerializeField] private Color _okColor = new Color(0.95f, 0.8f, 0.25f);
        [SerializeField] private Color _badColor = new Color(0.9f, 0.35f, 0.3f);
        [SerializeField] private Color _unknownColor = new Color(0.6f, 0.6f, 0.6f);

        private ServerRegion _region;
        private Action<ServerRegion> _onClick;

        public ServerRegion Region => _region;

        public void Bind(ServerRegion region, string displayName, Action<ServerRegion> onClick)
        {
            _region = region;
            _onClick = onClick;

            if (_nameText != null)
            {
                _nameText.text = displayName;
            }

            SetPing(-1);
            SetSelected(false);

            _button.onClick.RemoveListener(HandleClick);
            _button.onClick.AddListener(HandleClick);
        }

        public void SetPing(int milliseconds)
        {
            if (_pingText == null)
            {
                return;
            }

            if (milliseconds < 0)
            {
                _pingText.text = "...";
                _pingText.color = _unknownColor;
                return;
            }

            _pingText.text = milliseconds + " ms";

            if (milliseconds <= _goodMaxMs)
            {
                _pingText.color = _goodColor;
            }
            else if (milliseconds <= _okMaxMs)
            {
                _pingText.color = _okColor;
            }
            else
            {
                _pingText.color = _badColor;
            }
        }

        public void SetSelected(bool selected)
        {
            if (_selectedMarker != null)
            {
                _selectedMarker.SetActive(selected);
            }
        }

        private void HandleClick()
        {
            _onClick?.Invoke(_region);
        }
    }
}
