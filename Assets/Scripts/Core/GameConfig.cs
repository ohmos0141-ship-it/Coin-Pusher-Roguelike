using UnityEngine;

namespace CoinPusher.Core
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "Coin Pusher/Game Config")]
    public class GameConfig : ScriptableObject
    {
        [Header("Coin")]
        public float coinMass = 1f;
        [Range(0f, 1f)] public float coinFriction = 0.4f;
        public int coinBaseScore = 10;

        [Header("Drop")]
        public float dropHeight = 2f;
        public float dropHorizontalRange = 2.5f;
        public float dropMoveSpeed = 3f;

        [Header("Pusher")]
        public float pusherSpeed = 1.5f;
        public float pusherDistance = 1.2f;
        public float pusherPauseDuration = 0.4f;
    }
}
