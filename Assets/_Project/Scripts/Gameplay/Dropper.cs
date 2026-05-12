using System.Collections;
using SuikaGame.Core;
using SuikaGame.Data;
using UnityEngine;
using UnityEngine.UI;

namespace SuikaGame.Gameplay
{
    public class Dropper : MonoBehaviour
    {
        [SerializeField]
        private FruitData[] droppableFruits; // 1~5단계만

        [SerializeField]
        private float dropCooldown = 0.5f;

        [SerializeField]
        private float boxHalfWidth = 2.9f;

        [SerializeField]
        private Transform dropPoint;

        [Header("Preview")]
        [SerializeField]
        private SpriteRenderer currentFruitRenderer; // 드로퍼에 붙은 현재 과일 표시

        [SerializeField]
        private Image nextFruitImage; // NextFruitUIContainer/Frame의 UI Image

        private FruitData currentFruit;
        private FruitData nextFruit;

        private bool canDrop = true;

        private void Start()
        {
            currentFruit = GetRandomFruit();
            nextFruit = GetRandomFruit();
            RefreshPreview();
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

            currentFruit = nextFruit;
            nextFruit = GetRandomFruit();
            RefreshPreview();

            yield return new WaitForSeconds(dropCooldown);
            canDrop = true;
        }

        // 현재·다음 과일 스프라이트 갱신
        private void RefreshPreview()
        {
            if (currentFruitRenderer != null)
            {
                currentFruitRenderer.sprite = currentFruit?.sprite;
                if (currentFruit != null)
                {
                    float d = currentFruit.radius * 2f;
                    currentFruitRenderer.transform.localScale = new Vector3(d, d, 1f);
                }
            }

            if (nextFruitImage != null)
            {
                nextFruitImage.sprite = nextFruit?.sprite;
                nextFruitImage.preserveAspect = true;
            }
        }

        private FruitData GetRandomFruit()
        {
            if (droppableFruits == null || droppableFruits.Length == 0)
                return null;

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
