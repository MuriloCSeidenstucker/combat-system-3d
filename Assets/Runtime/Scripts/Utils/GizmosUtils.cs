using UnityEngine;

public static class GizmosUtils
{

    private const float defaultThickness = 8.0f;

    public static void DrawRay(Vector3 from, Vector3 direction, float width = defaultThickness)
    {
        DrawLine(from, from + direction, width);
    }

    public static void DrawLine(Vector3 p1, Vector3 p2, float width = defaultThickness)
    {
        int count = 1 + Mathf.CeilToInt(width); // how many lines are needed.
        if (count == 1)
        {
            Gizmos.DrawLine(p1, p2);
        }
        else
        {
            Camera c = Camera.current;
            if (c == null)
            {
                Debug.LogError("Camera.current is null");
                return;
            }
            var scp1 = c.WorldToScreenPoint(p1);
            var scp2 = c.WorldToScreenPoint(p2);

            Vector3 v1 = (scp2 - scp1).normalized; // line direction
            Vector3 n = Vector3.Cross(v1, Vector3.forward); // normal vector

            for (int i = 0; i < count; i++)
            {
                Vector3 o = 0.99f * n * width * ((float)i / (count - 1) - 0.5f);
                Vector3 origin = c.ScreenToWorldPoint(scp1 + o);
                Vector3 destiny = c.ScreenToWorldPoint(scp2 + o);
                Gizmos.DrawLine(origin, destiny);
            }
        }
    }

    public static void DrawVectorAtOrigin(Vector3 vec, float thickness = defaultThickness, bool drawArrowCap = true)
    {
        DrawVector(Vector3.zero, vec, thickness, drawArrowCap);
    }

    public static void DrawVector(Vector3 pos, Vector3 direction, float thickness = defaultThickness, bool drawArrowCap = true)
    {
        const float arrowHeadLength = 0.25f;
        const float arrowHeadAngle = 20.0f;

        DrawRay(pos, direction, thickness);

        if (drawArrowCap)
        {
            Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            DrawRay(pos + direction, right * arrowHeadLength, thickness);
            DrawRay(pos + direction, left * arrowHeadLength, thickness);
        }
    }

    public static void DrawPlane(Vector3 normal, Vector3 point, Vector2 size)
    {
        var prevMatrix = Gizmos.matrix;

        Quaternion rotation = Quaternion.LookRotation(normal);
        Matrix4x4 trs = Matrix4x4.TRS(point, rotation, Vector3.one);
        Gizmos.matrix = trs;
        Color32 color = Color.blue;
        color.a = 125;
        Gizmos.color = color;
        Gizmos.DrawCube(Vector3.zero, new Vector3(size.x, size.y, 0.0001f));

        Gizmos.matrix = prevMatrix;
    }
}