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

            // Rigidbody2D 설정
            rb.mass = data.mass;
            rb.linearDamping = 0.5f;
            rb.angularDamping = 0.5f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.sleepMode = RigidbodySleepMode2D.NeverSleep;
            rb.freezeRotation = false;

            // CircleCollider2D 설정
            col.radius = data.radius;

            // SpriteRenderer 설정
            if (data.sprite != null)
                sr.sprite = data.sprite;

            // 스케일을 반지름 기준으로 조정 (radius=1 기준 sprite 가정)
            float diameter = data.radius * 2f;
            transform.localScale = new Vector3(diameter, diameter, 1f);
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

            // 수박(11단계)은 FruitManager에서 별도 처리
            if (Level >= 11)
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
