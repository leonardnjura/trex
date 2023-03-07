using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System;

namespace TrexRunner.Entities
{
    public abstract class SkyObject : IGameEntity
    {

        protected readonly Trex _trex;      // only kidos shall access


        // props
        public int DrawOrder { get; set; }

        public abstract float Speed { get; }

        public Vector2 Position { get; set; }



        // overloads
        protected SkyObject(Trex trex, Vector2 position)
        {
            _trex = trex;
            Position = position;    
        }


        // methods
        public abstract void Draw(SpriteBatch spriteBatch, GameTime gameTime);

        public virtual void Update(GameTime gameTime)
        {
            if(_trex.IsAlive) // only change position if trex aint dead already
                Position = new Vector2(Position.X - Speed *(float)gameTime.ElapsedGameTime.TotalSeconds, Position.Y);
        }
    }
}
