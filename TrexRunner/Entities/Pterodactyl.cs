using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System;
using System.Collections.Generic;
using System.Text;

using TrexRunner.Graphics;

namespace TrexRunner.Entities
{
    public class Pterodactyl : Obstacle
    {

        private const int TEXTURE_COORDS_X = 134;
        private const int TEXTURE_COORDS_Y = 0;

        private const int SPRITE_WIDTH = 46;
        private const int SPRITE_HEIGHT = 42;

        private const float ANIMATION_FRAME_LENGTH = 0.2f;

        private const int VERTICAL_COLLISION_INSET = 10;
        private const int HORIZONTAL_COLLISION_INSET = 6;
        private const int SPEED_PPS = 80;

        private SpriteAnimation _animation;
        private Trex _trex;

        // props
        public override Rectangle CollisionBox
        {
            get
            {
                Rectangle box = new Rectangle((int)Math.Round(Position.X), (int)Math.Round(Position.Y), SPRITE_WIDTH, SPRITE_HEIGHT);
                box.Inflate(-HORIZONTAL_COLLISION_INSET, -VERTICAL_COLLISION_INSET);
                return box;
            
            }
        }


        // overloads
        public Pterodactyl(Trex trex, Vector2 position, Texture2D spriteSheet) : base(trex, position)
        {
            _trex = trex;

            Sprite spriteA = new Sprite(spriteSheet, TEXTURE_COORDS_X, TEXTURE_COORDS_Y, SPRITE_WIDTH, SPRITE_HEIGHT);
            Sprite spriteB = new Sprite(spriteSheet, TEXTURE_COORDS_X + SPRITE_WIDTH, TEXTURE_COORDS_Y, SPRITE_WIDTH, SPRITE_HEIGHT);

            _animation = new SpriteAnimation();
            _animation.AddFrame(spriteA, 0);
            _animation.AddFrame(spriteB, ANIMATION_FRAME_LENGTH);
            _animation.AddFrame(spriteA, ANIMATION_FRAME_LENGTH * 2);   // dummy frame to indicate end of animation
            _animation.ShouldLoop = true;
            _animation.Play();
        }

        // methods
        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            _animation.Draw(spriteBatch, Position);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if(_trex.IsAlive)
            {
                //pterodactyl moves master that trex/game speed
                Position = new Vector2(Position.X - SPEED_PPS * (float)gameTime.ElapsedGameTime.TotalSeconds, Position.Y);
                _animation.Update(gameTime);
            }
        }
    }
}
