using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace game_1.Agents;

public class Tick : Mob
{
    // Dies to a single hit for now; raise this for a tougher tick.
    private const int StartingHealth = 1;

    public Tick(Vector2 initialPosition) : base(initialPosition, StartingHealth) { }

    protected override float Scale => 0.15f;

    // Ticks close in twice as fast as zombies.
    protected override float Speed => 120f;

    public override void LoadContent(ContentManager content)
    {
        _texture = content.Load<Texture2D>("EnemyPlaceholder");
    }

    public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(
            texture: _texture,
            position: Position,
            sourceRectangle: null,
            color: Color,
            rotation: 0f,
            origin: new Vector2(_texture.Width / 2f, _texture.Height / 2f),
            scale: Scale,
            effects: SpriteEffects.None,
            layerDepth: 0f
        );
    }
}
