using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using game_1.Collisions;

namespace game_1.Projectiles;

public class Projectile
{
    // The placeholder art is taller than it is wide, so it points up (-Y) at a
    // rotation of 0. Turning it a quarter turn makes rotation 0 mean "right",
    // which is what Atan2 gives us. A texture drawn pointing right wants 0 here.
    private const float TextureFacingOffset = MathHelper.PiOver2;

    private readonly Texture2D _texture;

    private readonly float _rotation;

    private readonly float _scale;

    public Projectile(Texture2D texture, Vector2 position, Vector2 velocity, int damage, float scale)
    {
        _texture = texture;
        _scale = scale;
        _rotation = MathF.Atan2(velocity.Y, velocity.X) + TextureFacingOffset;

        Position = position;
        Velocity = velocity;
        Damage = damage;
    }

    public Vector2 Position { get; private set; }

    public Vector2 Velocity { get; }

    public int Damage { get; }

    public bool IsAlive { get; private set; } = true;

    // Axis-aligned box around the rotated sprite. BoundingRectangle cannot tilt,
    // so the extents are widened by the rotation instead of using the raw
    // texture size, which would be wrong the moment the shot is not vertical.
    public BoundingRectangle Bounds
    {
        get
        {
            float halfWidth = _texture.Width * _scale / 2f;
            float halfHeight = _texture.Height * _scale / 2f;
            float cos = MathF.Abs(MathF.Cos(_rotation));
            float sin = MathF.Abs(MathF.Sin(_rotation));

            float halfX = halfWidth * cos + halfHeight * sin;
            float halfY = halfWidth * sin + halfHeight * cos;

            return new BoundingRectangle(
                Position.X - halfX,
                Position.Y - halfY,
                halfX * 2f,
                halfY * 2f
            );
        }
    }

    public void Kill() => IsAlive = false;

    public void Update(GameTime gameTime)
    {
        Position += Velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
    }

    public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(
            texture: _texture,
            position: Position,
            sourceRectangle: null,
            color: Color.White,
            rotation: _rotation,
            origin: new Vector2(_texture.Width / 2f, _texture.Height / 2f),
            scale: _scale,
            effects: SpriteEffects.None,
            layerDepth: 0f
        );
    }
}
