using UnityEngine;

namespace Galactica.Utility {
    public static class Randomf {
        public static bool Half => Chance(0.5f);

        public static bool Chance(float probability) {
            return Random.value < probability;
        }

        public static float Triangular(float min, float max, float median) {
            if(min > median || median > max || min > max) throw new System.ArgumentException($"Invalid arguments for {nameof(Triangular)}");
            if(min == max) return min;

            float u = Random.value;
            float f = (median - min) / (max - min);

            if(u < f) {
                return min + Mathf.Sqrt(u * (max - min) * (median - min));
            } else {
                return max - Mathf.Sqrt((1f - u) * (max - min) * (max - median));
            }
        }
    }
}
