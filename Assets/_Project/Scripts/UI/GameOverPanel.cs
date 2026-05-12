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
            if (panel != null)
                panel.SetActive(false);

            if (restartButton != null)
                restartButton.onClick.AddListener(OnRestartClicked);
        }

        private void Start()
        {
            // Start()는 모든 Awake() 완료 후 호출되므로 싱글턴 안전하게 참조 가능
            if (GameManager.Instance != null)
                GameManager.Instance.OnStateChanged += OnStateChanged;
        }

        private void OnDestroy()
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
