using System;
using _Project.Core.Match;

namespace _Project.Application.Network
{
    public interface IMatchmakingService
    {
        event Action<MatchInfo> MatchFound;
        event Action<string> RoomCreated;
        event Action JoinRoomFailed;
        event Action SelfPlayRejected;

        bool IsSearching { get; }
        bool HasMatch { get; }
        bool IsRoomMatch { get; }
        bool LeftByLocalPlayer { get; }
        bool IsRoundActive { get; }
        MatchInfo CurrentMatch { get; }

        void JoinQueue();
        void CreateRoom(GameMode gameMode, string paintingId);
        void JoinRoom(string code);
        void Cancel();
        void LeaveMatch();
    }
}
