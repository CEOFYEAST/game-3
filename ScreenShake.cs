using System;
using Microsoft.Xna.Framework;

namespace game_1;

// Jolts the whole picture by handing SpriteBatch.Begin() a transform. Only what
// is drawn moves, so collisions and aiming carry on in untouched screen space.
public class ScreenShake
{
    // Pixels of strength lost per second, so a bigger jolt also lasts longer.
    private const float FadeRate = 40f;

    private readonly Random _random = new();

    // How far the picture can currently be thrown from its resting place, in pixels.
    private float _strength;

    private Vector2 _offset;

    public Matrix Transform => Matrix.CreateTranslation(_offset.X, _offset.Y, 0f);

    // Keeps the stronger of the two rather than adding them, so a pile of kills
    // on one frame cannot throw the screen further than the biggest of them.
    public void Add(float strength) => _strength = MathF.Max(_strength, strength);

    // Settles the picture so a new run does not open mid-jolt.
    public void Reset()
    {
        _strength = 0f;
        _offset = Vector2.Zero;
    }

    public void Update(GameTime gameTime)
    {
        _strength = MathF.Max(0f, _strength - FadeRate * (float)gameTime.ElapsedGameTime.TotalSeconds);

        float angle = (float)_random.NextDouble() * MathHelper.TwoPi;
        _offset = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * _strength;
    }
}
