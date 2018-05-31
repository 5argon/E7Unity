using Unity.Mathematics;
using UnityEngine;

namespace E7.ECS
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

        /// <summary>
        /// Cannot be used in a Job!
        /// RectTransform has to be aligned completely flat to the screen. Discards all Z information. The purpose is to use `Rect` in a job because it is a struct.
        /// </summary>
        public static Rect ScreenRectOfRectTransform(RectTransform rt, Camera c)
        {
            Vector3[] fc = new Vector3[4];
            rt.GetWorldCorners(fc);
            for (int a = 0; a < 3; a++)
            {
                fc[a] = Camera.main.WorldToScreenPoint(fc[a]);
            }
            return new Rect(fc[0].x, fc[0].y, fc[2].x - fc[1].x, fc[1].y - fc[0].y);
        }

        public static bool RectContains(in Rect rect, float2 point)
        {
            return (point.x >= rect.xMin) && (point.x < rect.xMax) && (point.y >= rect.yMin) && (point.y < rect.yMax);
        }
    }
}