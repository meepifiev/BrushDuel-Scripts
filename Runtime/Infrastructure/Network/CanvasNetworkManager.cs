using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using _Project.Application.AssetManagement;
using _Project.Application.Configs;
using _Project.Application.Match;
using _Project.Application.Network;
using _Project.Core.Match;
using _Project.Infrastructure.AssetManagement;
using Cysharp.Threading.Tasks;
using kcp2k;
using Mirror;
using UnityEngine;

namespace _Project.Infrastructure.Network
{
    public class CanvasNetworkManager : NetworkManager
    {
        private readonly ServerMatchmaking _matchmaking = new ServerMatchmaking();
        private readonly ServerMatchRegistry _matches = new ServerMatchRegistry();
        private readonly RuntimeReferenceCatalog _runtimeCatalog = new RuntimeReferenceCatalog();

        [SerializeField] private ServerConfig _serverConfig;
        [SerializeField] private KcpTransport _transport;
        [SerializeField] private ScoringConfig _scoringConfig;
        [SerializeField] private bool _autoStartServer;
        [SerializeField] private ServerConfig _editorServerConfig;

        private const float ReservationTtlSeconds = 20f;
        private const int TicketLifetimeSeconds = 120;
        private const int ConnectionTimeoutMs = 30000;
        private const float ContentCheckIntervalSeconds = 5f;
        private const string ReloadMarkerName = ".reload";

        private readonly SessionReservationStore _reservations = new SessionReservationStore();
        private readonly UdpEchoService _pingEcho = new UdpEchoService();

        private MatchScorer _scorer;
        private int _nextPaintingIndex;
        private ServerRole _role = ServerRole.All;
        private string _ticketSecret;
        private List<string> _sessions = new List<string>();
        private int _nextSessionIndex;
        private float _contentCheckTimer;
        private long _lastReloadMarkerTicks;

        public event Action ClientConnected;
        public event Action ClientDisconnected;

        public override void Awake()
        {
            _role = ServerLaunchArgs.ResolveRole(ServerRole.All);
            _ticketSecret = ServerLaunchArgs.ResolveTicketSecret();
            _sessions = ServerLaunchArgs.ResolveSessions();

            ServerConfig config = ResolveConfig();
            networkAddress = config.Address;
            _transport.Port = config.ServerPort;

            if (ServerLaunchArgs.TryResolvePort(out ushort listenPort))
            {
                _transport.Port = listenPort;
            }

            _transport.Timeout = ConnectionTimeoutMs;
            base.Awake();
        }

        public void ApplyClientEndpoint(string address, ushort port)
        {
            networkAddress = address;
            _transport.Port = port;
        }

        public void StartClientAtSession(string sessionPort)
        {
            if (ushort.TryParse(sessionPort, out ushort port) == false)
            {
                Debug.LogError(nameof(CanvasNetworkManager) + ": invalid session port '" + sessionPort + "'");
                return;
            }

            _transport.Port = port;
            StartClient();
        }

        public override void Start()
        {
            base.Start();

            if (UnityEngine.Application.isBatchMode || _autoStartServer)
            {
                StartServerWhenContentReady().Forget();
            }
        }

        private async UniTaskVoid StartServerWhenContentReady()
        {
            try
            {
                FileReferenceContentLoader loader = new FileReferenceContentLoader(_runtimeCatalog);
                await loader.LoadAsync(null, CancellationToken.None);
                ServerLog.Info("content loaded: " + _runtimeCatalog.Count + " reference(s)");
            }
            catch (Exception exception)
            {
                ServerLog.Error("content load failed: " + exception.Message);
            }

            _lastReloadMarkerTicks = ReloadMarkerTicks();
            StartServer();
        }

        private void CheckContentReload()
        {
            long ticks = ReloadMarkerTicks();

            if (ticks > _lastReloadMarkerTicks)
            {
                _lastReloadMarkerTicks = ticks;
                ReloadContent();
            }
        }

        private void ReloadContent()
        {
            try
            {
                new FileReferenceContentLoader(_runtimeCatalog).LoadAsync(null, CancellationToken.None).Forget();
                ServerLog.Info("content reloaded: " + _runtimeCatalog.Count + " reference(s)");
            }
            catch (Exception exception)
            {
                ServerLog.Error("content reload failed: " + exception.Message);
            }
        }

        private static long ReloadMarkerTicks()
        {
            try
            {
                string marker = Path.Combine(ServerReferencePaths.Folder, ReloadMarkerName);
                return File.Exists(marker) ? File.GetLastWriteTimeUtc(marker).Ticks : 0L;
            }
            catch (Exception)
            {
                return 0L;
            }
        }

        public override void Update()
        {
            base.Update();

            if (NetworkServer.active == false)
            {
                return;
            }

            _matches.Tick(Time.deltaTime);

            if (_reservations.Count > 0)
            {
                CleanupExpiredReservations();
            }

            _contentCheckTimer += Time.unscaledDeltaTime;

            if (_contentCheckTimer >= ContentCheckIntervalSeconds)
            {
                _contentCheckTimer = 0f;
                CheckContentReload();
            }
        }

        public override void OnStartServer()
        {
            base.OnStartServer();

            if (_role != ServerRole.Session)
            {
                NetworkServer.RegisterHandler<JoinQueueMessage>(OnJoinQueue);
                NetworkServer.RegisterHandler<LeaveQueueMessage>(OnLeaveQueue);
                NetworkServer.RegisterHandler<CreateRoomMessage>(OnCreateRoom);
                NetworkServer.RegisterHandler<JoinRoomMessage>(OnJoinRoom);
            }

            if (_role != ServerRole.Hub)
            {
                NetworkServer.RegisterHandler<RedeemMatchMessage>(OnRedeemMatch);
                NetworkServer.RegisterHandler<LeaveMatchMessage>(OnLeaveMatch);
                NetworkServer.RegisterHandler<PlayerReadyMessage>(OnPlayerReady);
                NetworkServer.RegisterHandler<PlayerDoneMessage>(OnPlayerDone);
                NetworkServer.RegisterHandler<StrokeBeganMessage>(OnStrokeBegan);
                NetworkServer.RegisterHandler<StrokeContinuedMessage>(OnStrokeContinued);
                NetworkServer.RegisterHandler<StrokeEndedMessage>(OnStrokeEnded);
                NetworkServer.RegisterHandler<FillAppliedMessage>(OnFillApplied);
                NetworkServer.RegisterHandler<UndoAppliedMessage>(OnUndoApplied);
                _matches.RoundStarted += OnMatchRoundStarted;
                _matches.RoundFinished += OnMatchRoundFinished;
                _matches.MatchCanceled += OnMatchCanceled;
            }

            try
            {
                _pingEcho.Start(_transport.Port + 1);
            }
            catch (Exception exception)
            {
                ServerLog.Error("ping echo failed on port " + (_transport.Port + 1) + ": " + exception.Message);
            }

            ServerLog.Info("server role: " + _role + " listen port: " + _transport.Port);
        }

        public override void OnStopServer()
        {
            _pingEcho.Stop();
            _matches.RoundStarted -= OnMatchRoundStarted;
            _matches.RoundFinished -= OnMatchRoundFinished;
            _matches.MatchCanceled -= OnMatchCanceled;
            _matchmaking.Clear();
            _matches.Clear();
            _reservations.Clear();
            base.OnStopServer();
        }

        public override void OnClientConnect()
        {
            base.OnClientConnect();
            ClientConnected?.Invoke();
        }

        public override void OnClientDisconnect()
        {
            base.OnClientDisconnect();
            ClientDisconnected?.Invoke();
        }

        public override void OnServerConnect(NetworkConnectionToClient conn)
        {
            base.OnServerConnect(conn);
            ServerLog.Info("connected: " + ServerLog.Describe(conn) + " (online " + NetworkServer.connections.Count + ")");
        }

        public override void OnServerDisconnect(NetworkConnectionToClient conn)
        {
            _matchmaking.Forget(conn);
            _matches.RemovePlayer(conn);
            ForgetReservation(conn);
            base.OnServerDisconnect(conn);
            ServerLog.Info("disconnected: " + ServerLog.Describe(conn) + " (online " + NetworkServer.connections.Count + ")");
        }

        private void StartMatch(ServerMatchPair pair)
        {
            if (_role == ServerRole.Hub)
            {
                AssignToSession(pair);
                return;
            }

            StartLocalMatch(pair);
        }

        private void StartLocalMatch(ServerMatchPair pair)
        {
            ReferenceConfig reference = ResolveReference(pair.PaintingId);
            _matches.CreateMatch(pair, pair.GameMode, reference, reference.RoundDurationSeconds, ResolveConfig().ReadyTimeoutSeconds);

            pair.First.Connection.Send(new MatchFoundMessage
            {
                PaintingId = reference.Id,
                GameMode = (int)pair.GameMode,
                OpponentNickname = pair.Second.Nickname,
            });

            pair.Second.Connection.Send(new MatchFoundMessage
            {
                PaintingId = reference.Id,
                GameMode = (int)pair.GameMode,
                OpponentNickname = pair.First.Nickname,
            });

            ServerLog.Info("MATCH START: " + ServerLog.Describe(pair.First) + " vs " + ServerLog.Describe(pair.Second) + " | painting=" + reference.Id + " mode=" + pair.GameMode);
        }

        private void AssignToSession(ServerMatchPair pair)
        {
            if (_sessions.Count == 0)
            {
                ServerLog.Error("hub has no session ports (BRUSHDUEL_SESSIONS); canceling match");
                SendIfConnected(pair.First, new MatchCanceledMessage());
                SendIfConnected(pair.Second, new MatchCanceledMessage());
                return;
            }

            ReferenceConfig reference = ResolveReference(pair.PaintingId);
            string session = _sessions[_nextSessionIndex % _sessions.Count];
            _nextSessionIndex++;

            string matchId = Guid.NewGuid().ToString("N");
            long expires = NowUnix() + TicketLifetimeSeconds;

            SendIfConnected(pair.First, BuildAssignment(session, reference, pair.GameMode, matchId, 0, pair.First, pair.Second.Nickname, expires));
            SendIfConnected(pair.Second, BuildAssignment(session, reference, pair.GameMode, matchId, 1, pair.Second, pair.First.Nickname, expires));

            ServerLog.Info("MATCH ASSIGNED: " + ServerLog.Describe(pair.First) + " vs " + ServerLog.Describe(pair.Second) + " -> session " + session + " match=" + matchId + " painting=" + reference.Id);
        }

        private MatchAssignedMessage BuildAssignment(string session, ReferenceConfig reference, GameMode gameMode, string matchId, int slot, ServerPlayer player, string opponentNickname, long expires)
        {
            MatchTicket ticket = new MatchTicket
            {
                MatchId = matchId,
                Slot = slot,
                PlayerId = player.PlayerId,
                Nickname = player.Nickname,
                PaintingId = reference.Id,
                GameMode = (int)gameMode,
                RoundDurationSeconds = reference.RoundDurationSeconds,
                ExpiresUnix = expires,
            };

            return new MatchAssignedMessage
            {
                SessionPort = session,
                Token = MatchTicketCodec.Encode(ticket, _ticketSecret),
                PaintingId = reference.Id,
                GameMode = (int)gameMode,
                OpponentNickname = opponentNickname,
                RoundDurationSeconds = reference.RoundDurationSeconds,
            };
        }

        private void OnRedeemMatch(NetworkConnectionToClient conn, RedeemMatchMessage message)
        {
            if (MatchTicketCodec.TryDecode(message.Token, _ticketSecret, out MatchTicket ticket) == false)
            {
                ServerLog.Info("redeem rejected (bad token): " + ServerLog.Describe(conn));
                SendIfConnected(conn, new RedeemRejectedMessage());
                return;
            }

            if (ticket.ExpiresUnix < NowUnix())
            {
                ServerLog.Info("redeem rejected (expired): " + ServerLog.Describe(conn) + " match=" + ticket.MatchId);
                SendIfConnected(conn, new RedeemRejectedMessage());
                return;
            }

            MatchReservation reservation = _reservations.GetOrCreate(ticket.MatchId, Time.realtimeSinceStartup);
            reservation.PaintingId = ticket.PaintingId;
            reservation.GameMode = ticket.GameMode;
            reservation.RoundDurationSeconds = ticket.RoundDurationSeconds;

            ReservedPlayer redeemed = new ReservedPlayer { PlayerId = ticket.PlayerId, Nickname = ticket.Nickname, Connection = conn };

            if (ticket.Slot == 0)
            {
                reservation.First = redeemed;
            }
            else
            {
                reservation.Second = redeemed;
            }

            ServerLog.Info("redeem: " + ServerLog.Describe(conn) + " match=" + ticket.MatchId + " slot=" + ticket.Slot);

            if (reservation.BothRedeemed)
            {
                PromoteReservation(reservation);
            }
        }

        private void PromoteReservation(MatchReservation reservation)
        {
            _reservations.Remove(reservation.MatchId);

            ServerPlayer first = new ServerPlayer(reservation.First.Connection, reservation.First.Nickname, reservation.First.PlayerId);
            ServerPlayer second = new ServerPlayer(reservation.Second.Connection, reservation.Second.Nickname, reservation.Second.PlayerId);
            StartLocalMatch(new ServerMatchPair(first, second, (GameMode)reservation.GameMode, reservation.PaintingId));
        }

        private void ForgetReservation(NetworkConnectionToClient conn)
        {
            MatchReservation reservation = _reservations.FindByConnection(conn);

            if (reservation == null)
            {
                return;
            }

            _reservations.Remove(reservation.MatchId);
            ReservedPlayer other = reservation.First != null && reservation.First.Connection == conn ? reservation.Second : reservation.First;
            NotifyReservationCanceled(other);
        }

        private void CleanupExpiredReservations()
        {
            List<MatchReservation> expired = _reservations.DrainExpired(Time.realtimeSinceStartup, ReservationTtlSeconds);

            for (int i = 0; i < expired.Count; i++)
            {
                MatchReservation reservation = expired[i];
                NotifyReservationCanceled(reservation.First);
                NotifyReservationCanceled(reservation.Second);
                ServerLog.Info("reservation expired: match=" + reservation.MatchId);
            }
        }

        private void NotifyReservationCanceled(ReservedPlayer player)
        {
            if (player == null || player.Connection == null)
            {
                return;
            }

            SendIfConnected(player.Connection, new MatchCanceledMessage());
        }

        private static long NowUnix()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }

        private void SendIfConnected<T>(ServerPlayer player, T message) where T : struct, NetworkMessage
        {
            SendIfConnected(player.Connection, message);
        }

        private void SendIfConnected<T>(NetworkConnectionToClient connection, T message) where T : struct, NetworkMessage
        {
            if (connection == null || NetworkServer.connections.ContainsKey(connection.connectionId) == false)
            {
                return;
            }

            connection.Send(message);
        }

        private void RelayToOpponent<T>(NetworkConnectionToClient sender, T message) where T : struct, NetworkMessage
        {
            SendIfConnected(_matches.FindOpponent(sender), message);
        }

        private ReferenceConfig ResolveReference(string paintingId)
        {
            if (string.IsNullOrEmpty(paintingId) == false && _runtimeCatalog.TryGetById(paintingId, out ReferenceConfig chosen))
            {
                return chosen;
            }

            return PickReference();
        }

        private ReferenceConfig PickReference()
        {
            if (_runtimeCatalog.Count == 0)
            {
                throw new InvalidOperationException(nameof(_runtimeCatalog));
            }

            ReferenceConfig reference = _runtimeCatalog.GetAt(_nextPaintingIndex % _runtimeCatalog.Count);
            _nextPaintingIndex++;
            return reference;
        }

        private ServerConfig ResolveConfig()
        {
#if UNITY_EDITOR
            if (_editorServerConfig != null)
            {
                return _editorServerConfig;
            }
#endif
            return _serverConfig;
        }

        private void OnJoinQueue(NetworkConnectionToClient conn, JoinQueueMessage message)
        {
            if (_matches.Contains(conn))
            {
                return;
            }

            ServerMatchPair? pair = _matchmaking.EnqueueForQuickMatch(conn, message.Nickname, message.PlayerId);

            if (pair != null)
            {
                StartMatch(pair.Value);
                return;
            }

            ServerLog.Info("quick-search: " + ServerLog.Describe(conn) + " '" + message.Nickname + "' queued, waiting for opponent");
        }

        private void OnLeaveQueue(NetworkConnectionToClient conn, LeaveQueueMessage message)
        {
            _matchmaking.RemovePlayer(conn);
            _matches.RemovePlayer(conn);
        }

        private void OnCreateRoom(NetworkConnectionToClient conn, CreateRoomMessage message)
        {
            if (_matches.Contains(conn))
            {
                return;
            }

            string code = _matchmaking.CreateRoom(conn, message.Nickname, message.PlayerId, (GameMode)message.GameMode, message.PaintingId);
            conn.Send(new RoomCreatedMessage { Code = code });
            ServerLog.Info("room created: code " + code + " by " + ServerLog.Describe(conn) + " '" + message.Nickname + "' mode=" + (GameMode)message.GameMode + " painting=" + (string.IsNullOrEmpty(message.PaintingId) ? "(server-pick)" : message.PaintingId));
        }

        private void OnLeaveMatch(NetworkConnectionToClient conn, LeaveMatchMessage message)
        {
            ServerLog.Info("leave match: " + ServerLog.Describe(conn));
            _matchmaking.RemovePlayer(conn);
            _matches.RemovePlayer(conn);
        }

        private void OnJoinRoom(NetworkConnectionToClient conn, JoinRoomMessage message)
        {
            if (_matches.Contains(conn))
            {
                return;
            }

            if (IsSelfPlay(conn, message.PlayerId))
            {
                ServerLog.Info("room join rejected (self-play): " + ServerLog.Describe(conn) + " code " + message.Code);
                conn.Send(new SelfPlayRejectedMessage());
                return;
            }

            if (_matchmaking.TryJoinRoom(message.Code, conn, message.Nickname, message.PlayerId, out ServerMatchPair pair))
            {
                StartMatch(pair);
                return;
            }

            ServerLog.Info("room join failed: " + ServerLog.Describe(conn) + " code " + message.Code + " not found");
            conn.Send(new JoinRoomFailedMessage());
        }

        private bool IsSelfPlay(NetworkConnectionToClient conn, string playerId)
        {
            if (string.IsNullOrEmpty(playerId))
            {
                return false;
            }

            return _matchmaking.HasPlayer(playerId, conn) || _matches.HasPlayer(playerId, conn);
        }

        private void OnPlayerReady(NetworkConnectionToClient conn, PlayerReadyMessage message)
        {
            ServerLog.Info("ready: " + ServerLog.Describe(conn));
            _matches.MarkReady(conn);
        }

        private void OnPlayerDone(NetworkConnectionToClient conn, PlayerDoneMessage message)
        {
            ServerLog.Info("done: " + ServerLog.Describe(conn));
            _matches.MarkDone(conn);
        }

        private void OnStrokeBegan(NetworkConnectionToClient conn, StrokeBeganMessage message)
        {
            _matches.FindMatch(conn)?.RecorderFor(conn)?.ApplyStrokeBegan(message);
            RelayToOpponent(conn, message);
        }

        private void OnStrokeContinued(NetworkConnectionToClient conn, StrokeContinuedMessage message)
        {
            _matches.FindMatch(conn)?.RecorderFor(conn)?.ApplyStrokeContinued(message);
            RelayToOpponent(conn, message);
        }

        private void OnStrokeEnded(NetworkConnectionToClient conn, StrokeEndedMessage message)
        {
            _matches.FindMatch(conn)?.RecorderFor(conn)?.ApplyStrokeEnded();
            RelayToOpponent(conn, message);
        }

        private void OnFillApplied(NetworkConnectionToClient conn, FillAppliedMessage message)
        {
            _matches.FindMatch(conn)?.RecorderFor(conn)?.ApplyFill(message);
            RelayToOpponent(conn, message);
        }

        private void OnUndoApplied(NetworkConnectionToClient conn, UndoAppliedMessage message)
        {
            _matches.FindMatch(conn)?.RecorderFor(conn)?.ApplyUndo();
            RelayToOpponent(conn, message);
        }

        private void OnMatchRoundStarted(ServerMatch match)
        {
            RoundStartMessage start = new RoundStartMessage { DurationSeconds = match.RoundDurationSeconds };
            SendIfConnected(match.First, start);
            SendIfConnected(match.Second, start);
            ServerLog.Info("ROUND START: " + ServerLog.Describe(match.First) + " vs " + ServerLog.Describe(match.Second) + " | duration=" + match.RoundDurationSeconds + "s");
        }

        private void OnMatchRoundFinished(ServerMatch match)
        {
            ScoreAndReport(match).Forget();
        }

        private async UniTaskVoid ScoreAndReport(ServerMatch match)
        {
            int firstScore = 0;
            int secondScore = 0;

            await UniTask.RunOnThreadPool(() =>
            {
                firstScore = ScorePercent(match, match.FirstRecorder);
                secondScore = ScorePercent(match, match.SecondRecorder);
            });

            await UniTask.SwitchToMainThread();

            SendIfConnected(match.First, new RoundResultMessage
            {
                MyScore = firstScore,
                OpponentScore = secondScore,
                Outcome = (int)ResolveOutcome(firstScore, secondScore),
            });

            SendIfConnected(match.Second, new RoundResultMessage
            {
                MyScore = secondScore,
                OpponentScore = firstScore,
                Outcome = (int)ResolveOutcome(secondScore, firstScore),
            });

            ServerLog.Info("ROUND END: " + ServerLog.Describe(match.First) + "=" + firstScore + "% vs " + ServerLog.Describe(match.Second) + "=" + secondScore + "% -> " + DescribeWinner(match, firstScore, secondScore));
        }

        private string DescribeWinner(ServerMatch match, int firstScore, int secondScore)
        {
            if (firstScore > secondScore)
            {
                return "WIN " + ServerLog.Describe(match.First);
            }

            if (secondScore > firstScore)
            {
                return "WIN " + ServerLog.Describe(match.Second);
            }

            return "DRAW";
        }

        private int ScorePercent(ServerMatch match, ServerDrawingRecorder recorder)
        {
            try
            {
                float total = ResolveScorer().Score(match.Reference, match.GameMode, recorder.Document);
                return Mathf.RoundToInt(Mathf.Clamp01(total) * 100f);
            }
            catch (Exception exception)
            {
                ServerLog.Error("scoring failed: " + exception);
                return 0;
            }
        }

        private MatchOutcome ResolveOutcome(int myScore, int opponentScore)
        {
            if (myScore > opponentScore)
            {
                return MatchOutcome.Win;
            }

            if (myScore < opponentScore)
            {
                return MatchOutcome.Lose;
            }

            return MatchOutcome.Draw;
        }

        private MatchScorer ResolveScorer()
        {
            if (_scorer == null)
            {
                _scorer = new MatchScorer(_scoringConfig);
            }

            return _scorer;
        }

        private void OnMatchCanceled(ServerMatch match)
        {
            MatchCanceledMessage canceled = new MatchCanceledMessage();
            SendIfConnected(match.First, canceled);
            SendIfConnected(match.Second, canceled);
            ServerLog.Info("MATCH CANCELED: " + ServerLog.Describe(match.First) + " vs " + ServerLog.Describe(match.Second));
        }
    }
}
