using SuikaGame.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SuikaGame.UI
{
    public class GameOverPanel : MonoBehaviour
    {
        [SerializeField]
        private GameObject panel;

        [SerializeField]
        private TextMeshProUGUI finalScoreText;

        [SerializeField]
        private Button restartButton;

        private void Awake()
        {
            panel.SetActive(false);
            restartButton.onClick.AddListener(OnRestartClicked);
        }

        private void OnEnable()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnStateChanged += OnStateChanged;
        }

        private void OnDisable()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnStateChanged -= OnStateChanged;
        }

        private void OnStateChanged(GameState state)
        {
            if (state != GameState.GameOver)
                return;

            int score = ScoreManager.Instance != null ? ScoreManager.Instance.CurrentScore : 0;
            if (finalScoreText != null)
                finalScoreText.text = score.ToString("N0");

            panel.SetActive(true);
        }

        private void OnRestartClicked()
        {
            GameManager.Instance?.Restart();
        }
    }
}
