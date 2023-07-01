using UnityEngine;

namespace Galactica.World.Objects {
    [RequireComponent(typeof(SpriteRenderer))]
    public class Star : GalacticObject {
        [SerializeField] private Gradient colorGradient;
        [SerializeField] private Vector2 starSizeBounds;
        [SerializeField] private float starDensity;

        public override float Mass => 1000f * starDensity * Size * Size * Size;
        public override float Size {
            get {
                return size;
            }
            set {
                size = value;
                transform.localScale = Vector3.one * Mathf.Lerp(starSizeBounds.x, starSizeBounds.y, size);
            }
        }
        public float Temperature {
            get {
                return temperature;
            }
            set {
                temperature = value;
                surfaceColor = colorGradient.Evaluate(temperature);
                renderer.color = surfaceColor;
            }
        }

        private new SpriteRenderer renderer;

        private float size, temperature;
        private Color surfaceColor;

        private void Awake() {
            renderer = GetComponent<SpriteRenderer>();
        }

        public void DisplayAttributes() {
            Debug.Log($"size: {size}, temperature: {temperature}, distance from GC: {DistanceFromGC}");
        }
    }
}
