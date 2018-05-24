using Unity.Mathematics;

namespace E7.Entities
{
    public static class ECSMathExtension
    {
        public static float inverseLerp(float a, float b, float value)
        {
            if (a != b)
                return math.clamp((value - a) / (b - a), 0, 1);
            else
                return 0.0f;
        }
    }
}