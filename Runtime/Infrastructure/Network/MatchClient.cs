using System;
using System.Threading;
using _Project.Application.Network;
using _Project.Application.Profile;
using _Project.Core.Match;
using Cysharp.Threading.Tasks;
using Mirror;

namespace _Project.Infrastructure.Network
{
    public class MatchClient : IMatchmakingService, IMatchSession, IDisposable
    {
        private readonly IPlayerProfileService _profile;
        private readonly IClientConnection _connection;

        public MatchClient(IPlayerProfileService profile, IClientConnection connection)
        {
            _profile = profile ?? throw new ArgumentNullException(nameof(profile));
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _connection.Disconnected += OnConnectionLost;
        }

        public event Action<MatchInfo> MatchFound;
        public event Action<string> RoomCreated;
        public event Action JoinRoomFailed;
        public event Action SelfPlayRejected;
        public event Action<float> RoundStarted;
        public event Action RoundFinished;
        public event Action MatchCanceled;

        public bool IsSearching { get; private set; }
        public bool HasMatch { get; private set; }
        public bool IsRoomMatch { get; private set; }
        public bool IsReady { get; private set; }
        public bool LeftByLocalPlayer { get; private set; }
        public bool IsRoundActive { get; private set; }
        public MatchInfo CurrentMatch { get; private set; }
        public MatchResult LastResult { get; private set; }

        public void JoinQueue()
        {
            BeginSearch(false);
            NetworkClient.Send(new JoinQueueMessage { Nickname = _profile.Nickname, PlayerId = _profile.PlayerId });
        }

        public void CreateRoom(GameMode gameMode, string paintingId)
        {
            BeginSearch(true);
            NetworkClient.Send(new CreateRoomMessage { Nickname = _profile.Nickname, PlayerId = _profile.PlayerId, GameMode = (int)gameMode, PaintingId = paintingId });
        }

        public void JoinRoom(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentNullException(nameof(code));
            }

            BeginSearch(true);
            NetworkClient.Send(new JoinRoomMessage { Code = code.Trim().ToUpperInvariant(), Nickname = _profile.Nickname, PlayerId = _profile.PlayerId });
        }

        public void Cancel()
        {
            IsSearching = false;
            HasMatch = false;
            IsReady = false;
            IsRoundActive = false;

            if (NetworkClient.isConnected == false)
            {
                return;
            }

            NetworkClient.Send(new LeaveQueueMessage());
        }

        public void LeaveMatch()
        {
            IsSearching = false;
            HasMatch = false;
            IsReady = false;
            IsRoundActive = false;
            LeftByLocalPlayer = true;

            if (NetworkClient.isConnected == false)
            {
                return;
            }

            NetworkClient.Send(new LeaveMatchMessage());
        }

        public void SendReady()
        {
            RequireConnection();
            RegisterHandlers();
            IsReady = true;
            NetworkClient.Send(new PlayerReadyMessage());
        }

        public void SendDone()
        {
            RequireConnection();
            NetworkClient.Send(new PlayerDoneMessage());
        }

        public void Dispose()
        {
            _connection.Disconnected -= OnConnectionLost;
        }

        private void BeginSearch(bool roomFlow)
        {
            RequireConnection();
            RegisterHandlers();
            IsSearching = true;
            HasMatch = false;
            IsReady = false;
            IsRoundActive = false;
            IsRoomMatch = roomFlow;
            LeftByLocalPlayer = false;
        }

        private void RequireConnection()
        {
            if (NetworkClient.isConnected == false)
            {
                throw new InvalidOperationException(nameof(NetworkClient));
            }
        }

        private void RegisterHandlers()
        {
            NetworkClient.ReplaceHandler<MatchAssignedMessage>(OnMatchAssigned);
            NetworkClient.ReplaceHandler<RedeemRejectedMessage>(OnRedeemRejected);
            NetworkClient.ReplaceHandler<MatchFoundMessage>(OnMatchFound);
            NetworkClient.ReplaceHandler<RoomCreatedMessage>(OnRoomCreated);
            NetworkClient.ReplaceHandler<JoinRoomFailedMessage>(OnJoinRoomFailed);
            NetworkClient.ReplaceHandler<SelfPlayRejectedMessage>(OnSelfPlayRejected);
            NetworkClient.ReplaceHandler<RoundStartMessage>(OnRoundStart);
            NetworkClient.ReplaceHandler<RoundResultMessage>(OnRoundResult);
            NetworkClient.ReplaceHandler<MatchCanceledMessage>(OnMatchCanceled);
        }

        private void OnMatchFound(MatchFoundMessage message)
        {
            if (IsSearching == false)
            {
                return;
            }

            IsSearching = false;
            HasMatch = true;
            CurrentMatch = new MatchInfo(message.PaintingId, (GameMode)message.GameMode, message.OpponentNickname);

            if (IsRoomMatch)
            {
                SendReady();
            }

            MatchFound?.Invoke(CurrentMatch);
        }

        private void OnMatchAssigned(MatchAssignedMessage message)
        {
            if (IsSearching == false)
            {
                return;
            }

            CurrentMatch = new MatchInfo(message.PaintingId, (GameMode)message.GameMode, message.OpponentNickname);
            RedirectToSession(message).Forget();
        }

        private async UniTaskVoid RedirectToSession(MatchAssignedMessage message)
        {
            await UniTask.Yield();

            bool connected;

            try
            {
                connected = await _connection.ConnectToSessionAsync(message.SessionPort, CancellationToken.None);
            }
            catch (Exception)
            {
                connected = false;
            }

            if (connected == false)
            {
                IsSearching = false;
                HasMatch = false;
                MatchCanceled?.Invoke();
                return;
            }

            if (IsSearching == false)
            {
                return;
            }

            RegisterHandlers();
            NetworkClient.Send(new RedeemMatchMessage { Token = message.Token });
        }

        private void OnRedeemRejected(RedeemRejectedMessage message)
        {
            IsSearching = false;
            HasMatch = false;
            IsReady = false;
            MatchCanceled?.Invoke();
        }

        private void OnRoomCreated(RoomCreatedMessage message)
        {
            RoomCreated?.Invoke(message.Code);
        }

        private void OnJoinRoomFailed(JoinRoomFailedMessage message)
        {
            IsSearching = false;
            JoinRoomFailed?.Invoke();
        }

        private void OnSelfPlayRejected(SelfPlayRejectedMessage message)
        {
            IsSearching = false;
            HasMatch = false;
            SelfPlayRejected?.Invoke();
        }

        private void OnRoundStart(RoundStartMessage message)
        {
            IsRoundActive = true;
            RoundStarted?.Invoke(message.DurationSeconds);
        }

        private void OnRoundResult(RoundResultMessage message)
        {
            HasMatch = false;
            IsReady = false;
            IsRoundActive = false;
            LastResult = new MatchResult(message.MyScore, message.OpponentScore, (MatchOutcome)message.Outcome);
            RoundFinished?.Invoke();
        }

        private void OnMatchCanceled(MatchCanceledMessage message)
        {
            IsSearching = false;
            HasMatch = false;
            IsRoundActive = false;
            MatchCanceled?.Invoke();
            IsReady = false;
        }

        private void OnConnectionLost()
        {
            IsSearching = false;
            HasMatch = false;
            IsReady = false;
            IsRoundActive = false;
        }
    }
}
