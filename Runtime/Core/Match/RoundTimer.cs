using System;

namespace _Project.Core.Match
{
    public class RoundTimer
    {
        public RoundTimer(float durationSeconds)
        {
            if (durationSeconds <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(durationSeconds));
            }

            Duration = durationSeconds;
            Remaining = durationSeconds;
        }

        public float Duration { get; }
        public float Remaining { get; private set; }
        public bool IsFinished => Remaining <= 0f;

        public void Tick(float deltaSeconds)
        {
            if (IsFinished)
            {
                return;
            }

            Remaining = Math.Max(0f, Remaining - deltaSeconds);
        }
    }
}
