using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SuikaGame.Core
{
    public enum GameState
    {
        Ready,
        Playing,
        GameOver,
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        private GameState state = GameState.Ready;

        public GameState State => state;
        public bool IsPlaying => state == GameState.Playing;

        public event Action<GameState> OnStateChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            SetState(GameState.Playing);
        }

        public void GameOver()
        {
            if (state == GameState.GameOver)
                return;

            ScoreManager.Instance?.SaveBestScore();
            SetState(GameState.GameOver);
        }

        public void Restart()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private void SetState(GameState next)
        {
            state = next;
            OnStateChanged?.Invoke(state);
        }
    }
}
