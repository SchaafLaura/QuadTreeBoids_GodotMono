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

    public override void _Ready()
    {
        // get the path the boids should swarm towards
        path = GetNode<Path2D>("../SwarmPath");
        Debug.Print(path.ToString());

        // setup spatial data structure
        boids = new QuadTree<Boid>(new Rectangle(
            new Vector2(500, 500),
            new Vector2(1000, 1000)));

        // spawn some random boids
        var rng = new Random();
        for(int i = 0; i < amount; i++)
        {
            var b = new Boid(new Vector2(rng.Next(0, 1000), rng.Next(0, 1000)));
            boids.Insert(b);
            boidList.Add(b);

            // make some sprites to display the boids
            var s = new Sprite();
            s.Texture = boidTexture;
            s.Position = b.Position;
            spriteList.Add(s);
            AddChild(s);
        }
    }

    public override void _PhysicsProcess(float delta)
    {
        // update and rebuilding needs two loops
        // one for updating, which uses the current quad-tree for efficient searching of the space
        // one for rebuilding the tree and updating sprites

        // update each boid
        foreach (var b in boidList)
            b.Update(boids, path);

        // rebuild the quad-tree every frame (yes, this is nessecary)
        boids = new QuadTree<Boid>(new Rectangle(
            new Vector2(500, 500),
            new Vector2(1000, 1000)));

        for (int i = 0; i < boidList.Count; i++)
        {
            boids.Insert(boidList[i]);

            // update sprite positions and rotations
            spriteList[i].Position = boidList[i].Position;
            spriteList[i].Rotation = boidList[i].vel.Angle();
        }
    }
}
