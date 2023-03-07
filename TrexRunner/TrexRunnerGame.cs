using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

using TrexRunner.Entities;
using TrexRunner.Extensions;
using TrexRunner.Graphics;
using TrexRunner.System;

namespace TrexRunner
{

    public class TrexRunnerGame : Game
    {
        public enum DisplayMode
        {
            Default,
            Zoomed
        }

        public const string GAME_TITLE = "T-Rex Runner"; 

        private const string ASSET_NAME_SPRITESHEET = "spritesheet";
        private const string ASSET_NAME_SFX_HIT = "hit";
        private const string ASSET_NAME_SFX_SCORE_REACHED = "score-reached";
        private const string ASSET_NAME_SFX_BUTTON_PRESS = "button-press";

        public const int GAME_WINDOW_HEIGHT_ADJUSTMENT = 50; 

        public const int GAME_WINDOW_WIDTH = 600;
        public const int GAME_WINDOW_HEIGHT = 150 + GAME_WINDOW_HEIGHT_ADJUSTMENT;

        public const int GAME_WINDOW_MARGIN_LEFT = 1;
        public const int GAME_WINDOW_MARGIN_BOTTOM = 16;

        private const int SCORE_BOARD_POS_X = GAME_WINDOW_WIDTH - 130;
        private const int SCORE_BOARD_POS_Y = 10;


        public const int TREX_START_POS_X = 1;
        public const int TREX_START_POS_Y = GAME_WINDOW_HEIGHT - GAME_WINDOW_MARGIN_BOTTOM - Trex.TREX_DEFAULT_SPRITE_HEIGHT;
        private const float FADE_IN_ANIMATION_SPEED = 820f;
        private const string SAVE_FILE_NAME = "Save.dat";

        public const int DISPLAY_ZOOM_FACTOR = 2;

        public static Color GAME_SCENE_COLOR = Color.White;
        public static Color TREX_TINT_COLOR = Color.White;      // HotPink, Crimson

        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private SoundEffect _sfxHit;
        private SoundEffect _sfxButtonPress;
        private SoundEffect _sfxScoreReached;

        private Texture2D _spritesheetTexture;
        private Texture2D _invertedSpritesheet;
        private Texture2D _fadeInTexture;

        private float _fadeInTexturePosX;

        private Trex _trex;
        private ScoreBoard _scoreBoard;

        private InputController _inputController;

        private GroundManager _groundManager;
        private ObstacleManager _obstacleManager;
        private EntityManager _entityManager;
        private SkyManager _skyManager;

        private GameOverOverlay _gameOverOverlay;


        private KeyboardState _previousKeyboardState;

        private DateTime _highScoreDate;

        private Matrix _transformMatrix = Matrix.Identity;

        // props
        public GameState State { get; private set; }

        public DisplayMode WindowDisplayMode { get; set; } = DisplayMode.Default;

        public float ZoomFactor => WindowDisplayMode == DisplayMode.Default ? 1 : DISPLAY_ZOOM_FACTOR;

        // overloads
        public TrexRunnerGame()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            _entityManager = new EntityManager();
            State = GameState.StandBy;
            _fadeInTexturePosX = Trex.TREX_DEFAULT_SPRITE_WIDTH;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            Window.Title = GAME_TITLE;

            _graphics.PreferredBackBufferWidth = GAME_WINDOW_WIDTH;
            _graphics.PreferredBackBufferHeight = GAME_WINDOW_HEIGHT;
            _graphics.SynchronizeWithVerticalRetrace = true;
            _graphics.ApplyChanges();   

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _sfxButtonPress = Content.Load<SoundEffect>(ASSET_NAME_SFX_BUTTON_PRESS);
            _sfxHit = Content.Load<SoundEffect>(ASSET_NAME_SFX_HIT);    
            _sfxScoreReached = Content.Load<SoundEffect>(ASSET_NAME_SFX_SCORE_REACHED);

            _spritesheetTexture = Content.Load<Texture2D>(ASSET_NAME_SPRITESHEET);
            _invertedSpritesheet = _spritesheetTexture.InvertColors(Color.Transparent);
            // Texture2DExt.InvertColors(_spritesheetTexture); // variant

            _fadeInTexture = new Texture2D(GraphicsDevice, 1, 1);
            _fadeInTexture.SetData(new Color[] { GAME_SCENE_COLOR });

            _trex = new Trex(_spritesheetTexture, new Vector2(TREX_START_POS_X, TREX_START_POS_Y), _sfxButtonPress);
            _trex.DrawOrder = 10;
            _trex.JumpComplete += trex_JumpComplete;
            _trex.JumpComplete += (o, e) => Console.WriteLine("Trex jump complete, yay!"); // chuck
            _trex.Died += trex_Died;

            _scoreBoard = new ScoreBoard(_spritesheetTexture, new Vector2(SCORE_BOARD_POS_X, SCORE_BOARD_POS_Y), _trex, _sfxScoreReached);
            //_scoreBoard.Score = 912;
            //_scoreBoard.HighScore = 12345;

            _inputController = new InputController(_trex);

            _groundManager = new GroundManager(_spritesheetTexture, _entityManager, _trex);
            _obstacleManager = new ObstacleManager(_entityManager, _trex, _scoreBoard, _spritesheetTexture);
            _skyManager = new SkyManager(_trex, _spritesheetTexture, _invertedSpritesheet, _entityManager, _scoreBoard);

            _gameOverOverlay = new GameOverOverlay(_spritesheetTexture, this);
            _gameOverOverlay.Position = new Vector2(GAME_WINDOW_WIDTH / 2 - GameOverOverlay.GAME_OVER_SPRITE_WIDTH / 2, GAME_WINDOW_HEIGHT / 2 - 30);

            _entityManager.AddEntity(_trex);    
            _entityManager.AddEntity(_groundManager); 
            _entityManager.AddEntity(_scoreBoard);
            _entityManager.AddEntity(_obstacleManager);
            _entityManager.AddEntity(_gameOverOverlay);
            _entityManager.AddEntity(_skyManager);

            _groundManager.Initialize();

            LoadSaveState();
        }

        private void trex_Died(object sender, EventArgs e)
        {
            State = GameState.GameOver;
            _obstacleManager.IsEnabled = false;
            _gameOverOverlay.IsEnabled = true;

            _sfxHit.Play();

            Debug.WriteLine($"Game over {DateTime.Now}");


            if (_scoreBoard.DisplayScore > _scoreBoard.HighScore)
            {
                Debug.WriteLine("New highscore set: " + _scoreBoard.DisplayScore);
                _scoreBoard.HighScore = _scoreBoard.DisplayScore;
                _highScoreDate = DateTime.Now;

                SaveGame();
            }
        }

        private void trex_JumpComplete(object sender, EventArgs e)
        {
            if(State == GameState.Transition)
            {
                State = GameState.Playing;
                _trex.Initialize();

                _obstacleManager.IsEnabled = true;
            }
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            KeyboardState keyboardState = Keyboard.GetState();

            if (State == GameState.Playing)
                _inputController.ProcessControlls(gameTime);

            else if (State == GameState.Transition)
                _fadeInTexturePosX += (float)gameTime.ElapsedGameTime.TotalSeconds * FADE_IN_ANIMATION_SPEED;

            else if(State == GameState.StandBy)
            {
                bool isStartKeyPressed = keyboardState.IsKeyDown(Keys.Up) || keyboardState.IsKeyDown(Keys.Space);
                bool wasStartKeyPressed = _previousKeyboardState.IsKeyDown(Keys.Up) || _previousKeyboardState.IsKeyDown(Keys.Space);
                
                if(isStartKeyPressed && !wasStartKeyPressed)
                {
                    StartGame();
                }
            
            }

            _entityManager.Update(gameTime);

            // reset hi score
            if(keyboardState.IsKeyDown(Keys.F8) && !_previousKeyboardState.IsKeyDown(Keys.F8))
            {
                ResetSaveState();
            }

            // zoom-in or -out game window
            if (keyboardState.IsKeyDown(Keys.F12) && !_previousKeyboardState.IsKeyDown(Keys.F12))
            {
                ToggleDisplayMode();
            }



            _previousKeyboardState = keyboardState;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            if(_skyManager == null)
                GraphicsDevice.Clear(GAME_SCENE_COLOR); //GAME_SCENE_COLOR
            else
                GraphicsDevice.Clear(_skyManager.ClearColor);

            _spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: _transformMatrix); // pixel snapping


            /* QUICKIE
             * Sprite trexSprite = new Sprite(_spritesheetTexture, 848, 0, 44, 50);
             * trexSprite.Draw(_spriteBatch, new Vector2(10, 40));*/

            _entityManager.Draw(_spriteBatch, gameTime);

            if(State == GameState.StandBy || State == GameState.Transition)
            {
                _spriteBatch.Draw(_fadeInTexture, new Rectangle((int)Math.Round(_fadeInTexturePosX), 0, GAME_WINDOW_WIDTH, GAME_WINDOW_HEIGHT), GAME_SCENE_COLOR);
            }


            _spriteBatch.End();

            base.Draw(gameTime);
        }


        private bool StartGame()
        {
            if(State != GameState.StandBy)
                return false;

            int startScore = 0;
            //if max_score = 99999 && start_score = 0
            //then 99999 - 0 = score_to_unlock_all_levels of game, :|
            //then 99999 - 99900 = score_to_unlock_all_levels of game, :)
            _scoreBoard.Score = startScore;        
            State = GameState.Transition;
            _trex.BeginJump();

            return true;
        }

        public bool Replay()
        {
            if(State != GameState.GameOver)
                return false;

            State = GameState.Playing;
            _trex.Initialize();
            _obstacleManager.Reset();
            _obstacleManager.IsEnabled = true;
            _gameOverOverlay.IsEnabled = false;
            _scoreBoard.Score = 0;
            _groundManager.Initialize();
            _inputController.BlockInputsTemporarily(); 

            return true;

        }

        public void SaveGame()
        {
            SaveState saveState = new SaveState()               // object initialization syntax
            {
                HighScore = _scoreBoard.HighScore,
                HighScoreDate = _highScoreDate
            };

            try
            {
                using(FileStream fs = new FileStream(SAVE_FILE_NAME, FileMode.Create))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(fs, saveState);
                }

            }
            catch (Exception e)
            {
                Debug.WriteLine("Ayayai, an error occured while saving game to disk ::" + e.Message);
            }

        }


        public void LoadSaveState()
        {
            try
            {
                using (FileStream fs = new FileStream(SAVE_FILE_NAME, FileMode.OpenOrCreate))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    SaveState  saveState = formatter.Deserialize(fs) as SaveState;

                    if (saveState != null)
                    {
                        if(_scoreBoard != null)
                            _scoreBoard.HighScore = saveState.HighScore;

                        _highScoreDate = saveState.HighScoreDate;

                        Debug.WriteLine($"\n\t\t::::::: hi score {saveState.HighScore} set {saveState.HighScoreDate} :::::::\n");

                    }
                }

            }
            catch (Exception e)
            {
                Debug.WriteLine("Ayayai, an error occured while loading game from disk ::" + e.Message);
            }
        }


        public void ResetSaveState()
        {
            _scoreBoard.HighScore = 0;
            _highScoreDate = default(DateTime);

            SaveGame();
        }


        public void ToggleDisplayMode()
        {
            if(WindowDisplayMode == DisplayMode.Default)
            {
                WindowDisplayMode = DisplayMode.Zoomed;
                _graphics.PreferredBackBufferHeight = GAME_WINDOW_HEIGHT * DISPLAY_ZOOM_FACTOR;
                _graphics.PreferredBackBufferWidth = GAME_WINDOW_WIDTH * DISPLAY_ZOOM_FACTOR;
                _transformMatrix = Matrix.Identity * Matrix.CreateScale(DISPLAY_ZOOM_FACTOR, DISPLAY_ZOOM_FACTOR, 1);

            }
            else
            {
                WindowDisplayMode = DisplayMode.Default;
                _graphics.PreferredBackBufferHeight = GAME_WINDOW_HEIGHT;
                _graphics.PreferredBackBufferWidth = GAME_WINDOW_WIDTH;
                _transformMatrix = Matrix.Identity;

            }

            _graphics.ApplyChanges();
        }
    }
}
