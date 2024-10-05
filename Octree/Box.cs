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
            p.X >= Position.X - halfSize.X - r &&
            p.X <= Position.X + halfSize.X + r &&

            p.Y >= Position.Y - halfSize.Y - r &&
            p.Y <= Position.Y + halfSize.Y + r &&

            p.Z >= Position.Z - halfSize.Z - r &&
            p.Z <= Position.Z + halfSize.Z + r;
    }

    public bool Contains(Vector3 p)
    {
        return
            p.X >= Position.X - halfSize.X &&
            p.X <= Position.X + halfSize.X &&

            p.Y >= Position.Y - halfSize.Y &&
            p.Y <= Position.Y + halfSize.Y &&

            p.Z >= Position.Z - halfSize.Z &&
            p.Z <= Position.Z + halfSize.Z;
    }

}

