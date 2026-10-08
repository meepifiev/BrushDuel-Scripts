using System;
using _Project.Application.Configs;
using _Project.Application.Progress;
using _Project.Core.Match;
using CodeStage.AntiCheat.ObscuredTypes;

namespace _Project.Infrastructure.Steam
{
    public class FileProgressStorage : IProgressStorage
    {
        private const string CoinsKey = "coins";

        private readonly LocalSaveFile _save;
        private readonly EconomyConfig _economy;

        private ObscuredInt _coins;
        private bool _coinsLoaded;

        public FileProgressStorage(LocalSaveFile save, EconomyConfig economy)
        {
            _save = save ?? throw new ArgumentNullException(nameof(save));

            if (economy == null)
            {
                throw new ArgumentNullException(nameof(economy));
            }

            _economy = economy;
        }

        public event Action CoinsChanged;

        public bool TryGetBestScore(string referenceId, GameMode mode, out float score)
        {
            string key = ScoreKey(referenceId, mode);

            if (_save.HasKey(key) == false)
            {
                score = 0f;
                return false;
            }

            score = _save.GetFloat(key, 0f);
            return true;
        }

        public void SetBestScore(string referenceId, GameMode mode, float score)
        {
            _save.SetFloat(ScoreKey(referenceId, mode), score);
        }

        public int GetCoins()
        {
            EnsureCoinsLoaded();
            return _coins;
        }

        public void AddCoins(int amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount));
            }

            EnsureCoinsLoaded();
            _coins += amount;
            _save.SetInt(CoinsKey, _coins);
            CoinsChanged?.Invoke();
        }

        public bool TrySpendCoins(int amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount));
            }

            EnsureCoinsLoaded();

            if (_coins < amount)
            {
                return false;
            }

            _coins -= amount;
            _save.SetInt(CoinsKey, _coins);
            _save.Save();
            CoinsChanged?.Invoke();
            return true;
        }

        private void EnsureCoinsLoaded()
        {
            if (_coinsLoaded)
            {
                return;
            }

            _coins = _save.GetInt(CoinsKey, _economy.StartingCoins);
            _coinsLoaded = true;
        }

        public void Save()
        {
            _save.Save();
        }

        private static string ScoreKey(string referenceId, GameMode mode)
        {
            return "score." + referenceId + "." + (int)mode;
        }
    }
}
