using UnityEngine;
using CoinPusher.Core;

namespace CoinPusher.Gameplay
{
    public class CoinDropController : MonoBehaviour
    {
        [SerializeField] GameConfig config;
        [SerializeField] GameObject coinPrefab;
        [SerializeField] KeyCode dropKey = KeyCode.Space;

        Vector3 _basePosition;
        float _offsetX;

        void Start()
        {
            _basePosition = transform.position;
        }

        void Update()
        {
            float range = config.dropHorizontalRange;
            float input = Input.GetAxisRaw("Horizontal");
            _offsetX = Mathf.Clamp(_offsetX + input * config.dropMoveSpeed * Time.deltaTime, -range, range);

            transform.position = new Vector3(_basePosition.x + _offsetX, config.dropHeight, _basePosition.z);

            if (Input.GetKeyDown(dropKey) || Input.GetMouseButtonDown(0))
            {
                DropCoin();
            }
        }

        void DropCoin()
        {
            if (coinPrefab == null) return;
            var coinObj = Instantiate(coinPrefab, transform.position, Quaternion.identity);
            coinObj.GetComponent<Coin>()?.Configure(config);
        }
    }
}
