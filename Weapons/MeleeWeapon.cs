using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using game_1.Collisions;

namespace game_1.Weapons;

// A short swing in front of the wielder. Always available, so it works as a
// last resort when something has closed the distance.
public class MeleeWeapon : Weapon
{
    // Seconds a swing stays on screen. Long enough to read, and over well
    // before the weapon comes off cooldown, so the two never look tangled.
    private const float StrikeDuration = 0.15f;

    protected override float Cooldown => 0.4f;

    // How far ahead of the wielder the centre of the strike sits.
    protected virtual float Reach => 40f;

    // How wide an area the strike covers.
    protected virtual float Radius => 26f;

    protected virtual int Damage => 1;

    // The art shown over the area a swing covers. It is a circle drawn out to
    // the edges of a square image, so it maps straight onto the strike bounds.
    protected virtual string StrikeTexture => "MeleePlaceholder";

    private Texture2D _texture;

    // Where the last swing landed, and how long ago. Starting the age at the
    // full duration means a weapon that has never swung draws nothing.
    private BoundingCircle _lastArea;

    private float _strikeAge = StrikeDuration;

    // True while the last swing is still worth drawing.
    private bool IsStrikeShowing => _strikeAge < StrikeDuration;

    public void LoadContent(ContentManager content)
    {
        _texture = content.Load<Texture2D>(StrikeTexture);
    }

    // Returns the area the swing covers, or false while still on cooldown.
    public bool TryStrike(Vector2 origin, float aimRadians, out MeleeStrike strike)
    {
        if (!IsReady)
        {
            strike = default;
            return false;
        }

        MarkUsed();

        Vector2 direction = new(MathF.Cos(aimRadians), MathF.Sin(aimRadians));
        strike = new MeleeStrike(new BoundingCircle(origin + direction * Reach, Radius), Damage);

        // The swing is drawn over exactly the area it hits, so what the player
        // sees and what the mobs are checked against cannot drift apart.
        _lastArea = strike.Area;
        _strikeAge = 0f;

        return true;
    }

    // Drops a swing still on screen, so a new run does not open with the last
    // one left over from the previous.
    public void ClearStrike() => _strikeAge = StrikeDuration;

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        if (IsStrikeShowing)
            _strikeAge += (float)gameTime.ElapsedGameTime.TotalSeconds;
    }

    public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        if (_texture is null || !IsStrikeShowing) return;

        // Fades out over its life, so the swing reads as a moment passing
        // rather than a sprite that blinks off.
        float fade = 1f - _strikeAge / StrikeDuration;

        // The art is square and the circle fills it, so a single scale puts the
        // drawn circle on top of the strike's bounds.
        float scale = _lastArea.Radius * 2f / _texture.Width;

        spriteBatch.Draw(
            texture: _texture,
            position: _lastArea.Center,
            sourceRectangle: null,
            color: Color.White * fade,
            // A circle looks the same whichever way it is turned; art with a
            // point to it would want the aim angle here instead.
            rotation: 0f,
            origin: new Vector2(_texture.Width / 2f, _texture.Height / 2f),
            scale: scale,
            effects: SpriteEffects.None,
            layerDepth: 0f
        );
    }
}
