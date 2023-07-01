using UnityEngine;

namespace Galactica.World {
    public static class OrbitSolver {
        public static class Gizmos {
            private const int numDrawPoints = 100;
            private const float drawAngleStepRad = 360f / numDrawPoints * Mathf.Deg2Rad;

            public static void DrawOrbit(float semiMinor, float eccentricity, float tiltAngle) {
                DrawOrbit(Orbit.Of(semiMinor, eccentricity, tiltAngle));
            }

            public static void DrawOrbit(Orbit orbit) {
                float drawAngle = 0f;
                Vector2 lastDrawPos = Vector2.zero;

                while(drawAngle <= 2.1f * Mathf.PI) {
                    Vector2 newDrawPos = orbit.Get(drawAngle);

                    if(lastDrawPos != Vector2.zero) {
                        UnityEngine.Gizmos.DrawLine(lastDrawPos, newDrawPos);
                    }

                    lastDrawPos = newDrawPos;
                    drawAngle += drawAngleStepRad;
                }
            }
        }
    }
}
