using System.Collections.Generic;
using System.Threading;
using _Project.Application.Configs;
using _Project.Application.Factories;
using _Project.Application.Localization;
using _Project.Application.Network;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

namespace _Project.Presentation.Menu
{
    public class ServerSelectView : MonoBehaviour
    {
        private const string LobbyNameKey = "region.lobby";

        [SerializeField] private Transform _rowsRoot;
        [SerializeField] private ServerRegionRowView _rowPrefab;

        private ServerRegionsConfig _regions;
        private IServerPingService _ping;
        private SelectedRegion _selected;
        private IInjectObjectFactory _factory;
        private ILocalizationService _localization;
        private IClientConnection _connection;

        private readonly List<ServerRegionRowView> _rows = new List<ServerRegionRowView>();
        private CancellationTokenSource _cts;
        private bool _built;

        [Inject]
        public void Construct(ServerRegionsConfig regions, IServerPingService ping, SelectedRegion selected, IInjectObjectFactory factory, ILocalizationService localization, IClientConnection connection)
        {
            _regions = regions;
            _ping = ping;
            _selected = selected;
            _factory = factory;
            _localization = localization;
            _connection = connection;
        }

        private void OnEnable()
        {
            Build();

            _cts = new CancellationTokenSource();
            PingAll(_cts.Token).Forget();
        }

        private void OnDisable()
        {
            if (_cts != null)
            {
                _cts.Cancel();
                _cts.Dispose();
                _cts = null;
            }
        }

        private void Build()
        {
            if (_built)
            {
                return;
            }

            if (_rowPrefab == null || _rowsRoot == null)
            {
                return;
            }

            string lobbyName = _localization != null ? _localization.Get(LobbyNameKey) : "Лобби";
            IReadOnlyList<ServerRegion> regions = _regions.Regions;

            for (int i = 0; i < regions.Count; i++)
            {
                ServerRegionRowView row = _factory.Create(_rowPrefab, _rowsRoot);
                row.Bind(regions[i], lobbyName + " " + (i + 1), OnRowClicked);
                _rows.Add(row);
            }

            _built = true;

            if (_selected.Current == null && _rows.Count > 0)
            {
                Select(_rows[0].Region, false);
            }
            else
            {
                RefreshSelection();
            }
        }

        private async UniTaskVoid PingAll(CancellationToken cancellationToken)
        {
            foreach (ServerRegionRowView row in _rows)
            {
                int ms = await _ping.PingAsync(row.Region, cancellationToken);

                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                row.SetPing(ms);
            }
        }

        private void OnRowClicked(ServerRegion region)
        {
            Select(region, true);
        }

        private void Select(ServerRegion region, bool byUser)
        {
            if (byUser && _selected.Current != null && _selected.Current != region)
            {
                _connection.Disconnect();
            }

            _selected.Set(region);
            RefreshSelection();
        }

        private void RefreshSelection()
        {
            foreach (ServerRegionRowView row in _rows)
            {
                row.SetSelected(_selected.Current != null && row.Region == _selected.Current);
            }
        }
    }
}
