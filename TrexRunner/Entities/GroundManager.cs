using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TrexRunner.Graphics;

namespace TrexRunner.Entities
{

    public class GroundManager : IGameEntity
    {
        private const float GROUND_TILE_POS_Y = 121 + TrexRunnerGame.GAME_WINDOW_HEIGHT_ADJUSTMENT;

        private const int SPRITE_WIDTH = 600;
        private const int SPRITE_HEIGHT = 14;

        private const int SPRITE_POS_X = 2;
        private const int SPRITE_POS_Y = 54;

        private Texture2D _spriteSheet;
        private EntityManager _entityManager;

        private readonly List<GroundTile> _groundTiles;

        private Sprite _regularSprite;
        private Sprite _bumpySprite;

        private Trex _trex;

        private Random _random;


        // props
        public int DrawOrder { get; set; }


        // overloads
        public GroundManager(Texture2D spriteSheet, EntityManager entityManager, Trex trex)
        {
            _spriteSheet = spriteSheet;
            _entityManager = entityManager;
            _trex = trex;

            _groundTiles = new List<GroundTile>();
            _regularSprite = new Sprite(spriteSheet, SPRITE_POS_X, SPRITE_POS_Y, SPRITE_WIDTH, SPRITE_HEIGHT);
            _bumpySprite = new Sprite(spriteSheet, SPRITE_POS_X + SPRITE_WIDTH, SPRITE_POS_Y, SPRITE_WIDTH, SPRITE_HEIGHT);

            _random = new Random();
        }


        // methods
        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            
        }

        public void Update(GameTime gameTime)
        {
            if(_groundTiles.Any())
            {
                float maxPosX = _groundTiles.Max(g => g.PositionX);

                if (maxPosX < 0)
                    SpawnTile(maxPosX);

            }



            List<GroundTile> tilesToRemove = new List<GroundTile>();
           
            foreach (GroundTile tile in _groundTiles)
            {
                tile.PositionX -= _trex.Speed * (float)gameTime.ElapsedGameTime.TotalSeconds;

                if(tile.PositionX < -SPRITE_WIDTH)
                {
                    _entityManager.RemoveEntity(tile);
                    tilesToRemove.Add(tile);
                }
            }

            // safe chucky
            foreach (GroundTile tile in tilesToRemove)
                _groundTiles.Remove(tile);
        }

        public void Initialize()
        {
            _groundTiles.Clear();

            foreach (GroundTile tile in _entityManager.GetEntitiesOfType<GroundTile>())
            {
                _entityManager.RemoveEntity(tile);
            }

            GroundTile groundTile = CreateRegularTile(0);
            _groundTiles.Add(groundTile);

            _entityManager.AddEntity(groundTile);
        }

        private GroundTile CreateRegularTile(float positionX)
        {
            GroundTile groundTile = new GroundTile(positionX, GROUND_TILE_POS_Y, _regularSprite);
            
            return groundTile;
        }

        private GroundTile CreateBumbyTile(float positionX)
        {
            GroundTile groundTile = new GroundTile(positionX, GROUND_TILE_POS_Y, _bumpySprite);
            
            return groundTile;
        }

        private void SpawnTile(float maxPosX)
        {
            double randomNumber = _random.NextDouble();

            float posX = maxPosX + SPRITE_WIDTH;

            GroundTile groundTile;

            if(randomNumber < 0.5)
                groundTile = CreateRegularTile(posX);
            else
                groundTile = CreateBumbyTile(posX);

            _entityManager.AddEntity(groundTile);
            _groundTiles.Add(groundTile);
        }
    }
}
