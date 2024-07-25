using System;
using System.Runtime.CompilerServices;
using Godot;

internal class Vec2
{
    public static Vec2 operator +(Vec2 a, Vec2 b) => new Vec2(a.x + b.x, a.y + b.y);
    public static Vec2 operator -(Vec2 a, Vec2 b) => new Vec2(a.x - b.x, a.y - b.y);
    public static Vec2 operator *(Vec2 v, float f) => new Vec2(v.x * f, v.y * f);
    public static bool operator ==(Vec2 a, Vec2 b) => a.x == b.x && a.y == b.y;
    public static bool operator !=(Vec2 a, Vec2 b) => !(a == b);

    public static Vec2 Random()
    {
        lock (Util.rng)
        {
            var rx = (float)Util.rng.NextDouble() - 0.5f;
            var ry = (float)Util.rng.NextDouble() - 0.5f;
            /*while (rx == 0 || ry == 0)
            {
                rx = (float)Util.rng.NextDouble() - 0.5f;
                ry = (float)Util.rng.NextDouble() - 0.5f;
            }*/
            Vec2 v = new Vec2(rx, ry);
            v.Normalize();
            return v;
        }
    }

    public float x, y;
    public Vec2(float x, float y)
    {
        this.x = x;
        this.y = y;
    }

    public override string ToString()
    {
        return "("+x+","+y+")";
    }

    public void Add(Vec2 v)
    {
        x += v.x;
        y += v.y;
    }
    public void Subtract(Vec2 v)
    {
        x -= v.x;
        y -= v.y;
    }
    public void Multiply(float f)
    {
        x *= f;
        y *= f;
    }
    public void Divide(float f)
    {
        x /= f;
        y /= f;
    }
    public void Normalize()
    {
        if (x == 0 && y == 0) return;
        Divide((float)Math.Sqrt(x * x + y * y));
    }
    public void SetMag(float mag)
    {
        Normalize();
        Multiply(mag);
    }
    public void Constrain(float xMin, float xMax, float yMin, float yMax)
    {
        ConstrainX(xMin, xMax);
        ConstrainY(yMin, yMax);
    }
    public void ConstrainY(float min, float max)
    {
        y = y < min ? min : y;
        y = y > max ? max : y;
    }
    public void ConstrainX(float min, float max)
    {
        x = x < min ? min : x;
        x = x > max ? max : x;
    }

    public Vec2 Normalized()
    {
        var v = new Vec2(x, y);
        v.Normalize();
        return v;
    }
    public Vector2 ToVector2()
    {
        return new Vector2(x, y);
    }
    public float DistanceSquaredTo(Vec2 v)
    {
        var dx = x - v.x;
        var dy = y - v.y;
        return dx * dx + dy * dy;
    }
    public float Magnitude()
    {
        return x * x + y * y;
    }
    public float Angle()
    {
        return ToVector2().Angle();
    }
}