using System.Collections.Generic;
using UnityEngine;
using Galactica.World.Objects;
using Galactica.Utility;
using Unity.VisualScripting;

namespace Galactica.World {
    public class GalaxyManager : MonoBehaviour {
        public static GalaxyManager Instance { get; private set; }

        [Header("Universe"), SerializeField] private float gravitationalConstant;
        [Header("Galaxy"), SerializeField] private float galaxyRadius, galaxyMinRadius;
        [SerializeField] private float galacticCenterMass, innermostObjectVelocity, outermostObjectVelocity;

        [Header("Stars"), SerializeField] private int starCount;
        [SerializeField] private AnimationCurve starSizeDistribution, starTemperatureDistribution, galaxyStarDistribution;
        [SerializeField] private GameObject starPrefab;
        [SerializeField] private Transform starParentTransform;

        [Header("Nebulae"), SerializeField] private int nebulaCount;
        [SerializeField] private AnimationCurve nebulaSizeDistribution, galaxyNebulaDistribution, nebulaSizeByDistCorrelation;
        [SerializeField] private GameObject nebulaPrefab;
        [SerializeField] private Transform nebulaParentTransform;

        [Header("Orbits"), SerializeField, Range(0.001f, 0.999f)] private float orbitalEccentricity;
        [SerializeField] private AnimationCurve eccentricityByDistCorrelation;
        [SerializeField, Range(0f, 360f)] private float galacticArmAngle;
        [SerializeField, Range(0f, 360f)] private float armGenAngleWidth;
        [SerializeField, Range(0f, 360f)] private float orbitAngleLagToGalacticArm = 50f;
        [SerializeField, Range(-5f, 5f)] private float orbitTiltRate = -2f;

        [Header("Galactic Arm"), SerializeField] private AnimationCurve armNebulaSizeByDistCorrelation;
        [SerializeField] private Gradient armNebulaColor;
        [SerializeField] private Vector2 armNebulaSizeBounds;
        [SerializeField] private float armNebulaSpacing;
        [SerializeField, Range(0f, 180f)] private float armNebulaSpread;
        [SerializeField] private GameObject armNebulaPrefab;
        [SerializeField] private Transform armNebulaParentTransform;

        [Header("Gizmos"), SerializeField] private float maxOrbitSemiMinor = 1000f;
        [SerializeField, Range(1f, 100f)] private float orbitDrawStep = 50f;
        [SerializeField] private bool drawOrbitPathGizmo, drawGalacticArmGizmo;

        private readonly List<(GalacticObject obj, float dist)> galacticObjects = new List<(GalacticObject, float)>();

        private void Awake() {
            if(Instance != null) Destroy(this);
            Instance = this;
        }

        private void Start() {
            // Check if all prefabs are valid
            bool valid = true;
            if(starPrefab.GetComponent<Star>() == null) {
                Debug.LogError($"Error while generating galaxy: star prefab has no {nameof(Star)} component attached.");
                valid = false;
            }
            if(nebulaPrefab.GetComponent<Nebula>() == null) {
                Debug.LogError($"Error while generating galaxy: nebula prefab has no {nameof(Nebula)} component attached.");
                valid = false;
            }
            if(armNebulaPrefab.GetComponent<SpriteRenderer>() == null) {
                Debug.LogError($"Error while generating galaxy: arm nebula prefab has no {nameof(SpriteRenderer)} component attached.");
                valid = false;
            }

            if(valid) {
                for(int i = 0; i < starCount; i++) {
                    // Generate position vector for the new star
                    Vector2 dir = Random.insideUnitCircle;
                    Vector2 nextPosition = (galaxyRadius - galaxyMinRadius) * galaxyStarDistribution.Evaluate(Random.value) * dir + dir.normalized * galaxyMinRadius;

                    // Instantiate the new star
                    GameObject newStar = Instantiate(starPrefab, nextPosition, Quaternion.identity, starParentTransform);

                    // Initialize orbit
                    Star star = newStar.GetComponent<Star>();
                    float starDist = star.DistanceFromGC;
                    float eccentricityScale = eccentricityByDistCorrelation.Evaluate(Mathf.InverseLerp(galaxyMinRadius, galaxyRadius, starDist));
                    star.Orbit = Orbit.Of(starDist, orbitalEccentricity * eccentricityScale, (galacticArmAngle + starDist * orbitTiltRate) * Mathf.Deg2Rad);

                    // Calculate initial orbital angle
                    float orbitAnglePos = star.Orbit.TiltAngle + (orbitAngleLagToGalacticArm - galacticArmAngle + Randomf.Triangular(-armGenAngleWidth / 2f, armGenAngleWidth / 2f, 0f)) * Mathf.Deg2Rad;
                    if(Randomf.Half) orbitAnglePos += 180f * Mathf.Deg2Rad;
                    star.OrbitAnglePos = orbitAnglePos;

                    // Initialize values
                    star.Size = starSizeDistribution.Evaluate(Random.value);
                    star.Temperature = starTemperatureDistribution.Evaluate(Random.value);
                    galacticObjects.Add((star, star.DistanceFromGC));
                }

                for(int i = 0; i < nebulaCount; i++) {
                    // Generate position vector for the new nebula
                    Vector2 dir = Random.insideUnitCircle;
                    Vector2 nextPosition = (galaxyRadius - galaxyMinRadius) * galaxyNebulaDistribution.Evaluate(Random.value) * dir + dir.normalized * galaxyMinRadius;

                    // Instantiate the new nebula
                    GameObject newNebula = Instantiate(nebulaPrefab, nextPosition, Quaternion.identity, nebulaParentTransform);

                    // Initialize orbit
                    Nebula nebula = newNebula.GetComponent<Nebula>();
                    float nebulaDist = nebula.DistanceFromGC;
                    float eccentricityScale = eccentricityByDistCorrelation.Evaluate(Mathf.InverseLerp(galaxyMinRadius, galaxyRadius, nebulaDist));
                    nebula.Orbit = Orbit.Of(nebulaDist, orbitalEccentricity * eccentricityScale, (galacticArmAngle + nebulaDist * orbitTiltRate) * Mathf.Deg2Rad);

                    // Calculate initial orbital angle
                    float orbitAnglePos = nebula.Orbit.TiltAngle + (orbitAngleLagToGalacticArm - galacticArmAngle + Randomf.Triangular(-armGenAngleWidth / 2f, armGenAngleWidth / 2f, 0f)) * Mathf.Deg2Rad;
                    if(Randomf.Half) orbitAnglePos += 180f * Mathf.Deg2Rad;
                    nebula.OrbitAnglePos = orbitAnglePos;

                    // Initialize values
                    nebula.Size = starSizeDistribution.Evaluate(Random.value) * nebulaSizeByDistCorrelation.Evaluate(nebulaDist / galaxyRadius);
                    galacticObjects.Add((nebula, nebulaDist));
                }

                // Sort ascending on the distance of the object from the galactic center
                galacticObjects.Sort((a, b) => {
                    return a.dist.CompareTo(b.dist);
                });

                // Generate arm nebula accent
                for(float radius = galaxyMinRadius; radius <= galaxyRadius; radius += armNebulaSpacing) {
                    // Generate position vectors for both arm
                    float eccentricityScale = eccentricityByDistCorrelation.Evaluate(Mathf.InverseLerp(galaxyMinRadius, galaxyRadius, radius));
                    Orbit orbit = Orbit.Of(radius, orbitalEccentricity * eccentricityScale, (galacticArmAngle + radius * orbitTiltRate) * Mathf.Deg2Rad);

                    float angleLag = orbitAngleLagToGalacticArm * Mathf.Deg2Rad;
                    (Vector2 a, Vector2 b) = (
                        orbit.Get(0f + orbit.TiltAngle - angleLag + Randomf.Triangular(-armNebulaSpread / 2f, armNebulaSpread / 2f, 0f) * Mathf.Deg2Rad),
                        orbit.Get(Mathf.PI + orbit.TiltAngle - angleLag + Randomf.Triangular(-armNebulaSpread / 2f, armNebulaSpread / 2f, 0f) * Mathf.Deg2Rad)
                    );

                    Vector2 pos = a;
                    for(int i = 0; i < 2; i++) {
                        // Instantiate the new arm nebula
                        GameObject newArmNebula = Instantiate(armNebulaPrefab, pos, Quaternion.identity, armNebulaParentTransform);

                        // Set values
                        float sizeScale = armNebulaSizeByDistCorrelation.Evaluate(Mathf.InverseLerp(galaxyMinRadius, galaxyRadius, (pos - (Vector2)transform.position).magnitude)) * Random.value;
                        newArmNebula.transform.localScale = Mathf.Lerp(armNebulaSizeBounds.x, armNebulaSizeBounds.y, sizeScale) * Vector3.one;
                        newArmNebula.GetComponent<SpriteRenderer>().color = armNebulaColor.Evaluate(Random.value);

                        pos = b;
                    }
                }
            }
        }

        private void Update() {
            // Set initial mass to the galactic center mass
            float mass = galacticCenterMass;
            float lastDist = 0f;

            foreach((GalacticObject obj, float dist) in galacticObjects) {
                // Calculate the new angular position of each object
                float currentAngle = obj.OrbitAnglePos;
                //float angularVel = Mathf.Sqrt(gravitationalConstant * mass / Mathf.Pow(obj.Orbit.SqrRadiusAt(currentAngle), 1.5f));
                float objDist = obj.Orbit.RadiusAt(currentAngle);
                float objPosRel = Mathf.InverseLerp(galaxyMinRadius, galaxyRadius, objDist);
                float linearVel = Mathf.LerpUnclamped(innermostObjectVelocity, outermostObjectVelocity, objPosRel);
                float angularVel = linearVel / objDist;
                float newAngle = currentAngle + angularVel * Time.deltaTime;

                // Set the position of the object to its new calculated position
                //obj.transform.position = new Vector3(dist * Mathf.Cos(newAngle), dist * Mathf.Sin(newAngle));
                //obj.transform.position = obj.Orbit.Get(newAngle);
                obj.OrbitAnglePos = newAngle;

                // Add in the mass contribution of this object for the position calculation of the next object plus dark matter influence
                //mass += obj.Mass + Mathf.Pow(dist - lastDist, darkMatterExponent) * darkMatterDensity;
                mass += obj.Mass;
                lastDist = dist;
            }
        }

        private void OnDrawGizmosSelected() {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, galaxyMinRadius);
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, galaxyRadius);

            if(orbitDrawStep >= 1f) {
                (Vector2 a, Vector2 b) lastArmPoint = (Vector2.zero, Vector2.zero);
                for(float semiMinor = orbitDrawStep; semiMinor <= maxOrbitSemiMinor; semiMinor += orbitDrawStep) {
                    float eccentricityScale = eccentricityByDistCorrelation.Evaluate(Mathf.InverseLerp(galaxyMinRadius, galaxyRadius, semiMinor));
                    Orbit orbit = Orbit.Of(semiMinor, orbitalEccentricity * eccentricityScale, (galacticArmAngle + semiMinor * orbitTiltRate) * Mathf.Deg2Rad);

                    if(drawOrbitPathGizmo) {
                        Gizmos.color = Color.yellow;
                        OrbitSolver.Gizmos.DrawOrbit(orbit);
                    }

                    if(drawGalacticArmGizmo) {
                        float angleLag = orbitAngleLagToGalacticArm * Mathf.Deg2Rad;
                        (Vector2 a, Vector2 b) nextArmPoint = (orbit.Get(0f + orbit.TiltAngle - angleLag), orbit.Get(Mathf.PI + orbit.TiltAngle - angleLag));
                        if(lastArmPoint.a != Vector2.zero) {
                            Gizmos.color = Color.blue;
                            Gizmos.DrawLine(lastArmPoint.a, nextArmPoint.a);
                        }
                        if(lastArmPoint.b != Vector2.zero) {
                            Gizmos.color = Color.blue;
                            Gizmos.DrawLine(lastArmPoint.b, nextArmPoint.b);
                        }
                        lastArmPoint = nextArmPoint;
                    }
                }
            }
        }
    }
}
