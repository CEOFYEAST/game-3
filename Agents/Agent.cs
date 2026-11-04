using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using game_1.Collisions;

namespace game_1.Agents;

public abstract class Agent
{
    protected Texture2D _texture;

    protected Agent(Vector2 initialPosition)
    {
        Position = initialPosition;
    }

    // Centre of the sprite, in screen space.
    public Vector2 Position { get; set; }

    // Tint applied when drawing; handy for showing collisions.
    public Color Color { get; set; } = Color.White;

    // Size multiplier applied to the texture when drawing.
    protected abstract float Scale { get; }

    // Half the scaled texture's larger side: how far the sprite can reach from
    // Position. Used to place spawns fully off-screen, not for collisions.
    public float DrawRadius =>
        _texture is null ? 0f : MathF.Max(_texture.Width, _texture.Height) * Scale / 2f;

    // Collision bounds, inscribed in the scaled texture so they stay contained
    // within it on both axes. Derived from Position, so they can never go stale.
    public BoundingCircle Bounds => new(
        Position,
        _texture is null ? 0f : MathF.Min(_texture.Width, _texture.Height) * Scale / 2f
    );

    public abstract void LoadContent(ContentManager content);

    public abstract void Draw(GameTime gameTime, SpriteBatch spriteBatch);
}
