using UnityEngine;

namespace VARLab.Navigation.PointClick
{
    public static class MathHelper
    {
        /// <summary>
        /// Check if two vectors are approximately equal for a given error threshold
        /// </summary>
        /// <param name="vecA">First vector to compare</param>
        /// <param name="vecB">Second vector to compare</param>
        /// <param name="errorThreshold">Acceptable error threshold</param>
        /// <returns>Returns true if both vectors are approximately equals</returns>
        public static bool CompareVectors(Vector3 vecA, Vector3 vecB, float errorThreshold)
        {
            return CompareFloats(vecA.x, vecB.x, errorThreshold) &&
                CompareFloats(vecA.y, vecB.y, errorThreshold) &&
                CompareFloats(vecA.z, vecB.z, errorThreshold);
        }

        /// <summary>
        /// Check if two floats are approximately equal for a given error threshold
        /// </summary>
        /// <param name="a">First float to compare</param>
        /// <param name="b">Second float to compare</param>
        /// <param name="errorThreshold">Acceptable error threshold</param>
        /// <returns>Returns true if both floats are approximately equals</returns>
        public static bool CompareFloats(float a, float b, float errorThreshold)
        {
            if (errorThreshold > 0f)
            {
                return Mathf.Abs(a - b) <= errorThreshold;
            }
            else
            {
                return Mathf.Approximately(a, b);
            }
        }
    }
}
