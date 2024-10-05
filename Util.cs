using Godot;
using System;

internal static class Util
{
    public static Random rng = new Random();
    public static Vector3 Constrain(this Vector3 v, float xMin, float xMax, float yMin, float yMax, float zMin, float zMax)
    {
        float x = v.X < xMin ? xMin : v.X;
        x = x > xMax ? xMax : x;

        float y = v.Y < yMin ? yMin : v.Y;
        y = y > yMax ? yMax : y;

        float z = v.Z < zMin ? zMin : v.Z;
        z = z > zMax ? zMax : z;

        return new Vector3(x, y, z);
    }

    public static Vector2 Constrain(this Vector2 v, float xMin, float xMax, float yMin, float yMax)
    {
        float x = v.X < xMin ? xMin : v.X;
        x = x > xMax ? xMax : x;

        float y = v.Y < yMin ? yMin : v.Y;
        y = y > yMax ? yMax : y;

        return new Vector2(x, y);
    }

    public static Vec2 ToVec2(this Vector2 v)
    {
        return new Vec2(v.X, v.Y);
    }
}

