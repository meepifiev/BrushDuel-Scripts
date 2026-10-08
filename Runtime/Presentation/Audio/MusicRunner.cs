using System;
using UnityEngine;
using VContainer;

namespace _Project.Presentation.Audio
{
    public class MusicRunner : MonoBehaviour
    {
        [SerializeField] private AudioAsset _asset;

        private IAudioService _audioService;

        [Inject]
        public void Construct(IAudioService audioService)
        {
            _audioService = audioService ?? throw new ArgumentNullException(nameof(audioService));
        }

        private void Start()
        {
            _audioService.PlayMusic(_asset);
        }
    }
}