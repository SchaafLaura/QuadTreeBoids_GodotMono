using Godot;
internal class Box
{
    public Vector3 Position { get; set; }
    public Vector3 Size { get; set; }

    private Vector3 halfSize;

    public Box(Vector3 position, Vector3 size)
    {
        Position = position;
        Size = size;
        halfSize = size * 0.5f;
    }

    public bool Contains(Vector3 p, float r)
    {
        return
            p.x >= Position.x - halfSize.x - r &&
            p.x <= Position.x + halfSize.x + r &&

            p.y >= Position.y - halfSize.y - r &&
            p.y <= Position.y + halfSize.y + r &&

            p.z >= Position.z - halfSize.z - r &&
            p.z <= Position.z + halfSize.z + r;
    }

    public bool Contains(Vector3 p)
    {
        return
            p.x >= Position.x - halfSize.x &&
            p.x <= Position.x + halfSize.x &&

            p.y >= Position.y - halfSize.y &&
            p.y <= Position.y + halfSize.y &&

            p.z >= Position.z - halfSize.z &&
            p.z <= Position.z + halfSize.z;
    }

}

