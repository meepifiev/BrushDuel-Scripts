using System;
using _Project.Application.Configs;
using _Project.Application.Progress;

namespace _Project.Application.Match
{
    public class RoundRewardService
    {
        private readonly MatchLaunchContext _context;
        private readonly ReferenceConfig _reference;
        private readonly IProgressStorage _storage;
        private readonly GradingConfig _gradingConfig;

        public RoundRewardService(
            MatchLaunchContext context,
            ReferenceConfig reference,
            IProgressStorage storage,
            GradingConfig gradingConfig)
        {
            if (reference == null)
            {
                throw new ArgumentNullException(nameof(reference));
            }

            if (gradingConfig == null)
            {
                throw new ArgumentNullException(nameof(gradingConfig));
            }

            _context = context ?? throw new ArgumentNullException(nameof(context));
            _reference = reference;
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            _gradingConfig = gradingConfig;
        }

        public RoundReward Claim(float score)
        {
            bool hasBest = _storage.TryGetBestScore(_reference.Id, _context.GameMode, out float bestScore);
            int earnedCoins = CountCoins(score);
            int bestCoins = hasBest ? CountCoins(bestScore) : 0;
            int coins = Math.Max(0, earnedCoins - bestCoins);
            bool improved = hasBest == false || score > bestScore;

            if (improved)
            {
                _storage.SetBestScore(_reference.Id, _context.GameMode, score);
            }

            if (coins > 0)
            {
                _storage.AddCoins(coins);
            }

            if (improved || coins > 0)
            {
                _storage.Save();
            }

            return new RoundReward(coins, _storage.GetCoins());
        }

        private int CountCoins(float score)
        {
            return (int)MathF.Round(score * _gradingConfig.MaxCoins);
        }
    }
}
