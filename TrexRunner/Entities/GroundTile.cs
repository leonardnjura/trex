﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using TrexRunner.Graphics;

namespace TrexRunner.Entities
{
    public class GroundTile : IGameEntity
    {
        private float _positionY;

        // props
        public float PositionX { get; set; }
        public Sprite Sprite { get; }
        public int DrawOrder { get; set; }


        // overloads
        public GroundTile(float positionX, float positionY, Sprite sprite)
        {
            PositionX = positionX;
            _positionY = positionY;
            Sprite = sprite;
        }


        // methods
        public void Update(GameTime gameTime)
        {
            
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            Sprite.Draw(spriteBatch, new Vector2(PositionX, _positionY));
        }
    }
}
