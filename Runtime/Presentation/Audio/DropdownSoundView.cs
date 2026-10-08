using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using VContainer;

namespace _Project.Presentation.Audio
{
    [RequireComponent(typeof(TMP_Dropdown))]
    public class DropdownSoundView : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private AudioAsset _openAsset;
        [SerializeField] private AudioAsset _selectAsset;

        private IAudioService _audioService;

        [Inject]
        public void Construct(IAudioService audioService)
        {
            _audioService = audioService ?? throw new ArgumentNullException(nameof(audioService));
        }

        private void Awake()
        {
            GetComponent<TMP_Dropdown>().onValueChanged.AddListener(OnValueChanged);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            _audioService.PlayOneShot(_openAsset);
        }

        private void OnValueChanged(int value)
        {
            _audioService.PlayOneShot(_selectAsset);
        }
    }
}
