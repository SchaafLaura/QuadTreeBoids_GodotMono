using Godot;
using System;
using System.Collections.Generic;

public class Boid : Vertex
{
    public Vector2 acc { get; private set; }            // acceleration
    public Vector2 vel { get; private set; }            // velocity

    static float closeRangeSq           = 15f * 15f;    // range to avoid
    static float largeRange             = 30f;          // range to get close to

    static float velocityAlignment      = 0.05f;        // flock vel. align
    static float positionAlignment      = 0.05f;        // flock pos. align
    static float pathAlignment          = 0.1f;         // path align
    static float avoidStrength          = 0.4f;         // close-flock avoid
    static float randomStrength         = 0.1f;         // random movement
    static float foodAttractionStrength = 0.2f;         // attr. to player placed food
    static float foodDetectionRadius    = 100f;         

    static float velocityDecay          = 0.985f;       // vel *= velocityDecay every update
    static float accStrength            = 0.2f;         // acc. vector is set to this magnitude

    static float maxVel                 = 5.0f;         // max vel. component (-maxVel to maxVel)

    static float margin                 = 50.0f;        // edge region that is steered away from
    static float criticalMargin         = 5.0f;         // edge region that can not moved away from

    static Random rng = new Random();

    public Boid(Vector2 pos) : base(pos) 
    {
        // 
    }

    /// <summary>
    /// Updates the boid using the other boids and a path. Will crash if path is null - oopsie :3
    /// </summary>
    /// <param name="boids">The other boids</param>
    /// <param name="path">The path to follow</param>
    public void Update(QuadTree<Boid> boids, Path2D path, List<(Vector2 pos, int)> food)
    {
        // get surrounding boids
        var flock = boids.Query(Position, largeRange);

        // compute averages of 
        // - velocity
        // - position
        // - position of *very* close boids (that are to be avoided)
        (Vector2 vel, Vector2 pos, Vector2 closePos, int close) flockAvg = (
            new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0), 0);
        for(int i = 0; i < flock.Count; i++)
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
        var pathvel = path.Curve.InterpolateBaked(path.Curve.GetClosestOffset(Position)+10, true);

        // find closest food
        var foodPos = new Vector2();
        var foodExists = false;
        float record = float.MaxValue;
        for(int i = 0; i < food.Count; i++)
        {
            var d = Position.DistanceSquaredTo(food[i].pos);
            if (d > foodDetectionRadius * foodDetectionRadius)
                continue;
            if (d < record)
            {
                record = d;
                foodPos = food[i].pos;
                foodExists = true;
            }
        }

        // add all forces together
        acc =
            (flockAvg.vel - vel).Normalized() * velocityAlignment +
            (flockAvg.pos - Position).Normalized() * positionAlignment -
            (flockAvg.closePos - Position).Normalized() * avoidStrength +
            (pathvel - Position).Normalized() * pathAlignment +
            (new Vector2(rng.Next(-100, 100), rng.Next(-100, 100))).Normalized() * randomStrength;

        if (foodExists)
            acc += (foodPos - Position).Normalized() * foodAttractionStrength;

        // if boid is too close to edge, steer away from it
        // this assumes the quadtree is positioned at 0, 0
        if (Position.x < margin)
            acc = new Vector2(Math.Abs(acc.x) * 2f, acc.y);
        if (Position.y < margin)
            acc = new Vector2(acc.x, Math.Abs(acc.y) * 2f);
        if (Position.x > boids.boundary.Size.x - margin)
            acc = new Vector2(-Math.Abs(acc.x) * 2f, acc.y);
        if (Position.y > boids.boundary.Size.y - margin)
            acc = new Vector2(acc.x, -Math.Abs(acc.y) * 2f);

        // first part of euler integration (updating velocity due to accel)
        acc = acc.Normalized() * accStrength;
        vel *= velocityDecay;
        vel += acc;
        vel = vel.Constrain(-maxVel, maxVel, -maxVel, maxVel);

        // if boid is *very* close to edge, move away from it 
        // this assumes (again) the quadtree is positioned at 0, 0
        if (Position.x < criticalMargin)
            vel = new Vector2(Math.Abs(vel.x), vel.y);
        if (Position.y < criticalMargin)
            vel = new Vector2(vel.x, Math.Abs(vel.y) );
        if (Position.x > boids.boundary.Size.x - criticalMargin)
            vel = new Vector2(-Math.Abs(vel.x) , vel.y);
        if (Position.y > boids.boundary.Size.y - criticalMargin)
            vel = new Vector2(vel.x, -Math.Abs(vel.y));

        // second part of euler integration (updating position due to velocity)
        Position += vel + 0.5f * new Vector2(acc.x * acc.x, acc.y * acc.y);
        Position = Position.Constrain(0, boids.boundary.Size.x, 0, boids.boundary.Size.y);
    }
}
