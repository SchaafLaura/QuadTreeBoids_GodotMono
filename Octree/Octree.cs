using Godot;
using System.Collections.Generic;
internal class Octree<NodeType> where NodeType : Vertex3D
{
    public Box boundary { get; private set; }
    int capacity = 1;       // how many vertecies fit in a rect
    List<NodeType> nodes;   // the nodes inside this rect
    bool divided = false;

    // the eight sub-trees this one splits into if the capacity-plus-one-th vertex is added
    // upper four
    Octree<NodeType> UNE;
    Octree<NodeType> USE;
    Octree<NodeType> USW;
    Octree<NodeType> UNW;
    // lower four
    Octree<NodeType> DNE;
    Octree<NodeType> DSE;
    Octree<NodeType> DSW;
    Octree<NodeType> DNW;

    public Octree(Box boundary)
    {
        this.boundary = boundary;
        nodes = new List<NodeType>();
    }

    public List<NodeType> Query(Vector3 pos, float r)
    {
        var ret = new List<NodeType>();

        if (!boundary.Contains(pos, r))
            return ret;

        for (int i = 0; i < nodes.Count; i++)
            if (pos.DistanceSquaredTo(nodes[i].Position) < r * r)
                ret.Add(nodes[i]);

        if (divided)
        {
            ret.AddRange(UNE.Query(pos, r));
            ret.AddRange(USE.Query(pos, r));
            ret.AddRange(USW.Query(pos, r));
            ret.AddRange(UNW.Query(pos, r));
            ret.AddRange(DNE.Query(pos, r));
            ret.AddRange(DSE.Query(pos, r));
            ret.AddRange(DSW.Query(pos, r));
            ret.AddRange(DNW.Query(pos, r));
        }
        return ret;
    }

    public bool Insert(NodeType node)
    {
        // don't insert if the node is not inside the boundary
        if (!boundary.Contains(node.Position))
            return false;

        // insert, if there is still room
        if (nodes.Count < capacity)
        {
            nodes.Add(node);
            return true;
        }

        // insert into sub-tree, if there wasn't enough room
        if (!divided)
            Divide();

        if (UNE.Insert(node))
            return true;
        else if (USE.Insert(node))
            return true;
        else if (USW.Insert(node))
            return true;
        else if (UNW.Insert(node))
            return true;
        else if (DNE.Insert(node))
            return true;
        else if (DSE.Insert(node))
            return true;
        else if (DSW.Insert(node))
            return true;
        return DNW.Insert(node);
    }

    private void Divide()
    {
        divided = true;

        // center point
        var x = boundary.Position.X;
        var y = boundary.Position.Y;
        var z = boundary.Position.Z;

        // half of size
        var hsx = boundary.Size.X * 0.5f;
        var hsy = boundary.Size.Y * 0.5f;
        var hsz = boundary.Size.Z * 0.5f;
        var halfSize = new Vector3(hsx, hsy, hsz);

        // quarder size for offsets
        var qsx = boundary.Size.X * 0.25f;
        var qsy = boundary.Size.Y * 0.25f;
        var qsz = boundary.Size.Z * 0.25f;

        divided = true;
        UNE = new Octree<NodeType>(new Box(new Vector3(x + qsx, y + qsy, z + qsz), halfSize));
        USE = new Octree<NodeType>(new Box(new Vector3(x - qsx, y + qsy, z + qsz), halfSize));
        USW = new Octree<NodeType>(new Box(new Vector3(x + qsx, y - qsy, z + qsz), halfSize));
        UNW = new Octree<NodeType>(new Box(new Vector3(x - qsx, y - qsy, z + qsz), halfSize));
        DNE = new Octree<NodeType>(new Box(new Vector3(x + qsx, y + qsy, z - qsz), halfSize));
        DSE = new Octree<NodeType>(new Box(new Vector3(x - qsx, y + qsy, z - qsz), halfSize));
        DSW = new Octree<NodeType>(new Box(new Vector3(x + qsx, y - qsy, z - qsz), halfSize));
        DNW = new Octree<NodeType>(new Box(new Vector3(x - qsx, y - qsy, z - qsz), halfSize));
    }
}
