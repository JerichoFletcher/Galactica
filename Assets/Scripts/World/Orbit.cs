using UnityEngine;

namespace Galactica.World {
    public class Orbit {
        public float SemiMajorAxis { get; set; }
        public float SemiMinorAxis { get; set; }
        public float TiltAngle { get; set; }

        public Orbit(float semiMajor, float semiMinor, float tiltAngle) {
            SemiMajorAxis = semiMajor;
            SemiMinorAxis = semiMinor;
            TiltAngle = tiltAngle;
        }

        public static Orbit Of(float semiMinor, float eccentricity, float tiltAngle) {
            float semiMajor = Mathf.Sqrt(semiMinor * semiMinor / (1f - eccentricity * eccentricity));
            return new Orbit(semiMajor, semiMinor, tiltAngle);
        }

        public Vector2 Get(float angle) {
            float
                sinAngle = Mathf.Sin(angle - TiltAngle),
                cosAngle = Mathf.Cos(angle - TiltAngle),
                sinTilt = Mathf.Sin(TiltAngle),
                cosTilt = Mathf.Cos(TiltAngle);

            return new Vector2(
                SemiMajorAxis * cosAngle * cosTilt - SemiMinorAxis * sinAngle * sinTilt,
                SemiMajorAxis * cosAngle * sinTilt + SemiMinorAxis * sinAngle * cosTilt
            );
        }

        public float SqrRadiusAt(float angle) {
            return Get(angle).sqrMagnitude;
        }

        public float RadiusAt(float angle) {
            return Mathf.Sqrt(SqrRadiusAt(angle));
        }

        public override string ToString() {
            return $"Orbit of {SemiMinorAxis} to {SemiMajorAxis} tilted {TiltAngle}";
        }
    }
}
