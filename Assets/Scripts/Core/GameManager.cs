using UnityEngine;

namespace CoinPusher.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] GameConfig config;
        public GameConfig Config => config;

        public ScoreSystem Score { get; private set; } = new ScoreSystem();

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }
    }
}
