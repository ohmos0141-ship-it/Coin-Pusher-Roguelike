using UnityEngine;
using CoinPusher.Core;

namespace CoinPusher.Gameplay
{
    public class FallDetector : MonoBehaviour
    {
        [SerializeField] float recycleDelay = 0f;

        void OnTriggerEnter(Collider other)
        {
            var coin = other.GetComponentInParent<Coin>();
            if (coin == null || coin.Consumed) return;

            coin.MarkConsumed();
            GameManager.Instance.Score.AddScore(coin.BaseScore);
            Destroy(coin.gameObject, recycleDelay);
        }
    }
}
