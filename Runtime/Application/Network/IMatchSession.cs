using System;

namespace _Project.Application.Network
{
    public interface IMatchSession
    {
        event Action<float> RoundStarted;
        event Action RoundFinished;
        event Action MatchCanceled;

        bool IsReady { get; }
        MatchResult LastResult { get; }

        void SendReady();
        void SendDone();
    }
}
