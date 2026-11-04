namespace game_1.Weapons;

// A plain sidearm: one fast round per press.
public class Pistol : RangedWeapon
{
    protected override float Cooldown => 0.25f;

    protected override float ProjectileSpeed => 500f;

    protected override int ProjectileDamage => 1;

    protected override int ProjectileCount => 1;

    protected override string ProjectileTexture => "ProjectilePlaceholder";
}
