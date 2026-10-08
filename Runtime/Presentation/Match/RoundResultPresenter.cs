using System;
using _Project.Application.Configs;
using _Project.Application.Match;
using _Project.Presentation.Drawing;
using UnityEngine;
using VContainer.Unity;

namespace _Project.Presentation.Match
{
    public class RoundResultPresenter : IStartable, IDisposable
    {
        private const float PercentMultiplier = 100f;

        private readonly RoundHandler _roundHandler;
        private readonly SimilarityHandler _similarityHandler;
        private readonly ResultPopupView _resultPopupView;
        private readonly ResultCoinBarView _coinBarView;
        private readonly GradingConfig _gradingConfig;
        private readonly RoundRewardService _rewardService;
        private readonly ReferenceConfig _referenceConfig;
        private readonly MatchLaunchContext _context;
        private readonly RoundAnalysisController _analysis;

        public RoundResultPresenter(
            RoundHandler roundHandler,
            SimilarityHandler similarityHandler,
            ResultPopupView resultPopupView,
            ResultCoinBarView coinBarView,
            GradingConfig gradingConfig,
            RoundRewardService rewardService,
            ReferenceConfig referenceConfig,
            MatchLaunchContext context,
            RoundAnalysisController analysis)
        {
            if (gradingConfig == null)
            {
                throw new ArgumentNullException(nameof(gradingConfig));
            }

            if (referenceConfig == null)
            {
                throw new ArgumentNullException(nameof(referenceConfig));
            }

            _roundHandler = roundHandler ?? throw new ArgumentNullException(nameof(roundHandler));
            _similarityHandler = similarityHandler ?? throw new ArgumentNullException(nameof(similarityHandler));
            _resultPopupView = resultPopupView != null ? resultPopupView : throw new ArgumentNullException(nameof(resultPopupView));
            _coinBarView = coinBarView != null ? coinBarView : throw new ArgumentNullException(nameof(coinBarView));
            _gradingConfig = gradingConfig;
            _rewardService = rewardService ?? throw new ArgumentNullException(nameof(rewardService));
            _referenceConfig = referenceConfig;
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _analysis = analysis ?? throw new ArgumentNullException(nameof(analysis));
        }

        public void Start()
        {
            _resultPopupView.HideInstant();
            _analysis.Completed += OnRoundFinished;
        }

        public void Dispose()
        {
            _analysis.Completed -= OnRoundFinished;
        }

        private void OnRoundFinished()
        {
            float similarity = _similarityHandler.CurrentScore.Total;
            GradingConfig.GradeEntry grade = _gradingConfig.Resolve(similarity);
            RoundReward reward = _rewardService.Claim(similarity);
            int percent = Mathf.RoundToInt(similarity * PercentMultiplier);
            _coinBarView.Prepare(reward.WalletTotal - reward.Coins, reward.WalletTotal);
            _resultPopupView.ShowResults(grade, similarity, _roundHandler.ElapsedSeconds, reward.Coins);
        }
    }
}
