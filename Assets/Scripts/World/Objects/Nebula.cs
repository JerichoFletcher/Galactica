using UnityEngine;

namespace Galactica.World.Objects {
    [RequireComponent(typeof(SpriteRenderer))]
    public class Nebula : GalacticObject {
        [SerializeField] private Gradient colorVariation;
        [SerializeField] private Vector2 nebulaSizeBounds;
        [SerializeField] private float nebulaDensity;

        public override float Mass => 1000f * nebulaDensity * Size * Size * Size;
        public override float Size {
            get {
                return size;
            }
            set {
                size = value;
                transform.localScale = Vector3.one * Mathf.Lerp(nebulaSizeBounds.x, nebulaSizeBounds.y, size);
            }
        }

        private new SpriteRenderer renderer;

        private float size;
        private Color nebulaColor;

        private void Awake() {
            renderer = GetComponent<SpriteRenderer>();
        }

        private void Start() {
            nebulaColor = colorVariation.Evaluate(Random.value);
            renderer.color = nebulaColor;
        }

        public void DisplayAttributes() {
            Debug.Log($"size: {size}, distance from GC: {DistanceFromGC}");
        }
    }
}
