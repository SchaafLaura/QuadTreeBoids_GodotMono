using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;

internal class Boid : Vec2
{
    public Vec2 acc { get; private set; } = new Vec2(0, 0); // acceleration
    public Vec2 vel { get; private set; } = new Vec2(0, 0); // velocity

    static float closeRangeSq           = 10f * 10f;    // range to avoid
    static float largeRange             = 20f;          // range to get close to

    static float velocityAlignment      = 0.05f;        // flock vel. align
    static float positionAlignment      = 0.05f;        // flock pos. align
    static float pathAlignment          = 0.1f;         // path align
    static float pathLookAhead          = 0.003f;       // (0-1)
    static float avoidStrength          = 0.4f;         // close-flock avoid
    static float randomStrength         = 0.1f;         // random movement
    static float foodAttractionStrength = 0.2f;         // attr. to player placed food
    static float foodDetectionRadius    = 100f;         

    static float velocityDecay          = 0.985f;       // vel *= velocityDecay every update
    static float accStrength            = 0.2f;         // acc. vector is set to this magnitude

    static float maxVel                 = 5.0f;         // max vel. component (-maxVel to maxVel)

    static float margin                 = 50.0f;        // edge region that is steered away from
    static float marginSteerStrength    = 2.0f;         // strength of edge steering
    static float criticalMargin         = 5.0f;         // edge region that can not moved away from

    static Random rng = new Random();

    public Boid(float x, float y) : base(x, y) 
    {
        // 
    }

    /// <summary>
    /// Updates the boid using the other boids and a path. Will crash if path is null - oopsie :3
    /// </summary>
    /// <param name="boids">The other boids</param>
    /// <param name="path">The path to follow</param>
    public void Update(QuadTree<Boid> boids, Path2D path, List<(Vec2 pos, int)> food)
    {
        // get surrounding boids
        var flock = boids.Query(this, largeRange);

        // compute averages of 
        // - velocity
        // - position
        // - position of *very* close boids (that are to be avoided)
        (Vec2 vel, Vec2 pos, Vec2 closePos, int close) flockAvg = (
            new Vec2(0, 0), new Vec2(0, 0), new Vec2(0, 0), 0);
        for(int i = 0; i < flock.Count; i++)
        {
            var other = flock[i];

            flockAvg.vel.Add(other.vel);
            flockAvg.pos.Add(other);
            if (DistanceSquaredTo(other) < closeRangeSq)
            {
                flockAvg.closePos.Add(other);
                flockAvg.close++;
            }
        }

        // divide by count to get average
        flockAvg.vel.Divide(flock.Count);
        flockAvg.pos.Divide(flock.Count);

        if(flockAvg.close > 0)
            flockAvg.closePos.Divide(flockAvg.close);

        // find the closest point on curve
        var pathvel = path.Curve.InterpolateBaked
            (path.Curve.GetClosestOffset(ToVector2()) + 
            path.Curve.GetBakedLength() * pathLookAhead, true).ToVec2();

        
        // find closest food
        var foodPos = new Vec2(0, 0);
        var foodExists = false;
        float record = float.MaxValue;
        for(int i = 0; i < food.Count; i++)
        {
            var d = DistanceSquaredTo(food[i].pos);
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
            (flockAvg.pos - this).Normalized() * positionAlignment -
            (flockAvg.closePos - this).Normalized() * avoidStrength +
            (pathvel - this).Normalized() * pathAlignment +
            RandomNormal() * randomStrength;

        if (foodExists)
            acc += (foodPos - this).Normalized() * foodAttractionStrength;

        // if boid is too close to edge, steer away from it
        // this assumes the quadtree is positioned at 0, 0
        if (x < margin)
            acc.x = Math.Abs(acc.x) * marginSteerStrength;
        if (y < margin)
            acc.y = Math.Abs(acc.y) * marginSteerStrength;
        if (x > boids.boundary.Size.x - margin)
            acc.x = -Math.Abs(acc.x) * marginSteerStrength;
        if (y > boids.boundary.Size.y - margin)
            acc.y = -Math.Abs(acc.y) * marginSteerStrength;
        

        // first part of euler integration (updating velocity due to accel)
        acc.SetMag(accStrength);
        vel.Multiply(velocityDecay);
        vel.Add(acc);
        vel.Constrain(-maxVel, maxVel, -maxVel, maxVel);

        // if boid is *very* close to edge, move away from it 
        // this assumes (again) the quadtree is positioned at 0, 0
        if (x < criticalMargin)
            vel.x = Math.Abs(vel.x);
        if (y < criticalMargin)
            vel.y = Math.Abs(vel.y);
        if (x > boids.boundary.Size.x - criticalMargin)
            vel.x = -Math.Abs(vel.x);
        if (y > boids.boundary.Size.y - criticalMargin)
            vel.y = -Math.Abs(vel.y);

        acc.x *= acc.x;
        acc.y *= acc.y;

        // second part of euler integration (updating position due to velocity)
        Add(vel + acc * 0.5f);
        Constrain(0, boids.boundary.Size.x, 0, boids.boundary.Size.y);
    }
}
