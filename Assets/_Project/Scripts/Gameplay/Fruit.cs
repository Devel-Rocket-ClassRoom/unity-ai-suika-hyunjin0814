using SuikaGame.Data;
using UnityEngine;

namespace SuikaGame.Gameplay
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(CircleCollider2D))]
    [RequireComponent(typeof(SpriteRenderer))]
    public class Fruit : MonoBehaviour
    {
        [SerializeField]
        private FruitData data;

        private Rigidbody2D rb;
        private CircleCollider2D col;
        private SpriteRenderer sr;

        public int Level => data != null ? data.level : 0;
        public FruitData Data => data;

        // 머지 중복 방지 플래그
        public bool merged = false;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            col = GetComponent<CircleCollider2D>();
            sr = GetComponent<SpriteRenderer>();
        }

        public void Initialize(FruitData fruitData)
        {
            data = fruitData;

            // 컴포넌트 참조 재확인 (동적 생성 시 Awake 시점에 없을 수 있음)
            if (rb == null)
                rb = GetComponent<Rigidbody2D>();
            if (col == null)
                col = GetComponent<CircleCollider2D>();
            if (sr == null)
                sr = GetComponent<SpriteRenderer>();

            // 중복된 CircleCollider2D 제거 (Radius 0.5 문제 해결)
            var colliders = GetComponents<CircleCollider2D>();
            if (colliders.Length > 1)
            {
                for (int i = 1; i < colliders.Length; i++)
                {
                    Destroy(colliders[i]);
                }
            }
            col = colliders[0];

            // Rigidbody2D 설정
            rb.mass = data.mass;
            rb.linearDamping = 0.5f;
            rb.angularDamping = 0.5f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.sleepMode = RigidbodySleepMode2D.NeverSleep;
            rb.freezeRotation = false;

            // CircleCollider2D 설정
            // 시각적 반지름(visualRadius)이 있으면 해당 값으로 콜라이더를 설정하고,
            // 전체 스케일을 조절하여 실제 물리 반지름(data.radius)을 맞춥니다.
            float vRadius = data.visualRadius > 0 ? data.visualRadius : 0.5f;
            col.radius = vRadius;

            // SpriteRenderer 설정
            if (data.sprite != null)
                sr.sprite = data.sprite;

            // 스케일 계산: 물리 반지름 / 시각적 반지름
            float scaleValue = data.radius / vRadius;
            transform.localScale = new Vector3(scaleValue, scaleValue, 1f);
        }

        public bool IsResting() => rb.linearVelocity.magnitude < 0.1f;

        private void OnCollisionEnter2D(Collision2D col)
        {
            if (merged)
                return;

            var other = col.gameObject.GetComponent<Fruit>();
            if (other == null || other.merged)
                return;

            if (other.Level != Level)
                return;

            // InstanceID 비교로 한쪽만 머지 실행
            if (GetInstanceID() < other.GetInstanceID())
            {
                merged = true;
                other.merged = true;
                FruitManager.Instance.Merge(this, other);
            }
        }
    }
}
