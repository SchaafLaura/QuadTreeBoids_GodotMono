using Godot;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class Swarm : Node2D
{
    [Export(PropertyHint.Range, "0,5000,1")]
    int amount = 500;

    [Export]
    Texture2D boidTexture;

    [Export]
    Texture2D foodTexture;

    [Export]
    NodePath rectPath;
    ColorRect rect;
    Rectangle boundary;

    [Export]
    NodePath colliderPath;
    Vector2[] colliderPolygon;
    Transform3D colliderTransform;

    [Export]
    NodePath pathPath;
    Path2D path;

    [Export]
    ShaderMaterial material;

    QuadTree<Boid> boids;
    List<Boid> boidList = [];
    List<Sprite2D> spriteList = [];
    List<(Vec2 pos, int t)> food = [];
    List<Sprite2D> foodSpriteList = [];

    Vec2 pos;
    Vec2 size;

    public override void _Ready()
    {
        // get the path the boids should swarm towards
        if (pathPath != null)
        {
            path = GetNode<Path2D>(pathPath);
            if (path != null)
            {
                var pathPoints = path.Curve.GetBakedPoints();

                for (int i = 0; i < pathPoints.Length; i++)
                    pathPoints[i] = path.ToGlobal(pathPoints[i]);

                path = new Path2D();
                path.Curve = new Curve2D();
                foreach (var p in pathPoints)
                    path.Curve.AddPoint(p);
            }
        }


        if (colliderPath != null)
        {
            var c = GetNode<Path2D>(colliderPath);
            if (c != null)
            {
                colliderPolygon = c.Curve.GetBakedPoints();

                for (int i = 0; i < colliderPolygon.Length; i++)
                    colliderPolygon[i] = c.ToGlobal(colliderPolygon[i]);
            }
        }

        // get the rect defining the position and size of the area where the BOIDs live
        rect = GetNode<ColorRect>(rectPath);

        pos = new Vec2(rect.GlobalPosition.X, rect.GlobalPosition.Y);
        size = new Vec2(rect.Size.X, rect.Size.Y);

        // keep the bounding rect because we need to rebuild the quadtree a buncha times
        boundary = new Rectangle(pos + size * 0.5f, size);

        // setup spatial data structure
        boids = new QuadTree<Boid>(boundary);

        // spawn some random boids
        var rng = new Random();
        for (int i = 0; i < amount; i++)
        {
            var b = new Boid(
                rng.Next((int)pos.x, (int)(pos.x + size.x)),
                rng.Next((int)pos.y, (int)(pos.y + size.y)));

            if (colliderPolygon != null && Geometry2D.IsPointInPolygon(b.ToVector2(), colliderPolygon))
                continue;

            boids.Insert(b);
            boidList.Add(b);

            // make some sprites to display the boids
            var s = new Sprite2D();
            s.Texture = boidTexture;
            if (material != null)
                s.Material = material;

            s.Position = b.ToVector2();
            int amt = 100;
            s.Modulate = new Color(
                rng.Next(255 - amt, 255 + amt) / 255.0f,
                rng.Next(255 - amt, 255 + amt) / 255.0f,
                rng.Next(255 - amt, 255 + amt) / 255.0f);
            spriteList.Add(s);

            // increase z index so boids get drawn above food
            s.ZIndex++;
            AddChild(s);
        }

        int id = 0;
        foreach (var b in boidList)
            b.ID = id++;

        rect.GuiInput += HandleRectClick;
    }

    public override void _PhysicsProcess(double delta)
    {
        // update and rebuilding needs two loops
        // one for updating, which uses the current quad-tree for efficient searching of the space
        // one for rebuilding the tree with the new positions and updating sprites

        var threadSafeBag = new ConcurrentBag<Boid>();
        Parallel.For(0, boidList.Count, () => new List<Boid>(),
        (i, _, localList) =>
        {
            var boid = boidList[i];
            boid.Update(boids, path, food, colliderPolygon);
            localList.Add(boid);
            return localList;
        },
        (localList) =>
        {
            foreach (var boid in localList)
                threadSafeBag.Add(boid); // Aggregate all boids that have been updated
        });

        boidList.Clear();
        boidList.AddRange(threadSafeBag);

        // rebuild the quad-tree every frame (yes, this is nessecary)
        boids = new QuadTree<Boid>(boundary);
        for (int i = 0; i < boidList.Count; i++)
        {
            boids.Insert(boidList[i]);
            // update sprite positions and rotations
            spriteList[boidList[i].ID].Position = boidList[i].ToVector2();
            var lerpFactor = 10f;
            var lerpAmount = Mathf.Clamp(lerpFactor * (float)delta, 0, 1);
            spriteList[boidList[i].ID].Rotation = Mathf.LerpAngle(spriteList[boidList[i].ID].Rotation, boidList[i].vel.Angle(), lerpAmount);
        }

        // remove foob that has been monched on enough
        for (int i = food.Count - 1; i >= 0; i--)
        {
            food[i] = (food[i].pos, food[i].t + 1);
            if (food[i].t > 3000)
            {
                food.RemoveAt(i);

                RemoveChild(foodSpriteList[i]);
                foodSpriteList.RemoveAt(i);
            }
        }
    }
    public void HandleRectClick(InputEvent input)
    {
        if (input is InputEventMouseButton btnEvent)
            if (btnEvent.Pressed && btnEvent.ButtonIndex == MouseButton.Left)
                AddFood(btnEvent.Position);

        //base._UnhandledInput(input);
    }

    private void AddFood(Vector2 pos)
    {
        food.Add((pos.ToVec2() + rect.Position.ToVec2(), 0));
        var s = new Sprite2D();
        s.Texture = foodTexture;
        s.Rotate((float)(Util.rng.NextDouble() * 6.28));
        s.Position = pos + rect.Position;
        AddChild(s);
        foodSpriteList.Add(s);
    }
}
