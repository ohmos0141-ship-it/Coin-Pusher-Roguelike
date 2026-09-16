using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using CoinPusher.Core;

namespace CoinPusher.UI
{
    public class ScoreUI : MonoBehaviour
    {
        [SerializeField] Text scoreText;
        [SerializeField] Text popupText;
        [SerializeField] float popupDuration = 0.6f;

        Coroutine _popupRoutine;

        void Start()
        {
            if (GameManager.Instance == null) return;

            GameManager.Instance.Score.OnScoreChanged += HandleScoreChanged;
            GameManager.Instance.Score.OnScoreAdded += HandleScoreAdded;
            HandleScoreChanged(GameManager.Instance.Score.CurrentScore);
        }

        void OnDisable()
        {
            if (GameManager.Instance == null) return;

            GameManager.Instance.Score.OnScoreChanged -= HandleScoreChanged;
            GameManager.Instance.Score.OnScoreAdded -= HandleScoreAdded;
        }

        void HandleScoreChanged(int score)
        {
            if (scoreText != null) scoreText.text = score.ToString("D6");
        }

        void HandleScoreAdded(int amount)
        {
            if (popupText == null) return;
            if (_popupRoutine != null) StopCoroutine(_popupRoutine);
            _popupRoutine = StartCoroutine(ShowPopup(amount));
        }

        IEnumerator ShowPopup(int amount)
        {
            popupText.text = "+" + amount;
            popupText.enabled = true;
            yield return new WaitForSeconds(popupDuration);
            popupText.enabled = false;
        }
    }
}
