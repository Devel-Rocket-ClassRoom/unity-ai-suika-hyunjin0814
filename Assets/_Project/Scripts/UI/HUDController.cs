using SuikaGame.Core;
using TMPro;
using UnityEngine;

namespace SuikaGame.UI
{
    public class HUDController : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI scoreText;

        private void OnEnable()
        {
            if (ScoreManager.Instance != null)
                ScoreManager.Instance.OnScoreChanged += UpdateScore;
        }

        private void OnDisable()
        {
            if (ScoreManager.Instance != null)
                ScoreManager.Instance.OnScoreChanged -= UpdateScore;
        }

        private void Start()
        {
            UpdateScore(ScoreManager.Instance != null ? ScoreManager.Instance.CurrentScore : 0);
        }

        private void UpdateScore(int score)
        {
            if (scoreText != null)
                scoreText.text = score.ToString("N0");
        }
    }
}
