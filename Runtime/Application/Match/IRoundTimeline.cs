using System;

namespace _Project.Application.Match
{
    public interface IRoundTimeline
    {
        event Action<int> IntroTicked;
        event Action IntroFinished;
        event Action<float, float> Ticked;
        event Action Finished;
        event Action Aborted;
    }
}
