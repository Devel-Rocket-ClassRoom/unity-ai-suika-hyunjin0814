using System.Collections;
using SuikaGame.Core;
using SuikaGame.Data;
using UnityEngine;

namespace SuikaGame.Gameplay
{
    public class Dropper : MonoBehaviour
    {
        [SerializeField]
        private FruitData[] droppableFruits; // 1~5단계만

        [SerializeField]
        private float dropCooldown = 0.5f;

        [SerializeField]
        private float boxHalfWidth = 2.9f; // 박스 내부 좌우 한계 (벽 두께 고려)

        [SerializeField]
        private Transform dropPoint; // 드롭 위치 (드로퍼 하단)

        private FruitData currentFruit;
        private FruitData nextFruit;

        private bool canDrop = true;

        private void Start()
        {
            currentFruit = GetRandomFruit();
            nextFruit = GetRandomFruit();
        }

        private void Update()
        {
            if (!GameManager.Instance.IsPlaying)
                return;

            HandleMove();
            HandleDrop();
        }

        private void HandleMove()
        {
            // 마우스 X를 월드 좌표로 변환
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            float clampedX = Mathf.Clamp(mouseWorld.x, -boxHalfWidth, boxHalfWidth);
            transform.position = new Vector3(clampedX, transform.position.y, 0f);
        }

        private void HandleDrop()
        {
            if (!canDrop)
                return;

            if (Input.GetMouseButtonDown(0))
                StartCoroutine(CoDrop());
        }

        private IEnumerator CoDrop()
        {
            canDrop = false;

            Vector3 spawnPos = dropPoint != null ? dropPoint.position : transform.position;
            FruitManager.Instance.SpawnFruit(currentFruit, spawnPos);

            // 큐 전환
            currentFruit = nextFruit;
            nextFruit = GetRandomFruit();

            yield return new WaitForSeconds(dropCooldown);
            canDrop = true;
        }

        private FruitData GetRandomFruit()
        {
            if (droppableFruits == null || droppableFruits.Length == 0)
                return null;

            // 낮은 레벨에 높은 가중치 부여
            // 가중치: level 1→5, level 2→4, level 3→3, level 4→2, level 5→1
            int totalWeight = 0;
            foreach (var d in droppableFruits)
                totalWeight += Mathf.Max(1, 6 - d.level);

            int rand = Random.Range(0, totalWeight);
            int cumulative = 0;
            foreach (var d in droppableFruits)
            {
                cumulative += Mathf.Max(1, 6 - d.level);
                if (rand < cumulative)
                    return d;
            }

            return droppableFruits[0];
        }

        public FruitData CurrentFruit => currentFruit;
        public FruitData NextFruit => nextFruit;
    }
}
