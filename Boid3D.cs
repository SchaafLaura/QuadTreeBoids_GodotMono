using Godot;
using System;
using System.Diagnostics;
internal class Boid3D : Vertex3D
{
    public Boid3D(Vector3 position) : base(position) { }

    public Vector3 acc { get; private set; } // acceleration
    public Vector3 vel { get; private set; } // velocity

    static float closeRangeSq = 15f * 15f; // range to avoid
    static float largeRange = 30f; // range to get close to

    static float velocityAlignment = 0.05f;
    static float positionAlignment = 0.05f;
    static float pathAlignment = 0.1f;
    static float avoidStrength = 0.4f;
    static float randomStrength = 0.1f;

    static float velocityDecay = 0.985f;
    static float accStrength = 0.2f;

    static float maxVel = 5.0f;

    static float margin = 50.0f;
    static float criticalMargin = 5.0f;

    static Random rng = new Random();

    public void Update(Octree<Boid3D> boids, Path3D path)
    {
        // get surrounding boids
        var flock = boids.Query(Position, largeRange);

        // compute averages of 
        // - velocity
        // - position
        // - position of *very* close boids (that are to be avoided)
        (Vector3 vel, Vector3 pos, Vector3 closePos, int close) flockAvg = (
            new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(0, 0, 0), 0);
        for (int i = 0; i < flock.Count; i++)
        {
            var other = flock[i];

            flockAvg.vel += other.vel;
            flockAvg.pos += other.Position;
            if (Position.DistanceSquaredTo(other.Position) < closeRangeSq)
            {
                flockAvg.closePos += other.Position;
                flockAvg.close++;
            }
        }

        // divide by count to get average
        flockAvg = (
            flockAvg.vel / flock.Count,
            flockAvg.pos / flock.Count,
            flockAvg.close > 0 ? flockAvg.closePos / flockAvg.close : flockAvg.closePos,
            flockAvg.close);

        // find the closest point on curve
        var pathvel = path.Curve.SampleBaked(path.Curve.GetClosestOffset(Position) + 10, true);

        // add all forces together
        acc =
            (flockAvg.vel - vel).Normalized() * velocityAlignment +
            (flockAvg.pos - Position).Normalized() * positionAlignment -
            (flockAvg.closePos - Position).Normalized() * avoidStrength +
            (pathvel - Position).Normalized() * pathAlignment +
            (new Vector3(rng.Next(-100, 100), rng.Next(-100, 100), rng.Next(-100, 100))).Normalized() * randomStrength;

        // if boid is too close to edge, steer away from it
        // this assumes the quadtree is positioned at 0, 0, 0
        if (Position.X < margin)
            acc = new Vector3(Math.Abs(acc.X) * 2f, acc.Y, acc.Z);
        if (Position.Y < margin)
            acc = new Vector3(acc.X, Math.Abs(acc.Y) * 2f, acc.Z);
        if (Position.Z < margin)
            acc = new Vector3(acc.X, acc.Y, Math.Abs(acc.Z) * 2f);
        if (Position.X > boids.boundary.Size.X - margin)
            acc = new Vector3(-Math.Abs(acc.X) * 2f, acc.Y, acc.Z);
        if (Position.Y > boids.boundary.Size.Y - margin)
            acc = new Vector3(acc.X, -Math.Abs(acc.Y) * 2f, acc.Z);
        if (Position.Z > boids.boundary.Size.Z - margin)
            acc = new Vector3(acc.X, acc.Y, -Math.Abs(acc.Z) * 2f);

        // first part of euler integration (updating velocity due to accel)
        acc = acc.Normalized() * accStrength;
        vel *= velocityDecay;
        vel += acc;
        vel = vel.Constrain(-maxVel, maxVel, -maxVel, maxVel, -maxVel, maxVel);

        // if boid is *very* close to edge, move away from it 
        // this assumes (again) the quadtree is positioned at 0, 0, 0
        if (Position.X < criticalMargin)
            vel = new Vector3(Math.Abs(vel.X), vel.Y, vel.Z);
        if (Position.Y < criticalMargin)
            vel = new Vector3(vel.X, Math.Abs(vel.Y), vel.Z);
        if (Position.Z < criticalMargin)
            vel = new Vector3(vel.X, vel.Y, Math.Abs(vel.Z));
        if (Position.X > boids.boundary.Size.X - criticalMargin)
            vel = new Vector3(-Math.Abs(vel.X), vel.Y, vel.Z);
        if (Position.Y > boids.boundary.Size.Y - criticalMargin)
            vel = new Vector3(vel.X, -Math.Abs(vel.Y), vel.Z);
        if (Position.Z > boids.boundary.Size.Z - criticalMargin)
            vel = new Vector3(vel.X, vel.Y, -Math.Abs(vel.Z));

        // second part of euler integration (updating position due to velocity)
        Position += vel + 0.5f * new Vector3(acc.X * acc.X, acc.Y * acc.Y, acc.Z * acc.Z);
        Position = Position.Constrain(0, boids.boundary.Size.X, 0, boids.boundary.Size.Y, 0, boids.boundary.Size.Z);
    }
}

