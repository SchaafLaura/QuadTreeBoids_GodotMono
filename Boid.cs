using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;

internal class Boid : Vec2
{
    public Vec2 acc { get; private set; } = new Vec2(0, 0); // acceleration
    public Vec2 vel { get; private set; } = new Vec2(0, 0); // velocity


    //public bool process = Util.rng.NextDouble() < 0.5;

    static float closeRangeSq           = 12f * 12f;    // range to avoid
    static float largeRange             = 20f;          // range to get close to

    static float velocityAlignment      = 0.05f;        // flock vel. align
    static float positionAlignment      = 0.05f;        // flock pos. align
    static float pathAlignment          = 0.1f;         // path align
    static float pathLookAhead          = 0.03f;        // (0-1) probably keep this below 0.1
    static bool  useCubicInterpolation  = false;        // interpolation for path lookahead linear or cubic
    static float avoidStrength          = 0.7f;         // close-flock avoid
    static float randomStrength         = 0.1f;         // random movement
    static float foodAttractionStrength = 0.4f;         // attr. to player placed food
    static float foodDetectionRadiusSq  = 100f * 100f;
    static float foodEatRadiusSq        = 20f * 20f;

    static float velocityDecay          = 0.985f;       // vel *= velocityDecay every update
    static float accStrength            = 0.2f;         // acc. vector is set to this magnitude

    static float maxVel                 = 3.0f;         // max vel. component (-maxVel to maxVel)

    static float margin                 = 15.0f;        // edge region that is steered away from
    static float marginSteerStrength    = 2.0f;         // strength of edge steering
    static float criticalMargin         = 5.0f;         // edge region that can not moved away from

    public Boid(float x, float y) : base(x, y) 
    {
        // 
    }

    public Boid Clone()
    {
        return new Boid(x, y)
        {
            vel = new Vec2(vel.x, vel.y),
            acc = new Vec2(acc.x, acc.y)
        };
    }

    /// <summary>
    /// Updates the boid using the other boids, a path and food. Will crash if path is null - oopsie :3
    /// </summary>
    /// <param name="boids">The other boids</param>
    /// <param name="path">The path to follow</param>
    /// <param name="food">The food items to path towards</param>
    public void Update(QuadTree<Boid> boids, Path2D path, List<(Vec2 pos, int)> food)
    {
        /* process = !process;

         if (process)
         {
        */

        // get surrounding boids
        var flock = boids.Query(this, largeRange);

        // compute averages of 
        // - velocity
        // - position
        // - position of *very* close boids (that are to be avoided)
        (Vec2 vel, Vec2 pos, Vec2 closePos, int close) flockAvg = (
            new Vec2(0, 0), new Vec2(0, 0), new Vec2(0, 0), 0);
        for (int i = 0; i < flock.Count; i++)
        {
            var other = flock[i];
            var d = DistanceSquaredTo(other);
            flockAvg.vel.Add(other.vel);
            flockAvg.pos.Add(other);
            if (d < closeRangeSq)
            {
                flockAvg.closePos.Add(other);
                flockAvg.close++;
            }
        }

        // divide by count to get average
        if (flock.Count > 0)
        {
            flockAvg.vel.Divide(flock.Count);
            flockAvg.pos.Divide(flock.Count);
        }
        else
        {
            flockAvg.pos.x = x;
            flockAvg.pos.y = y;

            flockAvg.vel.x = vel.x;
            flockAvg.vel.y = vel.y;
        }

        if (flockAvg.close > 0)
            flockAvg.closePos.Divide(flockAvg.close);

        // find the closest point on curve in global space
        var pathvel = path.Curve.InterpolateBaked(
            path.Curve.GetClosestOffset(new Vector2(x - path.Position.x, y - path.Position.y)) +
            path.Curve.GetBakedLength() * pathLookAhead,
            useCubicInterpolation)
        .ToVec2();

        pathvel.x += path.Position.x;
        pathvel.y += path.Position.y;

        // find closest food
        var foodPos = new Vec2(0, 0);
        var foodExists = false;
        var record = float.MaxValue;
        for (int i = 0; i < food.Count; i++)
        {
            var d = DistanceSquaredTo(food[i].pos);
            if (d > foodDetectionRadiusSq)
                continue;
            
            if (d < record)
            {
                record = d;
                foodPos = food[i].pos;
                foodExists = true;
            }
            if (d < foodEatRadiusSq)
            {
                food[i] = (food[i].pos, food[i].Item2 + 1);
                goto FinishedFoodProcessing;
            }
        }
        FinishedFoodProcessing:

        flockAvg.vel.Subtract(vel);
        flockAvg.vel.SetMag(velocityAlignment);

        flockAvg.pos.Subtract(this);
        flockAvg.pos.SetMag(positionAlignment);

        pathvel.Subtract(this);
        pathvel.SetMag(pathAlignment);

        acc.x = 0;
        acc.y = 0;

        acc.Add(flockAvg.vel);
        acc.Add(flockAvg.pos);
        acc.Add(pathvel);
        acc.AddRandom(randomStrength);

        if(flockAvg.close > 0)
        {
            flockAvg.closePos.Subtract(this);
            flockAvg.closePos.SetMag(-avoidStrength);
            acc.Add(flockAvg.closePos);
        }

        if (foodExists)
            acc.Add(Vec2.OfMagnitude(foodPos - this, foodAttractionStrength));

        /*} end of if(process)*/

        // if boid is too close to edge, steer away from it
        if (x < boids.UpperLeft.x + margin)
            acc.x = Math.Abs(acc.x) * marginSteerStrength;
        if (y < boids.UpperLeft.y + margin)
            acc.y = Math.Abs(acc.y) * marginSteerStrength;

        if (x > boids.UpperLeft.x + boids.boundary.Size.x - margin)
            acc.x = -Math.Abs(acc.x) * marginSteerStrength;
        if (y > boids.UpperLeft.y + boids.boundary.Size.y - margin)
            acc.y = -Math.Abs(acc.y) * marginSteerStrength;

        // first part of euler integration (updating velocity due to accel)
        acc.SetMag(accStrength);
        vel.Multiply(velocityDecay);
        vel.Add(acc);
        vel.Constrain(-maxVel, maxVel, -maxVel, maxVel);

        // if boid is *very* close to edge, move away from it 
        if (x < boids.UpperLeft.x + criticalMargin)
            vel.x = Math.Abs(vel.x);
        if (y < boids.UpperLeft.y + criticalMargin)
            vel.y = Math.Abs(vel.y);

        if (x > boids.UpperLeft.x + boids.boundary.Size.x - criticalMargin)
            vel.x = -Math.Abs(vel.x);
        if (y > boids.UpperLeft.y + boids.boundary.Size.y - criticalMargin)
            vel.y = -Math.Abs(vel.y);

         acc.x *= acc.x * 0.5f;
         acc.y *= acc.y * 0.5f;

        // second part of euler integration (updating position due to velocity and accelleration)
        Add(vel);
        Add(acc);

        // this should not be necessary, but if any boids ever escape, I guess you can enable this...
        /*Constrain(
            boids.UpperLeft.x, boids.UpperLeft.x + boids.boundary.Size.x,
            boids.UpperLeft.y, boids.UpperLeft.y + boids.boundary.Size.y);*/
    }
}
