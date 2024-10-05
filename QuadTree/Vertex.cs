using Godot;

/// <summary>
/// Base class for things to go into quad-trees. Could possibly replaced with just Vector2
/// </summary>
public partial class Vertex
{
    public Vector2 Position { get; set; }
    public Vertex(Vector2 position)
    {
        Position = position;
    }
}
