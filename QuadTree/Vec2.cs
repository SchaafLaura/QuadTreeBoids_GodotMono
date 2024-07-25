using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;
namespace Idkyet.QuadTree
{
    internal class Vec2
    {
        public static Vec2 operator +(Vec2 a, Vec2 b) => new Vec2(a.x + b.x, a.y + b.y);
        public static Vec2 operator -(Vec2 v) => new Vec2(-v.x, -v.y);
        public static Vec2 operator -(Vec2 a, Vec2 b) => a - b;
        public static Vec2 operator *(Vec2 v, float f) => new Vec2(v.x * f, v.y * f);


        private static Random rng = new Random();
        public static Vec2 RandomNormal()
        {
            var x = rng.Next(-10000, 10000);
            var y = rng.Next(-10000, 10000);
            Vec2 v = new Vec2(x, y);
            v.Normalize();
            return v;
        }


        public float x, y;
        public Vec2(float x, float y)
        {
            this.x = x;
            this.y = y;
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
    }
}
