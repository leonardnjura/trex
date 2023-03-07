using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System.Collections.Generic;
using System.Text;

using TrexRunner.Graphics;

namespace TrexRunner.Entities
{

    public abstract class Obstacle : IGameEntity
    {
        private Trex _trex;

        protected Sprite _sprite;

        public abstract Rectangle CollisionBox { get; }

        // props
        public Vector2 Position { get; protected set; }     // sub classes can set

        public int DrawOrder { get; set; }



        // overloads
        protected Obstacle(Trex trex, Vector2 position)
        {
            _trex = trex;
            Position = position;
        }



        // methods
      
        public abstract void Draw(SpriteBatch spriteBatch, GameTime gameTime);

        public virtual void Update(GameTime gameTime)
        {
            float posX = Position.X - _trex.Speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            float posY = Position.Y;

            Position = new Vector2(posX, posY);

            CheckCollisions();
            
        }

        public void CheckCollisions()
        {
            Rectangle obstacleColllisionBox = CollisionBox;
            Rectangle trexColllisionBox = _trex.CollisionBox;

            if (obstacleColllisionBox.Intersects(trexColllisionBox))
            {
                _trex.Die();
            }

        }
    }
}
