using UnityEngine;

public static class DebugExtension
{

    public static void DebugCircle(Vector3 position, Vector3 normal, Color color, float radius, bool isPersistent = false, float duration = 0)
    {
        Vector3 right = Vector3.Cross(normal, Vector3.forward).normalized;
        if (right == Vector3.zero) right = Vector3.Cross(normal, Vector3.up).normalized;
        Vector3 forward = Vector3.Cross(normal, right).normalized;
        Vector3 from = position + radius * right;
        float stepSize = 10;
        for (int i = 1; i <= 360 / stepSize; i++)
        {
            Vector3 to = position + radius * (right * Mathf.Cos(i * stepSize * Mathf.Deg2Rad) + forward * Mathf.Sin(i * stepSize * Mathf.Deg2Rad));
            Debug.DrawLine(from, to, color, duration, false);
            from = to;
        }
    }
}