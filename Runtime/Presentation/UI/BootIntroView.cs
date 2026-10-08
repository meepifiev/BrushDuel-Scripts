using System.Threading;
using _Project.Application.Boot;
using _Project.Application.Configs;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace _Project.Presentation.UI
{
    public class BootIntroView : MonoBehaviour, IBootIntro
    {
        [SerializeField] private Image _logoImage;
        [SerializeField] private Sprite[] _frames;
        [SerializeField] private CanvasGroup _loadingGroup;
        [SerializeField] private AudioSource _writeSound;

        private BootConfig _config;

        [Inject]
        public void Construct(BootConfig config)
        {
            _config = config;
        }

        public async UniTask PlayAsync(CancellationToken cancellationToken)
        {
            using CancellationTokenSource linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, destroyCancellationToken);

            _logoImage.DOFade(1f, _config.FadeDuration).SetEase(Ease.OutQuad).SetLink(gameObject);
            _loadingGroup.DOFade(1f, _config.FadeDuration).SetEase(Ease.OutQuad).SetLink(gameObject);
            _writeSound.Play();

            float duration = _config.IntroDuration;
            int lastIndex = _frames.Length - 1;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                int index = Mathf.Min(Mathf.FloorToInt(elapsed / duration * _frames.Length), lastIndex);
                _logoImage.sprite = _frames[index];
                await UniTask.Yield(PlayerLoopTiming.Update, linked.Token);
                elapsed += Time.deltaTime;
            }

            _logoImage.sprite = _frames[lastIndex];
        }
    }
}
