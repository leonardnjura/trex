using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System;
using System.Collections.Generic;

namespace TrexRunner.Entities
{
    public class ObstacleManager : IGameEntity
    {
        private static readonly int[] PTERODACTYL_Y_POSITIONS = 
            new int[] { 
                90 + TrexRunnerGame.GAME_WINDOW_HEIGHT_ADJUSTMENT, 
                62 + TrexRunnerGame.GAME_WINDOW_HEIGHT_ADJUSTMENT, 
                24 + TrexRunnerGame.GAME_WINDOW_HEIGHT_ADJUSTMENT 
            };

        private const float MIN_SPAWN_DISTANCE = 10;            // free points, :)

        private const int MIN_OBSTACLE_DISTANCE = 8;
        private const int MAX_OBSTACLE_DISTANCE = 28;

        private const int OBSTACLE_DISTANCE_SPEED_TOLERANCE = 5; 

        private const int LARGE_CACTUS_POS_Y = 80 + TrexRunnerGame.GAME_WINDOW_HEIGHT_ADJUSTMENT;
        private const int SMALL_CACTUS_POS_Y = 94 + TrexRunnerGame.GAME_WINDOW_HEIGHT_ADJUSTMENT;

        private const int OBSTACLE_DRAW_ORDER = 10;             // trex z-index
        private const int OBSTACLE_DRAW_ORDER_TOP = 12;         
        private const int OBSTACLE_DRAW_ORDER_BOTTOM = 9;

        private const int OBSTACLE_DESPAWN_POS_X = -200;
        private const int PTERODACTYL_SPAWN_SCORE_MIN = 150;

        private double _lastSpawnScore = -1;                    // score at the last spawn
        private double _currentTargetDistance;                  // random distance between last and next obstacles

        private readonly EntityManager _entityManager;
        private readonly Trex _trex;
        private readonly ScoreBoard _scoreBoard;

        private readonly Random _random;

        private Texture2D _spriteSheet;


        // props
        public bool IsEnabled { get; set; }
        public bool CanSpawnObstacles => IsEnabled && _scoreBoard.Score >= MIN_SPAWN_DISTANCE;
        public int DrawOrder => 0;


        // overloads
        public ObstacleManager(EntityManager entityManager, Trex trex, ScoreBoard scoreBoard, Texture2D spriteSheet)
        {
            _entityManager = entityManager;
            _trex = trex;
            _scoreBoard = scoreBoard;
            _random = new Random();
            _spriteSheet = spriteSheet;

        }


        // methods
        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {

        }

        public void Update(GameTime gameTime)
        {
            if(!IsEnabled)
                return;

            if(CanSpawnObstacles && (_lastSpawnScore <= 0 || (_scoreBoard.Score - _lastSpawnScore >= _currentTargetDistance)))
            {
                _currentTargetDistance = _random.NextDouble() 
                    * (MAX_OBSTACLE_DISTANCE - MIN_OBSTACLE_DISTANCE) + MIN_OBSTACLE_DISTANCE;

                // increase gaps between obstacles at high speeds
                _currentTargetDistance += (_trex.Speed - Trex.START_SPEED) / (Trex.MAX_SPEED - Trex.START_SPEED) * OBSTACLE_DISTANCE_SPEED_TOLERANCE;

                _lastSpawnScore = _scoreBoard.Score;

                SpawnRandomObstacle();
            }

            // destroy obstacles that have left the lhs of the game screen
            foreach (Obstacle obstacle in _entityManager.GetEntitiesOfType<Obstacle>())
            {
                if(obstacle.Position.X < OBSTACLE_DESPAWN_POS_X)
                    _entityManager.RemoveEntity(obstacle);

            }
            


        }

        private void SpawnRandomObstacle()
        {
            // TODO: create instance of obstacle and add it to entity manager

            Obstacle obstacle = null;

            int cactusGroupSpawnRate = 75;
            int pterodactylSpawnRate = _scoreBoard.Score >= PTERODACTYL_SPAWN_SCORE_MIN ? 25 : 0;

            int rng = _random.Next(0, cactusGroupSpawnRate + pterodactylSpawnRate + 1);     //0-75 or 0-100

            bool isOnTopOfTrex = _random.NextDouble() > 0.5f;
            int drawOrder = isOnTopOfTrex ? OBSTACLE_DRAW_ORDER_TOP : OBSTACLE_DRAW_ORDER_BOTTOM;

            if(rng <= cactusGroupSpawnRate)
            {

                CactusGroup.GroupSize randomGroupSize = (CactusGroup.GroupSize)_random.Next((int)CactusGroup.GroupSize.Small, (int)CactusGroup.GroupSize.Large + 1);

                bool isLarge = _random.NextDouble() > 0.5f;

                float posY = isLarge ? LARGE_CACTUS_POS_Y : SMALL_CACTUS_POS_Y;

                obstacle = new CactusGroup(_spriteSheet, isLarge, randomGroupSize, _trex, new Vector2(TrexRunnerGame.GAME_WINDOW_WIDTH, posY));
            }
            else
            {
                int verticalPosIndex = _random.Next(0, PTERODACTYL_Y_POSITIONS.Length);
                float posY = PTERODACTYL_Y_POSITIONS[verticalPosIndex];
                obstacle = new Pterodactyl(_trex, new Vector2(TrexRunnerGame.GAME_WINDOW_WIDTH, posY), _spriteSheet);

            }

            // z-index relative to trex, guided by trex feet vs cactus posY/contact on the sand
            // if same level randomize to make trex feel immersed, :)
            // obstacle.DrawOrder = OBSTACLE_DRAW_ORDER;
            obstacle.DrawOrder = drawOrder;

            _entityManager.AddEntity(obstacle);
        
        }

        public void Reset()
        {
            foreach(Obstacle obstacle in _entityManager.GetEntitiesOfType<Obstacle>())
            {
                _entityManager.RemoveEntity(obstacle);
            }

            _currentTargetDistance = 0;
            _lastSpawnScore = -1;
        }
    }
}
