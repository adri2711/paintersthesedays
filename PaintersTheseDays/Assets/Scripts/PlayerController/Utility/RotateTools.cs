using UnityEngine;

public static class RotationTools
{
    public static Quaternion ClampRotationAroundXAxis(Quaternion q, float minAngle, float maxAngle)
    {
        var euler = q.eulerAngles;
        if (euler.x > 180)
        {
            euler.x -= 360;
        }
        euler.x = Mathf.Clamp(euler.x, minAngle, maxAngle);
        return Quaternion.Euler(euler);
    }
}
