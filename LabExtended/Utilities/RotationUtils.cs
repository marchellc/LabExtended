using UnityEngine;

namespace LabExtended.Utilities
{
    public static class RotationUtils
    {
        public static void LookAt(Vector3 direction, ref float curVertical, ref float curHorizontal, float lerp = 1f)
        {
            var vector = Quaternion.LookRotation(direction, Vector3.up).eulerAngles;

            curVertical = Mathf.LerpAngle(curVertical, -vector.x, lerp);
            curHorizontal = Mathf.LerpAngle(curHorizontal, vector.y, lerp);
        }

        public static Vector3 LookAt(Vector3 position, Vector3 up)
            => Quaternion.LookRotation(position, up).eulerAngles;
    }
}