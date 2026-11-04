using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using game_1.Projectiles;
using game_1.Weapons;

namespace game_1.Agents;

public class Player : Agent
{
    // Enemy touches a player survives; the third one kills them. Public so the
    // health display knows how many hearts to lay out.
    public const int MaxTouches = 3;

    // Extra clearance so a shot starts clear of the player's own sprite.
    private const float MuzzleGap = 4f;

    // The placeholder art is a circle, so it looks the same whichever way it
    // is turned. Directional art pointing up would want MathHelper.PiOver2 here.
    private const float TextureFacingOffset = 0f;

    public Player(Vector2 initialPosition) : base(initialPosition) { }

    protected override float Scale => 0.5f;

    // Touches left before the player dies.
    public int RemainingTouches { get; private set; } = MaxTouches;

    public bool IsAlive => RemainingTouches > 0;

    // Where the player is aiming, in radians. 0 points right, matching Atan2.
    public float Rotation { get; private set; }

    // Always carried, so there is something to swing when a mob gets too close.
    public MeleeWeapon Melee { get; } = new();

    public RangedWeapon EquippedWeapon { get; private set; } = new Pistol();

    // The point shots leave from: just past the sprite, along the aim.
    public Vector2 Muzzle => Position + Facing * (DrawRadius + MuzzleGap);

    private Vector2 Facing => new(MathF.Cos(Rotation), MathF.Sin(Rotation));

    public void Equip(RangedWeapon weapon) => EquippedWeapon = weapon;

    // Turns to face a point; the mouse only ever rotates the player.
    public void AimAt(Vector2 target)
    {
        Vector2 toTarget = target - Position;

        // Hold the last angle when the mouse sits exactly on the player,
        // rather than snapping to the right.
        if (toTarget == Vector2.Zero) return;

        Rotation = MathF.Atan2(toTarget.Y, toTarget.X);
    }

    // Ticks weapon cooldowns down.
    public void Update(GameTime gameTime)
    {
        Melee.Update(gameTime);
        EquippedWeapon.Update(gameTime);
    }

    public bool TryFire(ProjectileManager projectiles) =>
        EquippedWeapon.TryFire(Muzzle, Rotation, projectiles);

    public bool TryMelee(out MeleeStrike strike) =>
        Melee.TryStrike(Position, Rotation, out strike);

    public void Touch()
    {
        if (RemainingTouches > 0) RemainingTouches--;
    }

    // Puts the player back to full health for a fresh run.
    public void Revive()
    {
        RemainingTouches = MaxTouches;
        Melee.ClearStrike();
    }

    public override void LoadContent(ContentManager content)
    {
        _texture = content.Load<Texture2D>("PlayerPlaceholder");
        Melee.LoadContent(content);
    }

    public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        // Under the player, so the swing never hides who is swinging it. Only
        // while alive: once the run ends nothing ticks the swing's timer, so a
        // dying blow would otherwise sit on screen behind the game over prompt.
        if (IsAlive) Melee.Draw(gameTime, spriteBatch);

        spriteBatch.Draw(
            texture: _texture,
            position: Position,
            sourceRectangle: null,
            color: Color,
            rotation: Rotation + TextureFacingOffset,
            origin: new Vector2(_texture.Width / 2f, _texture.Height / 2f),
            scale: Scale,
            effects: SpriteEffects.None,
            layerDepth: 0f
        );
    }
}
