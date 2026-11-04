using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using game_1.Agents;
using game_1.Audio;
using game_1.Projectiles;
using game_1.Weapons;

namespace game_1;

public class TopDownShooter : Game
{
    // The title screen, in the order it reads: what the player is trying to
    // do, the two things they can do about it, and how to begin.
    private static readonly string[] StartMessages =
    [
        "Survive as long as you can",
        "Space - shoot",
        "Q - melee",
        "Press Enter to start"
    ];

    private const string GameOverMessage = "Game over";

    private const string RestartMessage = "Enter to play again, Esc to exit";

    // Vertical space between a prompt and the player sprite under it.
    private const float PromptGap = 24f;

    // Vertical space between two lines of the same prompt.
    private const float LineGap = 6f;

    // Space between the top of the screen and the run timer under it.
    private const float TimerMargin = 16f;

    // Space between the top left corner and the row of hearts in it.
    private const float HeartMargin = 16f;

    // Space between one heart and the next.
    private const float HeartGap = 6f;

    // How tall each heart is drawn, in pixels.
    private const float HeartHeight = 32f;

    // What is left of a heart once it has been spent. Lost hearts stay in
    // place at this strength, so the row shows how much health there was as
    // well as how much is left.
    private const float LostHeartFade = 0.2f;

    private GraphicsDeviceManager _graphics;

    private SpriteBatch _spriteBatch;

    private SpriteFont _font;

    private Texture2D _heartTexture;

    private SoundBank _sounds;

    private Player _player;

    private MobManager _mobManager;

    private ProjectileManager _projectileManager;

    private GameState _state = GameState.Start;

    // The prompt on screen, a line at a time, each with the position it was
    // laid out at. Empty while a run is in progress.
    private readonly List<(string Text, Vector2 Position)> _promptLines = [];

    // How long the current run has lasted, in seconds.
    private float _runTime;

    // The longest any run has lasted since the game was launched.
    private float _bestTime;

    // Kept so firing and swinging trigger on the press, not every frame held.
    private KeyboardState _previousKeyboard;

    public TopDownShooter()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        _player = new(
            GraphicsDevice.Viewport.Bounds.Center.ToVector2()
        );
        _mobManager = new(GraphicsDevice);
        _projectileManager = new();
        _sounds = new();

        // Anything already held as the game opens - the Enter that launched it
        // from a terminal, say - counts as held, not as a fresh press, so the
        // title screen is not skipped before it has been read.
        _previousKeyboard = Keyboard.GetState();

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _font = Content.Load<SpriteFont>("Bangers");
        _heartTexture = Content.Load<Texture2D>("HeartPixelArt");
        _sounds.LoadContent(Content);
        _player.LoadContent(Content);
        _mobManager.LoadContent(Content);
        _projectileManager.LoadContent(Content);

        // Laying out the title screen needs the font and the player texture.
        ShowPrompt(StartMessages);
    }

    protected override void Update(GameTime gameTime)
    {
        KeyboardState keyboard = Keyboard.GetState();

        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || keyboard.IsKeyDown(Keys.Escape))
            Exit();

        switch (_state)
        {
            case GameState.Start:
                if (WasJustPressed(keyboard, Keys.Enter)) StartRun();
                break;

            case GameState.Playing:
                UpdatePlaying(gameTime, keyboard);
                break;

            case GameState.GameOver:
                // Another run can beat the time just set; Escape is handled above.
                if (WasJustPressed(keyboard, Keys.Enter)) StartRun();
                break;
        }

        _previousKeyboard = keyboard;

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        _spriteBatch.Begin();

        if (_state == GameState.Playing)
        {
            _mobManager.Draw(gameTime, _spriteBatch);
            _projectileManager.Draw(gameTime, _spriteBatch);
            DrawRunTimer();
            DrawHealth();
        }
        else
        {
            foreach ((string text, Vector2 position) in _promptLines)
                _spriteBatch.DrawString(_font, text, position, Color.White);
        }

        // The player is on screen in every state.
        _player.Draw(gameTime, _spriteBatch);

        _spriteBatch.End();

        base.Draw(gameTime);
    }

    private void UpdatePlaying(GameTime gameTime, KeyboardState keyboard)
    {
        // Counted before the frame is resolved, so the frame a player dies on
        // still counts towards the time they survived.
        _runTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

        _player.AimAt(Mouse.GetState().Position.ToVector2());
        _player.Update(gameTime);

        HandleWeapons(keyboard);
        ResolveProjectileHits();

        // Settle contacts before the manager moves and prunes, so a mob killed
        // here is gone from the list before it can be drawn again.
        foreach (Mob mob in _mobManager.Mobs)
        {
            // Skip anything already killed this frame, so cutting a mob down as
            // it closes in does not still cost a life.
            if (!mob.IsAlive || !mob.Bounds.CollidesWith(_player.Bounds)) continue;

            mob.Kill();
            _sounds.PlayEnemyDeath();

            _player.Touch();
            _sounds.PlayDamage();
        }

        if (!_player.IsAlive)
        {
            FinishRun();
            return;
        }

        _mobManager.Update(gameTime, _player.Position, _runTime);
        _projectileManager.Update(gameTime, GraphicsDevice.Viewport.Bounds);
    }

    // Both weapons fire on the press rather than while held, so one tap is one shot.
    private void HandleWeapons(KeyboardState keyboard)
    {
        // The noise follows the shot, not the key, so a press that lands on
        // the cooldown stays quiet.
        if (WasJustPressed(keyboard, Keys.Space) && _player.TryFire(_projectileManager))
            _sounds.PlayShot();

        if (WasJustPressed(keyboard, Keys.Q) && _player.TryMelee(out MeleeStrike strike))
        {
            _sounds.PlayMelee();

            foreach (Mob mob in _mobManager.Mobs)
            {
                if (strike.Area.CollidesWith(mob.Bounds))
                    Hurt(mob, strike.Damage);
            }
        }
    }

    // Damage and the noises that go with it, in one place so a hit sounds the
    // same however it was dealt.
    private void Hurt(Mob mob, int damage)
    {
        // Already down: no second hit, and no second death rattle.
        if (!mob.IsAlive) return;

        mob.TakeDamage(damage);
        _sounds.PlayDamage();

        if (!mob.IsAlive) _sounds.PlayEnemyDeath();
    }

    private void ResolveProjectileHits()
    {
        foreach (Projectile projectile in _projectileManager.Projectiles)
        {
            if (!projectile.IsAlive) continue;

            foreach (Mob mob in _mobManager.Mobs)
            {
                if (!mob.IsAlive || !projectile.Bounds.CollidesWith(mob.Bounds)) continue;

                Hurt(mob, projectile.Damage);

                // A round is spent on the first thing it hits.
                projectile.Kill();
                break;
            }
        }
    }

    private bool WasJustPressed(KeyboardState keyboard, Keys key) =>
        keyboard.IsKeyDown(key) && _previousKeyboard.IsKeyUp(key);

    private void StartRun()
    {
        _state = GameState.Playing;
        _promptLines.Clear();
        _runTime = 0f;
        _player.Revive();
        _player.Position = GraphicsDevice.Viewport.Bounds.Center.ToVector2();
        _mobManager.Reset();
        _projectileManager.Reset();
        _sounds.StartMusic();
    }

    private void FinishRun()
    {
        _state = GameState.GameOver;
        _sounds.StopMusic();

        if (_runTime > _bestTime) _bestTime = _runTime;

        ShowPrompt(
            GameOverMessage,
            $"Time {FormatTime(_runTime)}",
            $"Best {FormatTime(_bestTime)}",
            RestartMessage
        );
    }

    // The running total, centred along the top edge where it stays clear of
    // the player and whatever is closing in on them.
    private void DrawRunTimer()
    {
        string text = FormatTime(_runTime);
        float centerX = GraphicsDevice.Viewport.Bounds.Center.X;

        _spriteBatch.DrawString(
            _font,
            text,
            new Vector2(centerX - _font.MeasureString(text).X / 2f, TimerMargin),
            Color.White
        );
    }

    // One heart per touch the player started with, filled while they still
    // have it. Tucked into the top left, clear of the timer along the top.
    private void DrawHealth()
    {
        float scale = HeartHeight / _heartTexture.Height;
        float step = _heartTexture.Width * scale + HeartGap;

        for (int i = 0; i < Player.MaxTouches; i++)
        {
            _spriteBatch.Draw(
                texture: _heartTexture,
                position: new Vector2(HeartMargin + step * i, HeartMargin),
                sourceRectangle: null,
                color: i < _player.RemainingTouches ? Color.White : Color.White * LostHeartFade,
                rotation: 0f,
                // Drawn from its top left, so the row starts flush with the margin.
                origin: Vector2.Zero,
                scale: scale,
                effects: SpriteEffects.None,
                layerDepth: 0f
            );
        }
    }

    // Minutes and seconds to a tenth, so the timer can be seen moving.
    private static string FormatTime(float seconds) =>
        TimeSpan.FromSeconds(seconds).ToString(@"m\:ss\.f");

    // Centres the lines and the player sprite together as one block, text on
    // top, so a four line prompt sits as evenly as a one line one.
    private void ShowPrompt(params string[] lines)
    {
        Vector2 center = GraphicsDevice.Viewport.Bounds.Center.ToVector2();

        float textHeight = LineGap * (lines.Length - 1);
        foreach (string line in lines) textHeight += _font.MeasureString(line).Y;

        float blockHeight = textHeight + PromptGap + _player.DrawRadius * 2f;
        float top = center.Y - blockHeight / 2f;

        _promptLines.Clear();

        float y = top;
        foreach (string line in lines)
        {
            Vector2 size = _font.MeasureString(line);
            _promptLines.Add((line, new Vector2(center.X - size.X / 2f, y)));
            y += size.Y + LineGap;
        }

        _player.Position = new Vector2(center.X, top + textHeight + PromptGap + _player.DrawRadius);
    }
}
