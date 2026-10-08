using System;
using System.Collections.Generic;
using System.Threading;
using _Project.Application.Advertising;
using _Project.Application.AssetManagement;
using _Project.Application.Configs;
using _Project.Application.Factories;
using _Project.Application.Match;
using _Project.Application.Progress;
using _Project.Application.SceneManagement;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace _Project.Presentation.Menu
{
    public class PaintingListView : MonoBehaviour
    {
        private const int PageSize = 4;
        private const float CardAppearDelayStep = 0.08f;

        [SerializeField] private PaintingCardView _cardPrefab;
        [SerializeField] private RectTransform _cardsContainer;
        [SerializeField] private Button _previousPageButton;
        [SerializeField] private Button _nextPageButton;
        [SerializeField] private GameObject _loadingIndicator;

        private readonly List<int> _filtered = new List<int>();
        private PaintingCardView[] _cards;
        private IInjectObjectFactory _objectFactory;
        private IContentLoader _contentLoader;
        private IReferenceCatalog _catalog;
        private GradingConfig _gradingConfig;
        private IProgressStorage _progressStorage;
        private MatchLaunchContext _matchContext;
        private ISceneLoader _sceneLoader;
        private IAdvertisingService _advertising;
        private int _pageIndex;
        private int _refreshId;

        [Inject]
        public void Construct(
            IInjectObjectFactory objectFactory,
            IContentLoader contentLoader,
            IReferenceCatalog catalog,
            GradingConfig gradingConfig,
            IProgressStorage progressStorage,
            MatchLaunchContext matchContext,
            ISceneLoader sceneLoader,
            IAdvertisingService advertising)
        {
            if (catalog == null)
            {
                throw new ArgumentNullException(nameof(catalog));
            }

            if (gradingConfig == null)
            {
                throw new ArgumentNullException(nameof(gradingConfig));
            }

            _objectFactory = objectFactory ?? throw new ArgumentNullException(nameof(objectFactory));
            _contentLoader = contentLoader ?? throw new ArgumentNullException(nameof(contentLoader));
            _catalog = catalog;
            _gradingConfig = gradingConfig;
            _progressStorage = progressStorage ?? throw new ArgumentNullException(nameof(progressStorage));
            _matchContext = matchContext ?? throw new ArgumentNullException(nameof(matchContext));
            _sceneLoader = sceneLoader ?? throw new ArgumentNullException(nameof(sceneLoader));
            _advertising = advertising ?? throw new ArgumentNullException(nameof(advertising));
            RefreshIfReady();
        }

        private void OnEnable()
        {
            _previousPageButton.onClick.AddListener(OnPreviousPage);
            _nextPageButton.onClick.AddListener(OnNextPage);
            _pageIndex = 0;
            LoadThenRefresh().Forget();
        }

        private async UniTaskVoid LoadThenRefresh()
        {
            SetLoading(true);

            try
            {
                await _contentLoader.LoadAsync(null, destroyCancellationToken);
            }
            catch (Exception)
            {
            }

            if (this == null)
            {
                return;
            }

            await BuildFilterAsync();

            if (this == null)
            {
                return;
            }

            RefreshIfReady();
        }

        private async UniTask BuildFilterAsync()
        {
            _filtered.Clear();
            int total = _catalog.Count;

            for (int i = 0; i < total; i++)
            {
                ReferenceConfig reference;

                try
                {
                    reference = await _catalog.GetAtAsync(i, destroyCancellationToken);
                }
                catch (Exception)
                {
                    return;
                }

                if (this == null)
                {
                    return;
                }

                if (reference != null && reference.Category == _matchContext.Category)
                {
                    _filtered.Add(i);
                }
            }
        }

        private void SetLoading(bool value)
        {
            if (_loadingIndicator != null)
            {
                _loadingIndicator.SetActive(value);
            }
        }

        private void OnDisable()
        {
            _previousPageButton.onClick.RemoveListener(OnPreviousPage);
            _nextPageButton.onClick.RemoveListener(OnNextPage);
            _refreshId++;
        }

        private void EnsureCards()
        {
            if (_cards != null)
            {
                return;
            }

            _cards = new PaintingCardView[PageSize];

            for (int i = 0; i < PageSize; i++)
            {
                _cards[i] = _objectFactory.Create(_cardPrefab, _cardsContainer);
                _cards[i].Clicked += OnCardClicked;
            }
        }

        private void RefreshIfReady()
        {
            if (isActiveAndEnabled == false)
            {
                return;
            }

            EnsureCards();
            RefreshAsync().Forget();
        }

        private async UniTaskVoid RefreshAsync()
        {
            int refreshId = ++_refreshId;
            int count = _filtered.Count;

            if (count == 0)
            {
                SetLoading(false);
                return;
            }

            int pageCount = Mathf.Max(1, (count + _cards.Length - 1) / _cards.Length);
            _pageIndex = Mathf.Clamp(_pageIndex, 0, pageCount - 1);
            int firstIndex = _pageIndex * _cards.Length;

            _previousPageButton.gameObject.SetActive(_pageIndex > 0);
            _nextPageButton.gameObject.SetActive(firstIndex + _cards.Length < count);

            for (int i = 0; i < _cards.Length; i++)
            {
                _cards[i].ShowEmpty();
            }

            SetLoading(true);

            ReferenceConfig[] loaded = new ReferenceConfig[_cards.Length];

            for (int i = 0; i < _cards.Length; i++)
            {
                int position = firstIndex + i;

                if (position >= count)
                {
                    continue;
                }

                try
                {
                    loaded[i] = await _catalog.GetAtAsync(_filtered[position], destroyCancellationToken);
                }
                catch (Exception)
                {
                    return;
                }

                if (this == null || refreshId != _refreshId)
                {
                    return;
                }
            }

            SetLoading(false);

            for (int i = 0; i < _cards.Length; i++)
            {
                if (loaded[i] == null)
                {
                    continue;
                }

                _cards[i].Show(loaded[i], ResolveGradeLabel(loaded[i]));
                _cards[i].PlayAppear(i * CardAppearDelayStep);
            }
        }

        private string ResolveGradeLabel(ReferenceConfig reference)
        {
            if (_progressStorage.TryGetBestScore(reference.Id, _matchContext.GameMode, out float score) == false)
            {
                return null;
            }

            return _gradingConfig.Resolve(score).Label;
        }

        private void OnCardClicked(ReferenceConfig reference)
        {
            _advertising.ShowInterstitial();
            _matchContext.SelectReference(reference);
            _sceneLoader.LoadAsync(SceneId.Gameplay).Forget();
        }

        private void OnPreviousPage()
        {
            _pageIndex--;
            RefreshAsync().Forget();
        }

        private void OnNextPage()
        {
            _pageIndex++;
            RefreshAsync().Forget();
        }
    }
}
