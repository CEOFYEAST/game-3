using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace game_1.Particles;

public class ParticleManager
{
    // Once this many are in the air new ones are dropped, which nobody can spot
    // in a burst this size.
    private const int MaxParticles = 1024;

    // A shard is drawn this much narrower than it is long, so its spin shows.
    private const float ShardAspect = 0.35f;

    // Share of a particle's speed left after each second, so bursts burst out
    // and then hang in the air as they fade.
    private const float SpeedKeptPerSecond = 0.05f;

    // Each particle lives for somewhere between these, in seconds.
    private const float MinLife = 0.25f;
    private const float MaxLife = 0.6f;

    // Fastest a burst shard turns, in radians per second, either way.
    private const float MaxSpin = 12f;

    // A particle sets off at somewhere between this share of the speed asked
    // for and all of it, so a burst spreads out instead of forming a ring.
    private const float MinSpeedShare = 0.25f;

    private struct Particle
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public float Rotation;
        public float Spin;
        public float Age;
        public float Life;
        public float Size;
        public Color Color;
    }

    // A flat array, as these come and go by the dozen every few frames. The
    // live ones are always the first _count entries.
    private readonly Particle[] _particles = new Particle[MaxParticles];

    private readonly Random _random = new();

    // One white pixel, made here rather than loaded so the effect needs no
    // asset. Stretched into a shard and tinted when drawn.
    private readonly Texture2D _pixel;

    private int _count;

    public ParticleManager(GraphicsDevice graphicsDevice)
    {
        _pixel = new Texture2D(graphicsDevice, 1, 1);
        _pixel.SetData([Color.White]);
    }

    // Clears anything still in the air so a new run starts empty.
    public void Reset()
    {
        _count = 0;
    }

    // Tumbling shards thrown out evenly in every direction.
    public void Burst(Vector2 position, Color color, int count, float speed, float size)
    {
        for (int i = 0; i < count; i++)
        {
            float angle = RandomBetween(0f, MathHelper.TwoPi);
            float spin = RandomBetween(-MaxSpin, MaxSpin);

            Spawn(position, angle, RandomBetween(0f, MathHelper.TwoPi), spin, color, speed, size);
        }
    }

    // Sparks thrown out across spread radians, centred on angle. Each one points
    // the way it is flying and does not turn, so the fan reads as streaks.
    public void Cone(Vector2 position, float angle, float spread, Color color, int count, float speed, float size)
    {
        for (int i = 0; i < count; i++)
        {
            float direction = angle + RandomBetween(-spread / 2f, spread / 2f);

            Spawn(position, direction, direction, 0f, color, speed, size);
        }
    }

    public void Update(GameTime gameTime)
    {
        float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
        float speedKept = MathF.Pow(SpeedKeptPerSecond, elapsed);

        // Backwards, as a finished particle is replaced by the one from the end
        // of the live range, which has already been updated by this point.
        for (int i = _count - 1; i >= 0; i--)
        {
            ref Particle particle = ref _particles[i];

            particle.Age += elapsed;

            if (particle.Age >= particle.Life)
            {
                _count--;
                particle = _particles[_count];
                continue;
            }

            particle.Position += particle.Velocity * elapsed;
            particle.Velocity *= speedKept;
            particle.Rotation += particle.Spin * elapsed;
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        for (int i = 0; i < _count; i++)
        {
            ref readonly Particle particle = ref _particles[i];

            spriteBatch.Draw(
                texture: _pixel,
                position: particle.Position,
                sourceRectangle: null,
                // Fades out over its life, so overlapping shards blend into
                // each other and none of them blink off.
                color: particle.Color * (1f - particle.Age / particle.Life),
                rotation: particle.Rotation,
                origin: new Vector2(0.5f, 0.5f),
                // The texture is one pixel, so the scale is the size in pixels.
                scale: new Vector2(particle.Size, particle.Size * ShardAspect),
                effects: SpriteEffects.None,
                layerDepth: 0f
            );
        }
    }

    private void Spawn(Vector2 position, float direction, float rotation, float spin, Color color, float speed, float size)
    {
        if (_count == MaxParticles) return;

        float launchSpeed = speed * RandomBetween(MinSpeedShare, 1f);

        _particles[_count++] = new Particle
        {
            Position = position,
            Velocity = new Vector2(MathF.Cos(direction), MathF.Sin(direction)) * launchSpeed,
            Rotation = rotation,
            Spin = spin,
            Life = RandomBetween(MinLife, MaxLife),
            Size = size,
            Color = color
        };
    }

    private float RandomBetween(float min, float max) =>
        min + (float)_random.NextDouble() * (max - min);
}
