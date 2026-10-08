using System;
using _Project.Core.Match;

namespace _Project.Application.Progress
{
    public interface IProgressStorage
    {
        event Action CoinsChanged;

        bool TryGetBestScore(string referenceId, GameMode mode, out float score);
        void SetBestScore(string referenceId, GameMode mode, float score);
        int GetCoins();
        void AddCoins(int amount);
        bool TrySpendCoins(int amount);
        void Save();
    }
}
