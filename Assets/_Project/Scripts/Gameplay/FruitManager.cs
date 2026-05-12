using System.Collections.Generic;
using SuikaGame.Data;
using UnityEngine;

namespace SuikaGame.Gameplay
{
    public class FruitManager : MonoBehaviour
    {
        public static FruitManager Instance { get; private set; }

        [SerializeField]
        private FruitData[] fruitDataList;

        private readonly List<Fruit> allFruits = new();

        public IReadOnlyList<Fruit> AllFruits => allFruits;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void Register(Fruit fruit) => allFruits.Add(fruit);

        public void Unregister(Fruit fruit) => allFruits.Remove(fruit);

        public void Merge(Fruit a, Fruit b)
        {
            int nextLevel = a.Level + 1;
            Vector3 midPoint = (a.transform.position + b.transform.position) * 0.5f;

            Unregister(a);
            Unregister(b);
            Destroy(a.gameObject);
            Destroy(b.gameObject);

            // 수박(11단계) 초과 시 보너스만 지급
            if (nextLevel > 11)
            {
                Core.ScoreManager.Instance?.Add(11);
                return;
            }

            if (fruitDataList == null || nextLevel - 1 >= fruitDataList.Length)
                return;

            var nextData = fruitDataList[nextLevel - 1];
            SpawnFruit(nextData, midPoint);
            Core.ScoreManager.Instance?.Add(nextLevel);
        }

        public Fruit SpawnFruit(FruitData data, Vector3 position)
        {
            var go = new GameObject($"Fruit_{data.displayName}");
            go.transform.position = position;

            var fruit = go.AddComponent<Fruit>();
            go.AddComponent<Rigidbody2D>();
            go.AddComponent<CircleCollider2D>();
            go.AddComponent<SpriteRenderer>();

            fruit.Initialize(data);
            Register(fruit);
            return fruit;
        }
    }
}
