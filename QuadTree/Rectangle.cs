using Godot;

/// <summary>
/// Axis aligned Rectangle with center at .Position and width/height .Size
/// </summary>
public class Rectangle
{
    public Vector2 Position { get; private set; } // center of rect
    public Vector2 Size { get; private set; }     // width and height

    private Vector2 halfSize;                     // half the width and height (for efficiency)

    /// <summary>
    /// Constructor of the Rectangle class
    /// </summary>
    /// <param name="Position">Center point of the rectangle</param>
    /// <param name="Size">Width and height of the rectangle</param>
    public Rectangle(Vector2 Position, Vector2 Size)
    {
        this.Position = Position;
        this.Size = Size;
        this.halfSize = Size * 0.5f;
    }

    /// <summary>
    /// Checks if this Rectangle intersects other Rectangle
    /// </summary>
    /// <param name="other">The other rectangle to check intersection against</param>
    /// <returns>True if the rectangles intersect - false otherwise</returns>
    public bool Intersects(Rectangle other)
    {
        return !(
          other.Position.x - other.Size.x > Position.x + Size.x ||
          other.Position.x + other.Size.x < Position.x - Size.x ||
          other.Position.y - other.Size.y > Position.y + Size.y ||
          other.Position.y + other.Size.y < Position.y - Size.y);
    }

    /// <summary>
    /// Checks if the rectangle possibly intersects a circle with radius r centered at position p.
    /// The check is just an approximation.
    /// </summary>
    /// <param name="p">The position of the cirlce to check intersection with</param>
    /// <param name="r">The radius of the circle to check intersection with</param>
    /// <returns>True if this rectangle could intersect the circle</returns>
    public bool Contains(Vector2 p, float r)
    {
        return
            p.x >= Position.x - halfSize.x - r &&
            p.x <= Position.x + halfSize.x + r &&
            p.y >= Position.y - halfSize.y - r &&
            p.y <= Position.y + halfSize.y + r;
    }

    /// <summary>
    /// Checks if the rectangle contains a point
    /// </summary>
    /// <param name="p">The point to check against</param>
    /// <returns>True if this rectangle contains the point - false otherwise</returns>
    public bool Contains(Vector2 p)
    {
        return
            p.x >= Position.x - halfSize.x &&
            p.x <= Position.x + halfSize.x &&
            p.y >= Position.y - halfSize.y &&
            p.y <= Position.y + halfSize.y;
    }
}
