using UnityEngine;

namespace SuikaGame.Data
{
    [CreateAssetMenu(fileName = "FruitData", menuName = "SuikaGame/FruitData")]
    public class FruitData : ScriptableObject
    {
        [SerializeField]
        public int level;

        [SerializeField]
        public string displayName;

        [SerializeField]
        public Sprite sprite;

        [SerializeField]
        public GameObject prefab;

        [SerializeField]
        public float radius;

        [SerializeField]
        public float visualRadius;

        [SerializeField]
        public int score;

        [SerializeField]
        public float mass;

        //[SerializeField]
        //public AudioClip mergeSfx;
    }
}
