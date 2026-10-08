namespace _Project.Application.Match
{
    public readonly struct RoundReward
    {
        public RoundReward(int coins, int walletTotal)
        {
            Coins = coins;
            WalletTotal = walletTotal;
        }

        public int Coins { get; }
        public int WalletTotal { get; }
    }
}
