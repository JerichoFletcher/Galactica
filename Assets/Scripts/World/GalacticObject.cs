using UnityEngine;

namespace Galactica.World {
    public abstract class GalacticObject : MonoBehaviour {
        public abstract float Mass { get; }
        public abstract float Size { get; set; }
        public Orbit Orbit { get; set; }
        public float OrbitAnglePos {
            get {
                return orbitAnglePos;
            }
            set {
                orbitAnglePos = value;
                transform.position = Orbit.Get(value);
            }
        }
        public float DistanceFromGC => (transform.position - GalaxyManager.Instance.transform.position).magnitude;

        private float orbitAnglePos;
    }
}
