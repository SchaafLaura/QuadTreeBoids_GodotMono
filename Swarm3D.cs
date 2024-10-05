/*using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
internal class Swarm3D : Node3D
{
    [Export(PropertyHint.Range, "0,5000,1")]
    int amount = 500;

    [Export]
    Texture2D boidTexture;

    Path3D path;

    Octree<Boid3D> boids;
    List<Boid3D> boidList = new List<Boid3D>();
    List<Sprite3D> spriteList = new List<Sprite3D>();

    public override void _Ready()
    {
        
        // get the path the boids should swarm towards
        path = GetNode<Path3D>("../SwarmPath");
        Debug.Print(path.ToString());

        // setup spatial data structure
        boids = new Octree<Boid3D>(new Box(
            new Vector3(500, 500, 500),
            new Vector3(1000, 1000, 1000)));
        
        // spawn some random boids
        var rng = new Random();
        for (int i = 0; i < amount; i++)
        {
            var b = new Boid3D(new Vector3(rng.Next(0, 1000), rng.Next(0, 1000), rng.Next(0, 1000)));
            
            boids.Insert(b);
            boidList.Add(b);

            // make some sprites to display the boids
            var s = new Sprite3D();
            s.Texture2D = boidTexture;
            s.Translate(b.Position);
            s.Scale = new Vector3(150, 150, 150);
            //s.Position = b.Position;
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
            b.Update(boids, path);
            

        // rebuild the quad-tree every frame (yes, this is nessecary)
        boids = new Octree<Boid3D>(new Box(
            new Vector3(500, 500, 500),
            new Vector3(1000, 1000, 1000)));

        for (int i = 0; i < boidList.Count; i++)
        {
            boids.Insert(boidList[i]);
            spriteList[i].Position = boidList[i].Position;
            spriteList[i].Rotation = boidList[i].vel;
            // update sprite positions and rotations
            // spriteList[i].Position = boidList[i].Position;
            // spriteList[i].Rotation = boidList[i].vel.Angle();
        }
    }
}

*/