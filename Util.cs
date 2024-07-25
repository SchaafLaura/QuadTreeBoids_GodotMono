using Godot;

internal static class Util
{
    public static Vector3 Constrain(this Vector3 v, float xMin, float xMax, float yMin, float yMax, float zMin, float zMax)
    {
        float x = v.x < xMin ? xMin : v.x;
        x = x > xMax ? xMax : x;

        float y = v.y < yMin ? yMin : v.y;
        y = y > yMax ? yMax : y;

        float z = v.z < zMin ? zMin : v.z;
        z = z > zMax ? zMax : z;

        return new Vector3(x, y, z);
    }

    public static Vector2 Constrain(this Vector2 v, float xMin, float xMax, float yMin, float yMax)
    {
        float x = v.x < xMin ? xMin : v.x;
        x = x > xMax ? xMax : x;

        float y = v.y < yMin ? yMin : v.y;
        y = y > yMax ? yMax : y;

        return new Vector2(x, y);
    }

    public static Vec2 ToVec2(this Vector2 v)
    {
        return new Vec2(v.x, v.y);
    }
}

