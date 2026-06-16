using UnityEngine;

namespace MidnightReturn.Utils
{
    public static class MathUtils
    {
        public static float Approach(float current, float target, float delta)
        {
            if (current < target) return Mathf.Min(current + delta, target);
            return Mathf.Max(current - delta, target);
        }

        public static float Lerp(float a, float b, float t) => a + (b - a) * t;
        public static float Clamp(float v, float min, float max) => Mathf.Max(min, Mathf.Min(max, v));
        public static int Sign(float v) => v < 0 ? -1 : v > 0 ? 1 : 0;

        public static float RandRange(float min, float max) => Random.Range(min, max);
        public static int RandInt(int min, int max)         => Random.Range(min, max + 1);

        // Exponential level threshold (SotN growth curve)
        public static int ExpThreshold(int level) => Mathf.FloorToInt(100 * Mathf.Pow(1.45f, level - 1));
    }
}
