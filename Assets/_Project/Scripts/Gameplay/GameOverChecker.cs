using System.Collections.Generic;
using SuikaGame.Core;
using UnityEngine;

namespace SuikaGame.Gameplay
{
    public class GameOverChecker : MonoBehaviour
    {
        [SerializeField]
        private float lineY = 3.5f; // RedLine Y 좌표

        [SerializeField]
        private float graceTime = 2f; // 초과 유지 허용 시간

        private readonly Dictionary<Fruit, float> overTimers = new();

        private void Update()
        {
            if (!GameManager.Instance.IsPlaying)
                return;

            var fruits = FruitManager.Instance.AllFruits;

            // 이미 제거된 과일 타이머 정리
            var toRemove = new List<Fruit>();
            foreach (var key in overTimers.Keys)
                if (key == null)
                    toRemove.Add(key);
            foreach (var key in toRemove)
                overTimers.Remove(key);

            foreach (var fruit in fruits)
            {
                if (fruit == null)
                    continue;

                if (fruit.transform.position.y > lineY && fruit.IsResting())
                {
                    if (!overTimers.ContainsKey(fruit))
                        overTimers[fruit] = 0f;

                    overTimers[fruit] += Time.deltaTime;

                    if (overTimers[fruit] >= graceTime)
                    {
                        GameManager.Instance.GameOver();
                        return;
                    }
                }
                else
                {
                    overTimers.Remove(fruit);
                }
            }
        }
    }
}
