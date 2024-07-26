using Godot;
using System.Collections.Generic;
using System.Diagnostics;

internal class QuadTree<NodeType> where NodeType : Vec2
{
    public Rectangle boundary { get; private set; }
    public Vec2 UpperLeft { get; private set; }
    int capacity = 10;       // how many vertecies fit in a rect
    List<NodeType> nodes;   // the nodes inside this rect
    bool divided = false;   

    // the four sub-trees this one splits into if the capacity-plus-one-th vertex is added
    QuadTree<NodeType> NE;  
    QuadTree<NodeType> SE;
    QuadTree<NodeType> SW;
    QuadTree<NodeType> NW;

    public QuadTree(Rectangle boundary)
    {
        this.boundary = boundary;
        UpperLeft = boundary.Position - boundary.Size * 0.5f;
        nodes = new List<NodeType>();
    }

    /// <summary>
    /// Inserts a node into the tree or into a appropiate sub-tree, if it goes above its capacity
    /// </summary>
    /// <param name="node">The node to be inserted</param>
    /// <returns>True if insertion worked - false otherwise</returns>
    public bool Insert(NodeType node)
    {
        // don't insert if the node is not inside the boundary
        if (!boundary.Contains(node))
        {
            return false;
        }

        // insert, if there is still room
        if(nodes.Count < capacity)
        {
            nodes.Add(node);
            return true;
        }

        // insert into sub-tree, if there wasn't enough room
        if (!divided)
            Divide();

        if (NE.Insert(node))
            return true;
        else if (SE.Insert(node))
            return true;
        else if (SW.Insert(node))
            return true;
        return NW.Insert(node);
    }

    /// <summary>
    /// Gets all nodes in a circle centered at pos with radius r
    /// </summary>
    /// <param name="pos">Mid-point of the circle</param>
    /// <param name="r">Radius of the circle</param>
    /// <returns>A list of nodes that are inside the circle provided</returns>
    public List<NodeType> Query(Vec2 pos, float r)
    {
        var ret = new List<NodeType>();

        if (!boundary.Contains(pos, r))
            return ret;

        // checking only x should be fine right? floats are pretty much never equal
        for (int i = 0; i < nodes.Count; i++)
            if (nodes[i] != pos && pos.DistanceSquaredTo(nodes[i]) < r * r)
                ret.Add(nodes[i]);

        if (divided)
        {
            ret.AddRange(NE.Query(pos, r));
            ret.AddRange(SE.Query(pos, r));
            ret.AddRange(SW.Query(pos, r));
            ret.AddRange(NW.Query(pos, r));
        }

        return ret;
    }

    /// <summary>
    /// Gets all nodes in a rectangle
    /// </summary>
    /// <param name="r">Rectangle to check nodes for</param>
    /// <returns>A list of nodes contained in the rectangle</returns>
    public List<NodeType> Query(Rectangle r)
    {
        var ret = new List<NodeType>();
        if (!boundary.Intersects(r))
            return ret;

        for(int i = 0; i < nodes.Count; i++)
            if (r.Contains(nodes[i]))
                ret.Add(nodes[i]);

        if (divided)
        {
            ret.AddRange(NE.Query(r));
            ret.AddRange(SE.Query(r));
            ret.AddRange(SW.Query(r));
            ret.AddRange(NW.Query(r));
        }
        return ret;
    }

    private void Divide()
    {
        divided = true;
        NE = new QuadTree<NodeType>(new Rectangle(new Vec2(boundary.Position.x + boundary.Size.x / 4.0f, boundary.Position.y - boundary.Size.y / 4.0f), boundary.Size * 0.5f));
        SE = new QuadTree<NodeType>(new Rectangle(new Vec2(boundary.Position.x + boundary.Size.x / 4.0f, boundary.Position.y + boundary.Size.y / 4.0f), boundary.Size * 0.5f));
        SW = new QuadTree<NodeType>(new Rectangle(new Vec2(boundary.Position.x - boundary.Size.x / 4.0f, boundary.Position.y + boundary.Size.y / 4.0f), boundary.Size * 0.5f));
        NW = new QuadTree<NodeType>(new Rectangle(new Vec2(boundary.Position.x - boundary.Size.x / 4.0f, boundary.Position.y - boundary.Size.y / 4.0f), boundary.Size * 0.5f));
    }
}
