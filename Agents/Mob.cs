using System;
using Microsoft.Xna.Framework;

namespace game_1.Agents;

public abstract class Mob : Agent
{
    // Health is passed up rather than read from a virtual member, so a subclass
    // cannot be asked for it before its own fields are ready.
    protected Mob(Vector2 initialPosition, int maxHealth) : base(initialPosition)
    {
        MaxHealth = maxHealth;
        Health = maxHealth;
    }

    // Pixels travelled per second, so movement is independent of framerate.
    protected abstract float Speed { get; }

    public int MaxHealth { get; }

    public int Health { get; private set; }

    public bool IsAlive => Health > 0;

    // Everything currently dies in one hit, but tougher mobs only need to be
    // built with more health for this to start mattering.
    public void TakeDamage(int amount)
    {
        if (amount <= 0) return;

        Health = Math.Max(0, Health - amount);
    }

    public void Kill() => Health = 0;

    // Mobs walk themselves towards the target; the manager only says where it is.
    public virtual void Update(GameTime gameTime, Vector2 target)
    {
        float step = Speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
        Vector2 toTarget = target - Position;

        // Stop exactly on the target rather than overshooting and jittering around it.
        if (toTarget.LengthSquared() <= step * step)
        {
            Position = target;
            return;
        }

        Position += Vector2.Normalize(toTarget) * step;
    }
}
