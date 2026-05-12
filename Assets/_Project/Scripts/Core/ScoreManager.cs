using System;
using UnityEngine;

namespace SuikaGame.Core
{
    public class ScoreManager : MonoBehaviour
    {
        public static ScoreManager Instance { get; private set; }

        private int currentScore;
        private int bestScore;

        public int CurrentScore => currentScore;
        public int BestScore => bestScore;

        public event Action<int> OnScoreChanged;

        private const string BestScoreKey = "BestScore";

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            bestScore = PlayerPrefs.GetInt(BestScoreKey, 0);
        }

        public void Add(int level)
        {
            int points = level * level * 2;
            currentScore += points;
            OnScoreChanged?.Invoke(currentScore);
        }

        public void SaveBestScore()
        {
            if (currentScore > bestScore)
            {
                bestScore = currentScore;
                PlayerPrefs.SetInt(BestScoreKey, bestScore);
                PlayerPrefs.Save();
            }
        }

        public void Reset()
        {
            currentScore = 0;
            OnScoreChanged?.Invoke(currentScore);
        }
    }
}
