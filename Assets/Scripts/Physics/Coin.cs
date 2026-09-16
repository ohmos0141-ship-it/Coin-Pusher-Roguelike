using UnityEngine;
using CoinPusher.Core;

namespace CoinPusher.Gameplay
{
    [RequireComponent(typeof(Rigidbody))]
    public class Coin : MonoBehaviour
    {
        public int BaseScore { get; private set; } = 10;
        public bool Consumed { get; private set; }

        public void Configure(GameConfig config)
        {
            BaseScore = config.coinBaseScore;
            GetComponent<Rigidbody>().mass = config.coinMass;
        }

        public void MarkConsumed() => Consumed = true;
    }
}
