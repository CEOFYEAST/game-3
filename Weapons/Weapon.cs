using Microsoft.Xna.Framework;

namespace game_1.Weapons;

public abstract class Weapon
{
    // Seconds that must pass between two uses of this weapon.
    protected abstract float Cooldown { get; }

    // Starts high so a freshly equipped weapon can be used straight away.
    private float _timeSinceUse = float.MaxValue;

    public bool IsReady => _timeSinceUse >= Cooldown;

    public virtual void Update(GameTime gameTime)
    {
        if (_timeSinceUse < float.MaxValue)
            _timeSinceUse += (float)gameTime.ElapsedGameTime.TotalSeconds;
    }

    protected void MarkUsed() => _timeSinceUse = 0f;
}
