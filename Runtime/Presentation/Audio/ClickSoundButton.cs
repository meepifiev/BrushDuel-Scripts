using System;
using _Project.Presentation.UI;
using UnityEngine;
using VContainer;

namespace _Project.Presentation.Audio
{
    public class ClickSoundButton : ButtonHandler
    {
        [SerializeField] private AudioAsset _asset;

        private IAudioService _audioService;

        [Inject]
        public void Construct(IAudioService audioService)
        {
            _audioService = audioService ?? throw new ArgumentNullException(nameof(audioService));
        }

        protected override void OnClick()
        {
            _audioService.PlayOneShot(_asset);
        }
    }
}
