using System;

namespace CoinPusher.Core
{
    public class ScoreSystem
    {
        public int CurrentScore { get; private set; }

        public event Action<int> OnScoreChanged;
        public event Action<int> OnScoreAdded;

        public void AddScore(int amount)
        {
            if (amount == 0) return;
            CurrentScore += amount;
            OnScoreAdded?.Invoke(amount);
            OnScoreChanged?.Invoke(CurrentScore);
        }

        public void Reset()
        {
            CurrentScore = 0;
            OnScoreChanged?.Invoke(CurrentScore);
        }
    }
}
