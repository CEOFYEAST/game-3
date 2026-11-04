using System;
using Microsoft.Xna.Framework;
using game_1.Projectiles;

namespace game_1.Weapons;

// Fires projectiles. Subclasses vary the shot by speed, damage, count and texture.
public abstract class RangedWeapon : Weapon
{
    // Pixels per second the shot travels.
    protected abstract float ProjectileSpeed { get; }

    protected abstract int ProjectileDamage { get; }

    // Projectiles released per shot; more than one fans out across Spread.
    protected abstract int ProjectileCount { get; }

    // Total angle, in radians, the shot fans across. Ignored when firing one.
    protected virtual float Spread => 0f;

    protected abstract string ProjectileTexture { get; }

    protected virtual float ProjectileScale => 1f;

    // Fires from origin along aimRadians, or returns false while on cooldown.
    public bool TryFire(Vector2 origin, float aimRadians, ProjectileManager projectiles)
    {
        if (!IsReady) return false;

        MarkUsed();

        // A single shot goes straight down the aim; a fan is spread evenly
        // across Spread and centred on it.
        float step = ProjectileCount > 1 ? Spread / (ProjectileCount - 1) : 0f;
        float start = ProjectileCount > 1 ? aimRadians - Spread / 2f : aimRadians;

        for (int i = 0; i < ProjectileCount; i++)
        {
            float angle = start + step * i;
            Vector2 velocity = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * ProjectileSpeed;

            projectiles.Spawn(ProjectileTexture, origin, velocity, ProjectileDamage, ProjectileScale);
        }

        return true;
    }
}
