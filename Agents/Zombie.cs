using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace game_1.Agents;

public class Zombie : Mob
{
    // Dies to a single hit for now; raise this for a tougher zombie.
    private const int StartingHealth = 1;

    public Zombie(Vector2 initialPosition) : base(initialPosition, StartingHealth) { }

    protected override float Scale => 0.25f;

    protected override float Speed => 60f;

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
