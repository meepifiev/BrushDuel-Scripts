using System;
using System.Collections.Generic;
using System.Threading;
using _Project.Application.AssetManagement;
using _Project.Application.Configs;
using _Project.Application.Factories;
using _Project.Application.Match;
using _Project.Application.Network;
using _Project.Presentation.Popups;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace _Project.Presentation.Menu
{
    public class RoomPaintingListView : MonoBehaviour
    {
        private const int PageSize = 4;
        private const float CardAppearDelayStep = 0.08f;

        [SerializeField] private PaintingCardView _cardPrefab;
        [SerializeField] private RectTransform _cardsContainer;
        [SerializeField] private Button _previousPageButton;
        [SerializeField] private Button _nextPageButton;
        [SerializeField] private PopupNavigator _navigator;
        [SerializeField] private PopupView _roomCreatedPopup;
        [SerializeField] private PopupView _errorPopup;
        [SerializeField] private GameObject _loadingIndicator;

        private readonly List<int> _filtered = new List<int>();
        private PaintingCardView[] _cards;
        private IInjectObjectFactory _objectFactory;
        private IContentLoader _contentLoader;
        private IReferenceCatalog _catalog;
        private MatchLaunchContext _matchContext;
        private IClientConnection _connection;
        private IMatchmakingService _matchmaking;
        private int _pageIndex;
        private int _refreshId;
        private bool _busy;
        private CancellationTokenSource _flowCancellation;

        [Inject]
        public void Construct(
            IInjectObjectFactory objectFactory,
            IContentLoader contentLoader,
            IReferenceCatalog catalog,
            MatchLaunchContext matchContext,
            IClientConnection connection,
            IMatchmakingService matchmaking)
        {
            if (catalog == null)
            {
                throw new ArgumentNullException(nameof(catalog));
            }

            _objectFactory = objectFactory ?? throw new ArgumentNullException(nameof(objectFactory));
            _contentLoader = contentLoader ?? throw new ArgumentNullException(nameof(contentLoader));
            _catalog = catalog;
            _matchContext = matchContext ?? throw new ArgumentNullException(nameof(matchContext));
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _matchmaking = matchmaking ?? throw new ArgumentNullException(nameof(matchmaking));
            RefreshIfReady();
        }

        private void OnEnable()
        {
            _previousPageButton.onClick.AddListener(OnPreviousPage);
            _nextPageButton.onClick.AddListener(OnNextPage);
            _pageIndex = 0;
            _busy = false;
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

        private void OnDestroy()
        {
            CancelFlow();
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

                _cards[i].Show(loaded[i], null);
                _cards[i].PlayAppear(i * CardAppearDelayStep);
            }
        }

        private void OnCardClicked(ReferenceConfig reference)
        {
            if (_busy)
            {
                return;
            }

            _busy = true;
            _flowCancellation = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);
            CreateRoomFlow(reference, _flowCancellation.Token).Forget();
        }

        private async UniTaskVoid CreateRoomFlow(ReferenceConfig reference, CancellationToken cancellationToken)
        {
            _navigator.Replace(_roomCreatedPopup);

            bool connected;

            try
            {
                connected = await _connection.ConnectAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                _busy = false;
                return;
            }

            if (cancellationToken.IsCancellationRequested)
            {
                _busy = false;
                return;
            }

            if (connected == false)
            {
                _navigator.Replace(_errorPopup);
                _busy = false;
                return;
            }

            _matchmaking.CreateRoom(_matchContext.GameMode, reference.Id);
            _busy = false;
        }

        private void CancelFlow()
        {
            if (_flowCancellation == null)
            {
                return;
            }

            _flowCancellation.Cancel();
            _flowCancellation.Dispose();
            _flowCancellation = null;
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
