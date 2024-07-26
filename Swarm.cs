using Godot;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

public class Swarm : Node2D
{
    [Export(PropertyHint.Range, "0,5000,1")]
    int amount = 500;

    [Export]
    Texture boidTexture;

    [Export]
    Texture foodTexture;

    [Export]
    NodePath rectPath;
    ColorRect rect;
    Rectangle boundary;


    [Export]
    NodePath pathPath;
    Path2D path;

    QuadTree<Boid> boids;
    List<Boid> boidList             = new List<Boid>();
    List<Sprite> spriteList         = new List<Sprite>();
    List<(Vec2 pos, int t)> food    = new List<(Vec2, int)>();
    List<Sprite> foodSpireList      = new List<Sprite>();

    //float w = 1920;
    //float h = 1080;
    Vec2 pos;
    Vec2 size;

    public override void _Ready()
    {
        // get the path the boids should swarm towards
        path = GetNode<Path2D>(pathPath);

        rect = GetNode<ColorRect>(rectPath);
        pos = new Vec2(rect.RectGlobalPosition.x, rect.RectGlobalPosition.y);
        size = new Vec2(rect.RectSize.x, rect.RectSize.y);

        boundary = new Rectangle(pos + size * 0.5f, size);
        // setup spatial data structure
        boids = new QuadTree<Boid>(boundary);

        // spawn some random boids
        var rng = new Random();
        for(int i = 0; i < amount; i++)
        {
            var b = new Boid(
                rng.Next((int)pos.x, (int)(pos.x + size.x)),
                rng.Next((int)pos.y, (int)(pos.y + size.y)));
            boids.Insert(b);
            boidList.Add(b);

            // make some sprites to display the boids
            var s = new Sprite();
            s.Texture = boidTexture;

            s.Position = b.ToVector2();
            spriteList.Add(s);
            s.ZIndex++;
            AddChild(s);
        }
    }

    public override void _PhysicsProcess(float delta)
    {
        // update and rebuilding needs two loops
        // one for updating, which uses the current quad-tree for efficient searching of the space
        // one for rebuilding the tree with the new positions and updating sprites

        var clonedList = new ConcurrentBag<Boid>();
        var clonedListLock = new object();
        Parallel.For(0, boidList.Count, () => new List<Boid>(), 
        (i, _, localList) =>
        {
            var clone = boidList[i]; // should probably .Clone() but seems to work fine without
            clone.Update(boids, path, food);
            localList.Add(clone); // Add to thread-local list
            return localList;
        },
        (localList) =>
        {
            foreach (var item in localList)
                clonedList.Add(item); // Add each item to ConcurrentBag
        });

        boidList.Clear();
        boidList.AddRange(clonedList);

        for (int i = food.Count - 1; i >= 0 ; i--)
        {
            food[i] = (food[i].pos, food[i].t + 1);
            if (food[i].t > 3000)
            {
                food.RemoveAt(i);
                RemoveChild(foodSpireList[i]);
                foodSpireList.RemoveAt(i);
            }
            
        }
        // rebuild the quad-tree every frame (yes, this is nessecary)
        boids = new QuadTree<Boid>(boundary);
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
        if (input is InputEventMouseButton btnEvent)
            if (btnEvent.Pressed && btnEvent.ButtonIndex == (int)ButtonList.Left)
                AddFood(btnEvent.Position);

        base._UnhandledInput(input);
    }

    private void AddFood(Vector2 pos)
    {
        food.Add((pos.ToVec2(), 0));
        var s = new Sprite();
        s.Texture = foodTexture;
        s.Position = pos;
        AddChild(s);
        foodSpireList.Add(s);
    }

}
