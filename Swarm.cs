using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;

public class Swarm : Node2D
{
    [Export(PropertyHint.Range, "0,5000,1")]
    int amount = 500;

    [Export]
    Texture boidTexture;

    Path2D path;

    QuadTree<Boid> boids;
    List<Boid> boidList = new List<Boid>();
    List<Sprite> spriteList = new List<Sprite>();
    List<(Vec2 pos, int t)> food = new List<(Vec2, int)>();

    public override void _Ready()
    {
        // get the path the boids should swarm towards
        path = GetNode<Path2D>("../SwarmPath");
        Debug.Print(path.ToString());

        // setup spatial data structure
        boids = new QuadTree<Boid>(new Rectangle(
            new Vec2(500, 500),
            new Vec2(1000, 1000)));

        // spawn some random boids
        var rng = new Random();
        for(int i = 0; i < amount; i++)
        {
            var b = new Boid(rng.Next(0, 1000), rng.Next(0, 1000));
            boids.Insert(b);
            boidList.Add(b);

            // make some sprites to display the boids
            var s = new Sprite();
            s.Texture = boidTexture;
            s.Position = b.ToVector2();
            spriteList.Add(s);
            AddChild(s);
        }
    }

    public override void _PhysicsProcess(float delta)
    {
        // update and rebuilding needs two loops
        // one for updating, which uses the current quad-tree for efficient searching of the space
        // one for rebuilding the tree with the new positions and updating sprites

        // update each boid
        foreach (var b in boidList)
            b.Update(boids, path, food);

        for (int i = food.Count - 1; i >= 0 ; i--)
        {
            food[i] = (food[i].pos, food[i].t + 1);
            if (food[i].t > 200)
                food.RemoveAt(i);
        }

        // rebuild the quad-tree every frame (yes, this is nessecary)
        boids = new QuadTree<Boid>(new Rectangle(
            new Vec2(500, 500),
            new Vec2(1000, 1000)));

        for (int i = 0; i < boidList.Count; i++)
        {
            //Debug.Print(boidList[i].ToString());
            boids.Insert(boidList[i]);

            // update sprite positions and rotations
            spriteList[i].Position = boidList[i].ToVector2();
            spriteList[i].Rotation = boidList[i].vel.Angle();
        }
    }
    public override void _UnhandledInput(InputEvent input)
    {
        if(input is InputEventMouseButton btnEvent)
            if(btnEvent.ButtonIndex == (int) ButtonList.Left)
                food.Add((btnEvent.Position.ToVec2(), 0));

        base._UnhandledInput(input);
    }
}
