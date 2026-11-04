using Microsoft.Xna.Framework;

namespace game_1.Collisions;

/// <summary>
/// A struct representing circular bounds
/// </summary>
public struct BoundingCircle
{
    public Vector2 Center;

    public float Radius;

    public BoundingCircle(Vector2 center, float radius)
    {
        Center = center;
        Radius = radius;
    }

    public bool CollidesWith(BoundingCircle other)
    {
        return CollisionHelper.Collides(this, other);
    }

    public bool CollidesWith(BoundingRectangle other)
    {
        return CollisionHelper.Collides(this, other);
    }
}
