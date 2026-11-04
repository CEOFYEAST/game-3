using game_1.Collisions;

namespace game_1.Weapons;

// One swing: the area it covers and what it does to anything inside.
public readonly struct MeleeStrike
{
    public readonly BoundingCircle Area;

    public readonly int Damage;

    public MeleeStrike(BoundingCircle area, int damage)
    {
        Area = area;
        Damage = damage;
    }
}
