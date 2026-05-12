using SuikaGame.Core;
using TMPro;
using UnityEngine;

namespace SuikaGame.UI
{
    public class HUDController : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI scoreText;

        private void Start()
        {
            // Start()는 모든 Awake() 완료 후 호출되므로 싱글턴 안전하게 참조 가능
            if (ScoreManager.Instance != null)
                ScoreManager.Instance.OnScoreChanged += UpdateScore;

            UpdateScore(ScoreManager.Instance != null ? ScoreManager.Instance.CurrentScore : 0);
        }

        private void OnDestroy()
        {
            if (ScoreManager.Instance != null)
                ScoreManager.Instance.OnScoreChanged -= UpdateScore;
        }

        private void UpdateScore(int score)
        {
            if (scoreText != null)
                scoreText.text = score.ToString("N0");
        }
    }
}
